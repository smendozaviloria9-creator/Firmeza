import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VehiculoService } from '../../services/vehiculo.service';
import { Vehiculo } from '../../models/vehiculo.models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, FormsModule],
  selector: 'app-vehiculos',
  templateUrl: './vehiculos.component.html'
})
export class VehiculosComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);

  vehiculos = signal<Vehiculo[]>([]);
  cargando = signal<boolean>(true);
  mensajeError = signal<string | null>(null);

  // Filtros
  buscar = '';
  tipo = '';
  estado = '';

  // Mensaje de éxito al simular acción
  mensajeAccion = signal<string | null>(null);

  ngOnInit(): void {
    this.cargarVehiculos();
  }

  cargarVehiculos(): void {
    this.cargando.set(true);
    this.mensajeError.set(null);

    this.vehiculoService.getVehiculos(this.buscar, this.tipo, this.estado).subscribe({
      next: (data) => {
        this.vehiculos.set(data);
        this.cargando.set(false);
      },
      error: (err) => {
        this.cargando.set(false);
        this.mensajeError.set('Error al cargar el catálogo de vehículos.');
      }
    });
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.tipo = '';
    this.estado = '';
    this.cargarVehiculos();
  }

  solicitarVehiculo(vehiculo: Vehiculo, modalidad: 'compra' | 'renta'): void {
    const accion = modalidad === 'compra' ? 'compra' : 'alquiler/renta';
    this.mensajeAccion.set(`¡Solicitud de ${accion} registrada para el ${vehiculo.marca} ${vehiculo.modelo} (${vehiculo.placa})!`);

    // Auto-ocultar el mensaje después de 4 segundos
    setTimeout(() => this.mensajeAccion.set(null), 4000);
  }
}
