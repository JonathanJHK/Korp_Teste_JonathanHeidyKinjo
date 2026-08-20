import { Component, inject, OnInit, signal } from '@angular/core';
import { LoadingService } from '@core/services/loading.service';
import { LoggerService } from '@core/services/logger.service';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { catchError, finalize, tap } from 'rxjs';
import { iProdutoResponseDTO } from '../../models/produto.model';
import { ProdutoService } from '../../services/produto.service';

@Component({
  selector: 'app-listar-produtos',
  imports: [ButtonModule, TableModule, ToastModule],
  providers: [MessageService],
  templateUrl: './listar-produtos.html',
  styleUrl: './listar-produtos.scss',
})
export class ListarProdutos implements OnInit {
  private readonly produtoService = inject(ProdutoService);
  private readonly loadingService = inject(LoadingService);
  private readonly logger = inject(LoggerService);
  private readonly messageService = inject(MessageService);

  protected readonly produtos = signal<iProdutoResponseDTO[]>([]);

  ngOnInit(): void {
    this.carregarProdutos();
  }

  protected carregarProdutos(): void {
    this.loadingService.start();

    this.produtoService
      .listar()
      .pipe(
        tap((produtos) => {
          this.produtos.set(produtos);

          this.logger.info('ListarProdutosComponent: produtos carregados', produtos.length);
        }),
        catchError((error) => {
          this.logger.error('ListarProdutosComponent: erro ao carregar produtos', error);

          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Não foi possível carregar os produtos.',
          });

          return [];
        }),
        finalize(() => this.loadingService.stop()),
      )
      .subscribe();
  }
}
