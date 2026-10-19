import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Venta, VentaCreate, VentaVehiculoCreate } from '../models/venta.models';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ventas`;
  private readonly exportUrl = `${environment.apiUrl}/exportacion`;

  // Listar ventas de repuestos
  getVentas(): Observable<Venta[]> {
    return this.http.get<Venta[]>(this.baseUrl);
  }

  // Registrar nueva venta de repuestos
  crearVenta(venta: VentaCreate): Observable<Venta> {
    return this.http.post<Venta>(this.baseUrl, venta);
  }

  // Registrar nueva venta de vehículo
  crearVentaVehiculo(venta: VentaVehiculoCreate): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/ventas-vehiculos`, venta);
  }

  // Eliminar venta
  eliminarVenta(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Descargar reporte Excel
  descargarExcel(): Observable<Blob> {
    return this.http.get(`${this.exportUrl}/ventas/excel`, { responseType: 'blob' });
  }

  // Descargar reporte PDF
  descargarPdf(): Observable<Blob> {
    return this.http.get(`${this.exportUrl}/ventas/pdf`, { responseType: 'blob' });
  }
}
