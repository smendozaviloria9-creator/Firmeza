import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Cliente, ClienteCreate } from '../models/cliente.models';

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/clientes`;

  getClientes(buscar?: string): Observable<Cliente[]> {
    const url = buscar ? `${this.baseUrl}?buscar=${encodeURIComponent(buscar)}` : this.baseUrl;
    return this.http.get<Cliente[]>(url);
  }

  crearCliente(cliente: ClienteCreate): Observable<Cliente> {
    return this.http.post<Cliente>(this.baseUrl, cliente);
  }
}
