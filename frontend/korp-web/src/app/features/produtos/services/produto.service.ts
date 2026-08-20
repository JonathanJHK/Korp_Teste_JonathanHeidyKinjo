import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { iProdutoCriarDTO, iProdutoResponseDTO } from '../models/produto.model';

@Injectable({
  providedIn: 'root',
})
export class ProdutoService {
  private readonly httpClient = inject(HttpClient);

  private readonly apiUrl = `${environment.apiUrls.estoque}/api/produtos`;

  listar(): Observable<iProdutoResponseDTO[]> {
    return this.httpClient.get<iProdutoResponseDTO[]>(this.apiUrl);
  }

  buscarPorId(id: number): Observable<iProdutoResponseDTO> {
    return this.httpClient.get<iProdutoResponseDTO>(`${this.apiUrl}/${id}`);
  }

  cadastrar(produto: iProdutoCriarDTO): Observable<iProdutoResponseDTO> {
    return this.httpClient.post<iProdutoResponseDTO>(this.apiUrl, produto);
  }
}
