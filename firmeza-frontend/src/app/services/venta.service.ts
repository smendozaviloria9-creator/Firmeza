import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Venta } from '../models/venta.models';

@Injectable({
  providedIn: 'root'
})
export class VentaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ventas`;
  private readonly exportUrl = `${environment.apiUrl}/exportacion`;

  // Listar de venta
  getVentas(): Observable<Venta[]> {
    return this.http.get<Venta[]>(this.baseUrl);
  }

  // Eliminar
  eliminarVenta(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Descargar reporte
  descargarExcel(): Observable<Blob> {
    return this.http.get(`${this.exportUrl}/ventas/excel`, { responseType: 'blob' });
  }

  // Descargar reporte PDF
  descargarPdf(): Observable<Blob> {
    return this.http.get(`${this.exportUrl}/ventas/pdf`, { responseType: 'blob' });
  }
}
