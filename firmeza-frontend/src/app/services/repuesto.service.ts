import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Repuesto, RepuestoCreate } from '../models/repuesto.models';

@Injectable({
  providedIn: 'root'
})
export class RepuestoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/productos`;

  getRepuestos(buscar?: string, categoria?: string): Observable<Repuesto[]> {
    let params = new HttpParams();
    if (buscar) params = params.set('buscar', buscar);
    if (categoria) params = params.set('categoria', categoria);

    return this.http.get<Repuesto[]>(this.baseUrl, { params });
  }

  getRepuestoById(id: number): Observable<Repuesto> {
    return this.http.get<Repuesto>(`${this.baseUrl}/${id}`);
  }

  crearRepuesto(repuesto: RepuestoCreate): Observable<Repuesto> {
    return this.http.post<Repuesto>(this.baseUrl, repuesto);
  }

  eliminarRepuesto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
