export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  nombreCompleto: string;
  roles: string[];
}

export interface RegistroRequest {
  email: string;
  password: string;
  nombreCompleto: string;
}

export interface UsuarioSesion {
  email: string;
  nombreCompleto: string;
  roles: string[];
}
