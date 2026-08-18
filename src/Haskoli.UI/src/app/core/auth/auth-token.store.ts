import { Injectable } from '@angular/core';

const TOKEN_KEY = 'haskoli.auth.token';

@Injectable({ providedIn: 'root' })
export class AuthTokenStore {
  getToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  setToken(token: string): void {
    sessionStorage.setItem(TOKEN_KEY, token);
  }

  clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
  }
}
