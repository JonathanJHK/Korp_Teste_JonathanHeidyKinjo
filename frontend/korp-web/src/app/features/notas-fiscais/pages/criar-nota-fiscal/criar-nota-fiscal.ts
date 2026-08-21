import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';

import { LoadingService } from '@core/services/loading.service';
import { LoggerService } from '@core/services/logger.service';
import { iProdutoResponseDTO } from '../../../produtos/models/produto.model';
import { ProdutoService } from '../../../produtos/services/produto.service';
import { iNotaFiscalCriarDTO } from '../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../services/nota-fiscal.service';

type ItemNotaFiscalForm = FormGroup<{
  produtoId: FormControl<number | null>;
  quantidade: FormControl<number>;
}>;

@Component({
  selector: 'app-criar-nota-fiscal',
  imports: [ReactiveFormsModule, RouterLink, ButtonModule, InputNumberModule, SelectModule],
  templateUrl: './criar-nota-fiscal.html',
  styleUrl: './criar-nota-fiscal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriarNotaFiscal implements OnInit {
  // Serviços usados para consultar produtos, cadastrar a nota, controlar o loading,
  // registrar eventos, exibir mensagens e navegar entre as páginas.
  private readonly produtoService = inject(ProdutoService);
  private readonly notaFiscalService = inject(NotaFiscalService);
  private readonly loadingService = inject(LoadingService);
  private readonly logger = inject(LoggerService);
  private readonly messageService = inject(MessageService);
  private readonly router = inject(Router);

  // Lista de produtos disponíveis para seleção no formulário.
  protected readonly produtos = signal<iProdutoResponseDTO[]>([]);
  // Indica se o cadastro da nota fiscal está sendo enviado.
  protected readonly enviando = signal(false);

  // Formulário principal da nota fiscal, iniciado com um item vazio.
  protected readonly form = new FormGroup({
    itens: new FormArray<ItemNotaFiscalForm>([this.criarItemForm()]),
  });

  // Atalho para acessar o FormArray de itens sem repetir o caminho completo.
  protected get itens(): FormArray<ItemNotaFiscalForm> {
    return this.form.controls.itens;
  }

  // Carrega os produtos assim que a tela é inicializada.
  ngOnInit(): void {
    this.carregarProdutos();
  }

  // Adiciona uma nova linha de produto ao formulário.
  protected adicionarItem(): void {
    this.itens.push(this.criarItemForm());
  }

  // Remove uma linha, mas mantém pelo menos um item no formulário.
  protected removerItem(index: number): void {
    if (this.itens.length === 1) return;

    this.itens.removeAt(index);
  }

  // Atualiza a validação da quantidade quando o produto selecionado muda.
  protected aoSelecionarProduto(index: number): void {
    // Obtém o item alterado e o identificador do produto selecionado.
    const itemForm = this.itens.at(index);
    const produtoId = itemForm.controls.produtoId.value;

    // Busca os dados do produto para conhecer o saldo disponível.
    const produto = this.produtos().find((item) => item.id === produtoId);

    // A quantidade não pode ser menor que um nem maior que o saldo em estoque.
    const quantidadeControl = itemForm.controls.quantidade;

    quantidadeControl.setValidators([
      Validators.required,
      Validators.min(1),
      Validators.max(produto?.saldo ?? 0),
    ]);

    // Recalcula imediatamente o estado de validade do campo.
    quantidadeControl.updateValueAndValidity();
  }

  // Retorna o saldo disponível do produto selecionado para exibição na tela.
  protected obterSaldoProduto(index: number): number {
    const produtoId = this.itens.at(index).controls.produtoId.value;

    return this.produtos().find((produto) => produto.id === produtoId)?.saldo ?? 0;
  }

  // Valida os dados e envia a nova nota fiscal para a API.
  protected cadastrar(): void {
    // Interrompe o envio quando algum campo obrigatório é inválido.
    if (this.form.invalid) {
      // Exibe os erros dos campos mesmo que ainda não tenham sido tocados.
      this.form.markAllAsTouched();

      this.messageService.add({
        severity: 'warn',
        summary: 'Formulário inválido',
        detail: 'Revise os produtos e as quantidades informadas.',
      });

      return;
    }

    // Evita que o mesmo produto seja incluído mais de uma vez na nota.
    if (this.possuiProdutosDuplicados()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Produto duplicado',
        detail: 'Um mesmo produto não pode aparecer mais de uma vez.',
      });

      return;
    }

    // Lê os valores preenchidos no formulário.
    const dadosFormulario = this.form.getRawValue();

    // Converte os dados do formulário para o DTO esperado pelo backend.
    const payload: iNotaFiscalCriarDTO = {
      itens: dadosFormulario.itens.map((item) => ({
        produtoId: item.produtoId!,
        quantidade: item.quantidade,
      })),
    };

    // Bloqueia novas ações e exibe o indicador de carregamento durante o envio.
    this.enviando.set(true);
    this.loadingService.start();

    // Envia a nota e garante a limpeza do estado ao terminar, com sucesso ou erro.
    this.notaFiscalService
      .cadastrar(payload)
      .pipe(
        finalize(() => {
          // Sempre libera o formulário e encerra o loading ao final da requisição.
          this.enviando.set(false);
          this.loadingService.stop();
        }),
      )
      .subscribe({
        // Processa a resposta quando a nota é cadastrada com sucesso.
        next: (notaFiscal) => {
          this.logger.info('CriarNotaFiscal: nota cadastrada', notaFiscal.id);

          this.messageService.add({
            severity: 'success',
            summary: 'Nota fiscal cadastrada',
            detail: `A nota fiscal nº ${notaFiscal.numero} foi criada como Aberta.`,
          });

          void this.router.navigate(['/notas-fiscais']);
        },
        // Encaminha falhas para mensagens específicas da operação.
        error: (error) => {
          this.tratarErroCadastro(error);
        },
      });
  }

  // Cria e configura o FormGroup que representa um item da nota fiscal.
  private criarItemForm(): ItemNotaFiscalForm {
    // Produto é obrigatório e começa sem seleção.
    return new FormGroup({
      produtoId: new FormControl<number | null>(null, {
        validators: [Validators.required],
      }),
      // Quantidade começa em um e deve ser positiva.
      quantidade: new FormControl(1, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(1)],
      }),
    });
  }

  // Consulta os produtos disponíveis para preencher os campos de seleção.
  private carregarProdutos(): void {
    // Exibe o loading enquanto a consulta estiver em andamento.
    this.loadingService.start();

    this.produtoService
      .listar()
      .pipe(finalize(() => this.loadingService.stop()))
      .subscribe({
        // Atualiza a lista quando os produtos são carregados.
        next: (produtos) => {
          this.produtos.set(produtos);

          // Informa que é necessário cadastrar um produto antes de criar a nota.
          if (produtos.length === 0) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Nenhum produto',
              detail: 'Cadastre pelo menos um produto antes de criar uma nota fiscal.',
            });
          }
        },
        // Informa ao usuário quando o Estoque não está disponível.
        error: (error) => {
          this.logger.error('CriarNotaFiscal: erro ao carregar produtos', error);

          this.messageService.add({
            severity: 'error',
            summary: 'Estoque indisponível',
            detail: 'Não foi possível carregar os produtos.',
          });
        },
      });
  }

  // Verifica se algum produto foi repetido entre os itens preenchidos.
  private possuiProdutosDuplicados(): boolean {
    // Obtém apenas IDs preenchidos, removendo valores nulos.
    const produtosIds = this.form
      .getRawValue()
      .itens.map((item) => item.produtoId)
      .filter((produtoId): produtoId is number => produtoId !== null);

    // Um Set elimina duplicidades; tamanhos diferentes indicam repetição.
    return new Set(produtosIds).size !== produtosIds.length;
  }

  // Traduz os principais erros da API em mensagens compreensíveis para o usuário.
  private tratarErroCadastro(error: HttpErrorResponse): void {
    // Mantém os detalhes técnicos no log para diagnóstico.
    this.logger.error('CriarNotaFiscal: erro ao cadastrar nota', error);

    // Mensagem padrão usada quando o erro não possui um tratamento específico.
    let detail = 'Não foi possível cadastrar a nota fiscal.';

    // Falha de rede ou serviço inacessível.
    if (error.status === 0) {
      detail = 'Não foi possível estabelecer comunicação com o Faturamento.';
      // Dados inválidos enviados para a API.
    } else if (error.status === 400) {
      detail = error.error?.detail ?? 'Os dados informados são inválidos.';
      // Produto duplicado ou outro conflito de cadastro.
    } else if (error.status === 409) {
      detail = error.error?.detail ?? 'A nota contém produtos duplicados ou dados conflitantes.';
      // Serviço de Estoque indisponível para validar os produtos.
    } else if (error.status === 503) {
      detail = error.error?.detail ?? 'O serviço de Estoque está indisponível.';
      // Usa o detalhe retornado pelo backend quando ele estiver disponível.
    } else if (error.error?.detail) {
      detail = error.error.detail;
    }

    this.messageService.add({
      severity: 'error',
      summary: 'Erro no cadastro',
      detail,
    });
  }
}
