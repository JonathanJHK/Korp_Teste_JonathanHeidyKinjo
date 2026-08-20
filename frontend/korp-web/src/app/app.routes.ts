import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout').then((component) => component.MainLayout),
    children: [
      {
        path: '',
        redirectTo: 'produtos',
        pathMatch: 'full',
      },
      {
        path: 'produtos',
        loadChildren: () =>
          import('./features/produtos/produtos.routes').then((routes) => routes.PRODUTOS_ROUTES),
      },
      {
        path: 'notas-fiscais',
        loadChildren: () =>
          import('./features/notas-fiscais/notas-fiscais.routes').then(
            (routes) => routes.NOTAS_FISCAIS_ROUTES,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'produtos',
  },
];
