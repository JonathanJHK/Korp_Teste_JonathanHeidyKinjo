import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, model, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

// Serviço de mensagens e componentes visuais utilizados no template.
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';

// Diretiva que aplica a máscara de entrada ao código do produto.
import { NgxMaskDirective } from 'ngx-mask';
// Serviços da aplicação e modelos usados pelo cadastro.
import { LoadingService } from '@core/services/loading.service';
import { LoggerService } from '@core/services/logger.service';
import { iProdutoCriarDTO, iProdutoResponseDTO } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';

@Component({
  selector: 'app-cadastrar-produto-modal',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    NgxMaskDirective,
  ],
  templateUrl: './cadastrar-produto-modal.html',
  styleUrl: './cadastrar-produto-modal.scss',
  // Atualiza a view somente quando necessário, melhorando a eficiência do componente.
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CadastrarProdutoModal {
  private readonly formBuilder = inject(FormBuilder);
  private readonly produtoService = inject(ProdutoService);
  private readonly loadingService = inject(LoadingService);
  private readonly logger = inject(LoggerService);
  private readonly messageService = inject(MessageService);

  // Controla a abertura e o fechamento do diálogo pelo componente pai.
  readonly visible = model(false);

  // Notifica o componente pai quando um produto foi cadastrado.
  readonly produtoCadastrado = output<iProdutoResponseDTO>();

  // Indica se existe uma requisição de cadastro em andamento.
  protected readonly enviando = signal(false);

  // Formulário reativo com os campos e regras de validação do produto.
  protected readonly form = this.formBuilder.nonNullable.group({
    // Exige quatro letras, hífen e pelo menos três números.
    codigo: [
      '',
      [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Za-z]{4}-\d{3,}$/)],
    ],
    descricao: ['', [Validators.required, Validators.maxLength(200)]],
    saldo: [0, [Validators.required, Validators.min(0)]],
  });

  // Valida o formulário e envia o produto para a API.
  protected cadastrar(): void {
    // Impede o envio quando algum campo é inválido.
    if (this.form.invalid) {
      // Exibe os erros dos campos mesmo que o usuário ainda não tenha interagido com eles.
      this.form.markAllAsTouched();

      // Informa ao usuário que os dados precisam ser corrigidos.
      this.messageService.add({
        severity: 'warn',
        summary: 'Formulário inválido',
        detail: 'Preencha corretamente os campos obrigatórios.',
      });

      return;
    }

    // Obtém os valores atuais do formulário no formato esperado pela API.
    const payload: iProdutoCriarDTO = this.form.getRawValue();

    // Desabilita ações concorrentes e exibe o loading durante o cadastro.
    this.enviando.set(true);
    this.loadingService.start();

    // Executa a requisição e garante a limpeza do estado ao final dela.
    this.produtoService
      .cadastrar(payload)
      .pipe(
        finalize(() => {
          // Executado tanto quando a requisição tem sucesso quanto quando falha.
          this.enviando.set(false);
          this.loadingService.stop();
        }),
      )
      .subscribe({
        // Executado quando a API cadastra o produto com sucesso.
        next: (produto) => {
          this.logger.info('CadastrarProdutoModal: produto cadastrado', produto.id);

          this.messageService.add({
            severity: 'success',
            summary: 'Produto cadastrado',
            detail: `${produto.descricao} foi cadastrado com sucesso.`,
          });

          this.produtoCadastrado.emit(produto);

          // Altera a visibilidade e limpa os dados antigos do formulário.
          this.visible.set(false);
          this.limparFormulario();
        },
        // Executado quando a requisição falha.
        error: (error) => {
          this.tratarErro(error);
        },
      });
  }

  // Fecha o modal somente quando não existe cadastro em andamento.
  protected fechar(): void {
    if (this.enviando()) return;

    // Altera a visibilidade e limpa os dados antigos do formulário.
    this.visible.set(false);
    this.limparFormulario();
  }

  // Restaura o formulário para os valores iniciais.
  protected limparFormulario(): void {
    this.form.reset({
      codigo: '',
      descricao: '',
      saldo: 0,
    });
  }

  // Converte os principais erros da API em mensagens compreensíveis.
  private tratarErro(error: HttpErrorResponse): void {
    // Registra o erro técnico para investigação sem expô-lo diretamente ao usuário.
    this.logger.error('CadastrarProdutoModal: erro ao cadastrar', error);

    // Status 0 normalmente indica falha de rede ou serviço indisponível.
    if (error.status === 0) {
      this.messageService.add({
        severity: 'error',
        summary: 'Estoque indisponível',
        detail: 'Não foi possível estabelecer comunicação com o Estoque.',
      });

      return;
    }

    // Status 409 indica conflito: o código do produto já existe.
    if (error.status === 409) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Código já cadastrado',
        detail: 'Já existe um produto com esse código.',
      });

      return;
    }

    // Mensagem de fallback para erros não previstos especificamente.
    this.messageService.add({
      severity: 'error',
      summary: 'Erro',
      detail: 'Não foi possível cadastrar o produto.',
    });
  }
}
