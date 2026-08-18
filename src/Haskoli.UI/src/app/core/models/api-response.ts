export interface ApiResponse<T> {
  succeeded: boolean;
  message: string | null;
  errors?: Readonly<Record<string, string[]>>;
  data: T;
}
