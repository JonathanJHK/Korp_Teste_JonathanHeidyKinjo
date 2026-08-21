import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { HttpErrorResponse } from '@angular/common/http';
import { LoadingService } from '@core/services/loading.service';
import { LoggerService } from '@core/services/logger.service';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { DetalhesNotaFiscalModal } from '../../components/detalhes-nota-fiscal-modal/detalhes-nota-fiscal-modal';
import { NotaFiscalImpressao } from '../../components/nota-fiscal-impressao/nota-fiscal-impressao';
import { iNotaFiscalResponseDTO, StatusNotaFiscal } from '../../models/nota-fiscal.model';
import { NotaFiscalService } from '../../services/nota-fiscal.service';

@Component({
  selector: 'app-listar-notas-fiscais',
  imports: [
    DatePipe,
    RouterLink,
    ButtonModule,
    TableModule,
    TagModule,
    DetalhesNotaFiscalModal,
    ConfirmDialog,
    TooltipModule,
    NotaFiscalImpressao,
  ],
  templateUrl: './listar-notas-fiscais.html',
  styleUrl: './listar-notas-fiscais.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListarNotasFiscais implements OnInit {
  private readonly notaFiscalService = inject(NotaFiscalService);
  private readonly loadingService = inject(LoadingService);
  private readonly logger = inject(LoggerService);
  private readonly messageService = inject(MessageService);
  private readonly confirmationService = inject(ConfirmationService);

  protected readonly notasFiscais = signal<iNotaFiscalResponseDTO[]>([]);
  protected readonly notaFiscalSelecionada = signal<iNotaFiscalResponseDTO | null>(null);

  protected readonly notaFiscalParaImpressao = signal<iNotaFiscalResponseDTO | null>(null);

  protected readonly dialogDetalhesVisivel = signal(false);

  ngOnInit(): void {
    this.carregarNotasFiscais();
  }

  protected carregarNotasFiscais(): void {
    this.loadingService.start();

    this.notaFiscalService
      .listar()
      .pipe(finalize(() => this.loadingService.stop()))
      .subscribe({
        next: (notasFiscais) => {
          this.notasFiscais.set(notasFiscais);

          this.logger.info('ListarNotasFiscais: notas carregadas', notasFiscais.length);
        },
        error: (error) => {
          this.logger.error('ListarNotasFiscais: erro ao carregar notas', error);

          const detail =
            error.status === 0
              ? 'Não foi possível estabelecer comunicação com o Faturamento.'
              : 'Não foi possível carregar as notas fiscais.';

          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail,
          });
        },
      });
  }

  protected visualizarNotaFiscal(id: number): void {
    this.loadingService.start();

    this.notaFiscalService
      .buscarPorId(id)
      .pipe(finalize(() => this.loadingService.stop()))
      .subscribe({
        next: (notaFiscal) => {
          this.notaFiscalSelecionada.set(notaFiscal);
          this.dialogDetalhesVisivel.set(true);

          this.logger.info('ListarNotasFiscais: detalhes carregados', notaFiscal.id);
        },
        error: (error) => {
          this.logger.error('ListarNotasFiscais: erro ao buscar detalhes', error);

          const detail =
            error.status === 404
              ? 'A nota fiscal não foi encontrada.'
              : error.status === 0
                ? 'Não foi possível estabelecer comunicação com o Faturamento.'
                : 'Não foi possível carregar a nota fiscal.';

          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail,
          });
        },
      });
  }

  protected obterSeveridadeStatus(status: StatusNotaFiscal): 'warn' | 'success' {
    return String(status).toLowerCase() === 'aberta' ? 'warn' : 'success';
  }

  protected calcularQuantidadeTotal(notaFiscal: iNotaFiscalResponseDTO): number {
    return notaFiscal.itens.reduce((total, item) => total + item.quantidade, 0);
  }

  protected notaEstaAberta(notaFiscal: iNotaFiscalResponseDTO): boolean {
    return String(notaFiscal.status).toLowerCase() === 'aberta';
  }

  protected confirmarImpressao(notaFiscal: iNotaFiscalResponseDTO): void {
    if (!this.notaEstaAberta(notaFiscal)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Impressão não permitida',
        detail: 'Somente notas fiscais abertas podem ser impressas.',
      });

      return;
    }

    this.confirmationService.confirm({
      key: 'confirmImpressao',
      header: 'Imprimir nota fiscal',
      message: `Deseja imprimir a nota fiscal nº ${notaFiscal.numero}?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Imprimir',
      rejectLabel: 'Cancelar',
      rejectButtonProps: {
        label: 'Cancel',
        severity: 'secondary',
        outlined: true,
      },
      accept: () => {
        this.imprimirNotaFiscal(notaFiscal);
      },
    });
  }

  private imprimirNotaFiscal(notaFiscal: iNotaFiscalResponseDTO): void {
    this.loadingService.start();

    this.notaFiscalService
      .imprimir(notaFiscal.id)
      .pipe(finalize(() => this.loadingService.stop()))
      .subscribe({
        next: (notaFiscalAtualizada) => {
          this.atualizarNotaFiscalNaLista(notaFiscalAtualizada);

          this.atualizarNotaFiscalNoDialog(notaFiscalAtualizada);

          this.notaFiscalParaImpressao.set(notaFiscalAtualizada);

          this.logger.info(
            'ListarNotasFiscaisComponent: nota fiscal impressa',
            notaFiscalAtualizada.id,
          );

          this.messageService.add({
            severity: 'success',
            summary: 'Nota fiscal impressa',
            detail:
              `A nota fiscal nº ${notaFiscalAtualizada.numero} ` +
              'foi fechada e o estoque foi atualizado.',
          });

          this.abrirImpressaoNavegador(notaFiscalAtualizada);
        },
        error: (error) => {
          this.tratarErroImpressao(error);
        },
      });
  }

  private atualizarNotaFiscalNaLista(notaFiscalAtualizada: iNotaFiscalResponseDTO): void {
    this.notasFiscais.update((notasFiscais) =>
      notasFiscais.map((notaFiscal) =>
        notaFiscal.id === notaFiscalAtualizada.id ? notaFiscalAtualizada : notaFiscal,
      ),
    );
  }

  private atualizarNotaFiscalNoDialog(notaFiscalAtualizada: iNotaFiscalResponseDTO): void {
    const notaSelecionada = this.notaFiscalSelecionada();

    if (notaSelecionada?.id === notaFiscalAtualizada.id) {
      this.notaFiscalSelecionada.set(notaFiscalAtualizada);
    }
  }

  private abrirImpressaoNavegador(notaFiscal: iNotaFiscalResponseDTO): void {
    const tituloAnterior = document.title;

    document.title = `Nota-Fiscal-${notaFiscal.numero}`;

    const restaurarTitulo = (): void => {
      document.title = tituloAnterior;
    };

    window.addEventListener('afterprint', restaurarTitulo, { once: true });

    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        window.print();
      });
    });
  }

  private tratarErroImpressao(error: HttpErrorResponse): void {
    this.logger.error('ListarNotasFiscaisComponent: erro ao imprimir nota fiscal', error);

    let detail = 'Não foi possível imprimir a nota fiscal.';

    if (error.status === 0) {
      detail = 'Não foi possível estabelecer comunicação com o Faturamento.';
    } else if (error.status === 404) {
      detail = 'A nota fiscal não foi encontrada.';
    } else if (error.status === 409) {
      detail =
        error.error?.detail ?? 'A nota fiscal já foi fechada ou não possui saldo suficiente.';
    } else if (error.status === 503) {
      detail = error.error?.detail ?? 'O serviço de Estoque está indisponível.';
    } else if (error.error?.detail) {
      detail = error.error.detail;
    }

    this.messageService.add({
      severity: 'error',
      summary: 'Erro na impressão',
      detail,
    });
  }
}
