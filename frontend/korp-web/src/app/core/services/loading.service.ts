import { inject, Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  private readonly spinnerService = inject(NgxSpinnerService);

  private readonly spinnerName = 'spinnerFullScreen';
  private activeRequests = 0;

  start(): void {
    this.activeRequests++;

    if (this.activeRequests === 1) {
      void this.spinnerService.show(this.spinnerName);
    }
  }

  stop(): void {
    if (this.activeRequests === 0) return;

    this.activeRequests--;

    if (this.activeRequests === 0) {
      void this.spinnerService.hide(this.spinnerName);
    }
  }
}
