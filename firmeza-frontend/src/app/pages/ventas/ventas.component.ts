import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VentaService } from '../../services/venta.service';
import { Venta } from '../../models/venta.models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, DatePipe, FormsModule],
  selector: 'app-ventas',
  templateUrl: './ventas.component.html'
})
export class VentasComponent implements OnInit {
  private readonly ventaService = inject(VentaService);

  ventas = signal<Venta[]>([]);
  cargando = signal<boolean>(true);
  mensajeExito = signal<string | null>(null);
  mensajeError = signal<string | null>(null);

  descargandoExcel = signal<boolean>(false);
  descargandoPdf = signal<boolean>(false);

  filtro = '';

  // Filtro reactivo en tiempo real
  ventasFiltradas = computed(() => {
    const termino = this.filtro.trim().toLowerCase();
    if (!termino) return this.ventas();

    return this.ventas().filter(v =>
      v.id.toString().includes(termino) ||
      (v.clienteNombre && v.clienteNombre.toLowerCase().includes(termino)) ||
      v.detalles.some(d => d.productoNombre.toLowerCase().includes(termino))
    );
  });

  // Métricas calculadas para las tarjetas superiores
  totalFacturado = computed(() => this.ventas().reduce((acc, v) => acc + v.total, 0));
  totalVentasCount = computed(() => this.ventas().length);

  ngOnInit(): void {
    this.cargarVentas();
  }

  cargarVentas(): void {
    this.cargando.set(true);
    this.mensajeError.set(null);

    this.ventaService.getVentas().subscribe({
      next: (data) => {
        this.ventas.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.mensajeError.set('Error al cargar el historial de ventas.');
      }
    });
  }

  eliminarVenta(id: number): void {
    if (!confirm(`¿Estás seguro de que deseas eliminar la venta N° ${id}? El stock se devolverá al inventario.`)) {
      return;
    }

    this.ventaService.eliminarVenta(id).subscribe({
      next: () => {
        this.mensajeExito.set(`Venta N° ${id} eliminada correctamente.`);
        this.cargarVentas();
        setTimeout(() => this.mensajeExito.set(null), 4000);
      },
      error: () => {
        this.mensajeError.set(`No se pudo eliminar la venta N° ${id}.`);
      }
    });
  }

  exportarExcel(): void {
    this.descargandoExcel.set(true);
    this.ventaService.descargarExcel().subscribe({
      next: (blob) => {
        this.descargarArchivo(blob, `Ventas_${new Date().toISOString().slice(0,10)}.xlsx`);
        this.descargandoExcel.set(false);
      },
      error: () => {
        this.descargandoExcel.set(false);
        this.mensajeError.set('Error al generar el archivo Excel.');
      }
    });
  }

  exportarPdf(): void {
    this.descargandoPdf.set(true);
    this.ventaService.descargarPdf().subscribe({
      next: (blob) => {
        this.descargarArchivo(blob, `Ventas_${new Date().toISOString().slice(0,10)}.pdf`);
        this.descargandoPdf.set(false);
      },
      error: () => {
        this.descargandoPdf.set(false);
        this.mensajeError.set('Error al generar el reporte PDF.');
      }
    });
  }

  private descargarArchivo(blob: Blob, nombre: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = nombre;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
