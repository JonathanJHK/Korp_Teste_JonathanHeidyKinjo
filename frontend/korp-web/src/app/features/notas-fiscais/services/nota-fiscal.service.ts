import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { iNotaFiscalCriarDTO, iNotaFiscalResponseDTO } from '../models/nota-fiscal.model';

@Injectable({
  providedIn: 'root',
})
export class NotaFiscalService {
  private readonly httpClient = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrls.faturamento}/api/notas-fiscais`;

  listar(): Observable<iNotaFiscalResponseDTO[]> {
    return this.httpClient.get<iNotaFiscalResponseDTO[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<iNotaFiscalResponseDTO> {
    return this.httpClient.get<iNotaFiscalResponseDTO>(`${this.apiUrl}/${id}`);
  }

  cadastrar(notaFiscal: iNotaFiscalCriarDTO): Observable<iNotaFiscalResponseDTO> {
    return this.httpClient.post<iNotaFiscalResponseDTO>(this.apiUrl, notaFiscal);
  }

  imprimir(id: number): Observable<iNotaFiscalResponseDTO> {
    return this.httpClient.post<iNotaFiscalResponseDTO>(`${this.apiUrl}/${id}/imprimir`, {});
  }
}
