import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Venta, VentaCreate, VentaVehiculo, VentaVehiculoCreate } from '../models/venta.models';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ventas`;
  private readonly vehiculosUrl = `${environment.apiUrl}/ventas-vehiculos`;
  private readonly exportUrl = `${environment.apiUrl}/exportacion`;

  // Listar ventas de repuestos
  getVentas(): Observable<Venta[]> {
    return this.http.get<Venta[]>(this.baseUrl);
  }

  // Registrar nueva venta de repuestos
  crearVenta(venta: VentaCreate): Observable<Venta> {
    return this.http.post<Venta>(this.baseUrl, venta);
  }

  // Listar ventas de vehículos
  getVentasVehiculos(): Observable<VentaVehiculo[]> {
    return this.http.get<VentaVehiculo[]>(this.vehiculosUrl);
  }

  // Registrar nueva venta de vehículo
  crearVentaVehiculo(venta: VentaVehiculoCreate): Observable<VentaVehiculo> {
    return this.http.post<VentaVehiculo>(this.vehiculosUrl, venta);
  }

  // Eliminar venta de repuestos
  eliminarVenta(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Eliminar venta de vehículo (devuelve vehículo a Disponible)
  eliminarVentaVehiculo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.vehiculosUrl}/${id}`);
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
