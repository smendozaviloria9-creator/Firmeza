import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Vehiculo } from '../models/vehiculo.models';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/vehiculos`;

  getVehiculos(buscar?: string, tipo?: string, estado?: string): Observable<Vehiculo[]> {
    let params = new HttpParams();
    if (buscar) params = params.set('buscar', buscar);
    if (tipo) params = params.set('tipo', tipo);
    if (estado) params = params.set('estado', estado);

    return this.http.get<Vehiculo[]>(this.baseUrl, { params });
  }

  getVehiculoById(id: number): Observable<Vehiculo> {
    return this.http.get<Vehiculo>(`${this.baseUrl}/${id}`);
  }
}
