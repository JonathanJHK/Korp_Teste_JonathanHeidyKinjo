import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ListarNotasFiscais } from './listar-notas-fiscais';

describe('ListarNotasFiscais', () => {
  let component: ListarNotasFiscais;
  let fixture: ComponentFixture<ListarNotasFiscais>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListarNotasFiscais]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListarNotasFiscais);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
