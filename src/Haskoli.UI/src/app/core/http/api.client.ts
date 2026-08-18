import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';

export interface ApiRequestOptions {
  headers?: HttpHeaders | Record<string, string | string[]>;
  params?:
    | HttpParams
    | Record<string, string | number | boolean | ReadonlyArray<string | number | boolean>>;
}

@Injectable({ providedIn: 'root' })
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  get<T>(path: string, options?: ApiRequestOptions): Observable<T> {
    return this.http.get<T>(this.resolve(path), options);
  }

  post<T>(path: string, body: unknown, options?: ApiRequestOptions): Observable<T> {
    return this.http.post<T>(this.resolve(path), body, options);
  }

  put<T>(path: string, body: unknown, options?: ApiRequestOptions): Observable<T> {
    return this.http.put<T>(this.resolve(path), body, options);
  }

  patch<T>(path: string, body: unknown, options?: ApiRequestOptions): Observable<T> {
    return this.http.patch<T>(this.resolve(path), body, options);
  }

  delete<T>(path: string, options?: ApiRequestOptions): Observable<T> {
    return this.http.delete<T>(this.resolve(path), options);
  }

  private resolve(path: string): string {
    const normalized = path.startsWith('/') ? path : `/${path}`;
    const prefix = this.baseUrl.replace(/\/$/, '');
    return `${prefix}/api${normalized}`;
  }
}
