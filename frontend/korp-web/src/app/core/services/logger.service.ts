import { Injectable, isDevMode } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LoggerService {
  private readonly enabled = isDevMode();

  info(message: string, ...optionalParams: unknown[]): void {
    if (!this.enabled) return;

    console.info(this.withDate(), message, ...optionalParams);
  }

  warn(message: string, ...optionalParams: unknown[]): void {
    if (!this.enabled) return;

    console.warn(this.withDate(), message, ...optionalParams);
  }

  error(message: string, ...optionalParams: unknown[]): void {
    if (!this.enabled) return;

    console.error(this.withDate(), message, ...optionalParams);
  }

  private withDate(): string {
    return `[${new Date().toISOString()}]`;
  }
}
