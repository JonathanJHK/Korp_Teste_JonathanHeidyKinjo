import { Routes } from '@angular/router';

export const PRODUTOS_ROUTES: Routes = [
  {
    path: '',
    title: 'Produtos',
    loadComponent: () =>
      import('./pages/listar-produtos/listar-produtos').then(
        (component) => component.ListarProdutos,
      ),
  },
];
