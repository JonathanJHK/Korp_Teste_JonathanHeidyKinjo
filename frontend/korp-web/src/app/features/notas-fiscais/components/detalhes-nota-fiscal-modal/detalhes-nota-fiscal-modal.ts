import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';

import { iNotaFiscalResponseDTO, StatusNotaFiscal } from '../../models/nota-fiscal.model';

@Component({
  selector: 'app-detalhes-nota-fiscal-modal',
  imports: [DatePipe, ButtonModule, DialogModule, TagModule],
  templateUrl: './detalhes-nota-fiscal-modal.html',
  styleUrl: './detalhes-nota-fiscal-modal.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DetalhesNotaFiscalModal {
  // Controla se o modal está aberto ou fechado. O componente pai pode alterar esse valor.
  readonly visible = model(false);

  // Recebe a nota fiscal que será exibida no modal. null indica que nenhuma nota foi selecionada.
  readonly notaFiscal = input<iNotaFiscalResponseDTO | null>(null);

  // Define a cor visual do status: notas abertas usam aviso e notas fechadas usam sucesso.
  protected obterSeveridadeStatus(status: StatusNotaFiscal): 'warn' | 'success' {
    return String(status).toLowerCase() === 'aberta' ? 'warn' : 'success';
  }

  // Soma a quantidade de todos os itens da nota fiscal.
  protected calcularQuantidadeTotal(notaFiscal: iNotaFiscalResponseDTO): number {
    // O acumulador começa em zero e recebe a quantidade de cada item percorrido.
    return notaFiscal.itens.reduce((total, item) => total + item.quantidade, 0);
  }

  protected fechar(): void {
    // Atualiza o model para informar ao componente pai que o modal foi fechado.
    this.visible.set(false);
  }
}
