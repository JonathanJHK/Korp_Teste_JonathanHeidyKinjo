import { Routes } from '@angular/router';

export const NOTAS_FISCAIS_ROUTES: Routes = [
  {
    path: '',
    title: 'Notas fiscais',
    loadComponent: () =>
      import('./pages/listar-notas-fiscais/listar-notas-fiscais').then(
        (component) => component.ListarNotasFiscais,
      ),
  },
  {
    path: 'nova',
    title: 'Cadastrar nota fiscal',
    loadComponent: () =>
      import('./pages/criar-nota-fiscal/criar-nota-fiscal').then(
        (component) => component.CriarNotaFiscal,
      ),
  },
];
