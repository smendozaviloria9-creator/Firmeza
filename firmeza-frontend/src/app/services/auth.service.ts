import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RegistroRequest, UsuarioSesion } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenKey = 'firmeza_token';
  private readonly userKey = 'firmeza_user';

  // Señal reactiva con el usuario autenticado actualmente
  currentUser = signal<UsuarioSesion | null>(this.obtenerUsuarioDeStorage());

  // Iniciar sesión
  login(credenciales: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, credenciales).pipe(
      tap(respuesta => this.guardarSesion(respuesta))
    );
  }

  // Registro de nuevo cliente
  registro(datos: RegistroRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/registro-cliente`, datos).pipe(
      tap(respuesta => this.guardarSesion(respuesta))
    );
  }

  // Cerrar sesión
  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  // Obtener el token JWT guardado
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Verificar si hay sesión activa
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  // Verificar si el usuario tiene un rol específico
  hasRole(rol: string): boolean {
    const usuario = this.currentUser();
    return usuario ? usuario.roles.includes(rol) : false;
  }

  private guardarSesion(respuesta: LoginResponse): void {
    localStorage.setItem(this.tokenKey, respuesta.token);
    const usuario: UsuarioSesion = {
      email: respuesta.email,
      nombreCompleto: respuesta.nombreCompleto,
      roles: respuesta.roles
    };
    localStorage.setItem(this.userKey, JSON.stringify(usuario));
    this.currentUser.set(usuario);
  }

  private obtenerUsuarioDeStorage(): UsuarioSesion | null {
    const data = localStorage.getItem(this.userKey);
    if (!data) return null;
    try {
      return JSON.parse(data) as UsuarioSesion;
    } catch {
      return null;
    }
  }
}
