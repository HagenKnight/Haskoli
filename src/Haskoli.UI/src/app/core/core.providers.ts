import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { API_BASE_URL } from './config/api.config';
import { authInterceptor } from './http/auth.interceptor';

export function provideCore(): EnvironmentProviders[] {
  return [
    makeEnvironmentProviders([{ provide: API_BASE_URL, useValue: environment.apiBaseUrl }]),
    provideHttpClient(withInterceptors([authInterceptor])),
  ];
}
