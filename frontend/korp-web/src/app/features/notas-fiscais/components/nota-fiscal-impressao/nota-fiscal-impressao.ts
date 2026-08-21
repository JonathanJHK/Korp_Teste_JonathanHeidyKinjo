import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { iNotaFiscalResponseDTO } from '../../models/nota-fiscal.model';

@Component({
  selector: 'app-nota-fiscal-impressao',
  imports: [DatePipe],
  templateUrl: './nota-fiscal-impressao.html',
  styleUrl: './nota-fiscal-impressao.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotaFiscalImpressao {
  readonly notaFiscal = input<iNotaFiscalResponseDTO | null>(null);

  protected calcularQuantidadeTotal(notaFiscal: iNotaFiscalResponseDTO): number {
    return notaFiscal.itens.reduce((total, item) => total + item.quantidade, 0);
  }
}
