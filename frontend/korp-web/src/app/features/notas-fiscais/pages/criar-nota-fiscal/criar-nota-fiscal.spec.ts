import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CriarNotaFiscal } from './criar-nota-fiscal';

describe('CriarNotaFiscal', () => {
  let component: CriarNotaFiscal;
  let fixture: ComponentFixture<CriarNotaFiscal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CriarNotaFiscal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CriarNotaFiscal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
