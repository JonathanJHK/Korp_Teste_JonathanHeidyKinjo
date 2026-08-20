import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CadastrarNotaFiscal } from './cadastrar-nota-fiscal';

describe('CadastrarNotaFiscal', () => {
  let component: CadastrarNotaFiscal;
  let fixture: ComponentFixture<CadastrarNotaFiscal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CadastrarNotaFiscal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CadastrarNotaFiscal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
