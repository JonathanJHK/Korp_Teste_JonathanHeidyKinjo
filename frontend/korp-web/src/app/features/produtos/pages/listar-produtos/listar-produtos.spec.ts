import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarProdutos } from './listar-produtos';

describe('ListarProdutos', () => {
  let component: ListarProdutos;
  let fixture: ComponentFixture<ListarProdutos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarProdutos]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarProdutos);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
