import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthTokenStore } from '../auth/auth-token.store';
import { API_BASE_URL } from '../config/api.config';

function isApiRequest(url: string, apiBaseUrl: string): boolean {
  if (url.startsWith('/api')) {
    return true;
  }

  if (apiBaseUrl && url.startsWith(`${apiBaseUrl.replace(/\/$/, '')}/api`)) {
    return true;
  }

  try {
    return new URL(url, 'http://localhost').pathname.startsWith('/api');
  } catch {
    return false;
  }
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthTokenStore).getToken();
  if (!token || !isApiRequest(req.url, inject(API_BASE_URL))) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
