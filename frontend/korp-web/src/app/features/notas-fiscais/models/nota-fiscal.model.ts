export type StatusNotaFiscal = 'Aberta' | 'Fechada';

export interface iItemNotaFiscalCriarDTO {
  produtoId: number;
  quantidade: number;
}

export interface iNotaFiscalCriarDTO {
  itens: iItemNotaFiscalCriarDTO[];
}

export interface iItemNotaFiscalResponseDTO {
  id: number;
  produtoId: number;
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
}

export interface iNotaFiscalResponseDTO {
  id: number;
  numero: number;
  status: StatusNotaFiscal;
  dataDeCriacao: string;
  dataDeFechamento: string | null;
  itens: iItemNotaFiscalResponseDTO[];
}
