export interface AuthRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  id: string;
  username: string;
  email: string;
  token: string;
}

export interface RegistrationRequest {
  nombre: string;
  apellidos: string;
  email: string;
  username: string;
  password: string;
}

export interface RegistrationResponse {
  userId: string;
  username: string;
  email: string;
  token: string;
}
