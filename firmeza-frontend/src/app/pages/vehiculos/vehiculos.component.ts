import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router,RouterLink } from '@angular/router';
import { VehiculoService } from '../../services/vehiculo.service';
import { AuthService } from '../../services/auth.service';
import { Vehiculo, VehiculoCreate } from '../../models/vehiculo.models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, FormsModule, RouterLink],
  selector: 'app-vehiculos',
  templateUrl: './vehiculos.component.html'
})
export class VehiculosComponent implements OnInit {
  private readonly vehiculoService = inject(VehiculoService);
  private readonly router = inject(Router);
  readonly authService = inject(AuthService);

  vehiculos = signal<Vehiculo[]>([]);
  cargando = signal<boolean>(true);
  guardando = signal<boolean>(false);
  mensajeError = signal<string | null>(null);
  mensajeAccion = signal<string | null>(null);

  // Modal para agregar o editar vehículo
  mostrarModalCrear = signal<boolean>(false);
  modoEdicion = signal<boolean>(false);
  vehiculoIdEnEdicion = signal<number | null>(null);

  // Filtros
  buscar = '';
  tipo = '';
  estado = '';

  // Modelo del formulario
  nuevoVehiculo: VehiculoCreate = this.getVehiculoInicial();

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
      error: () => {
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

  abrirModalCrear(): void {
    this.modoEdicion.set(false);
    this.vehiculoIdEnEdicion.set(null);
    this.nuevoVehiculo = this.getVehiculoInicial();
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  abrirModalEditar(auto: Vehiculo): void {
    this.modoEdicion.set(true);
    this.vehiculoIdEnEdicion.set(auto.id);
    this.nuevoVehiculo = {
      marca: auto.marca,
      modelo: auto.modelo,
      anio: auto.anio,
      placa: auto.placa,
      vin: auto.vin || '',
      color: auto.color || '',
      tipo: auto.tipo,
      kilometraje: auto.kilometraje,
      precioVenta: auto.precioVenta,
      precioRentaDia: auto.precioRentaDia,
      estado: auto.estado
    };
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  cerrarModalCrear(): void {
    this.mostrarModalCrear.set(false);
    this.modoEdicion.set(false);
    this.vehiculoIdEnEdicion.set(null);
  }

  guardarVehiculo(): void {
    if (!this.authService.isAuthenticated()) {
      this.mensajeError.set('Debes iniciar sesión para registrar o editar un vehículo.');
      return;
    }

    if (!this.nuevoVehiculo.marca || !this.nuevoVehiculo.modelo || !this.nuevoVehiculo.placa) {
      this.mensajeError.set('Marca, modelo y placa son campos obligatorios.');
      return;
    }

    this.guardando.set(true);
    this.mensajeError.set(null);

    if (this.modoEdicion() && this.vehiculoIdEnEdicion()) {
      // Modo Actualización / Edición
      this.vehiculoService.actualizarVehiculo(this.vehiculoIdEnEdicion()!, this.nuevoVehiculo).subscribe({
        next: () => {
          this.guardando.set(false);
          this.mostrarModalCrear.set(false);
          this.mensajeAccion.set(`¡Vehículo ${this.nuevoVehiculo.marca} ${this.nuevoVehiculo.modelo} (${this.nuevoVehiculo.placa}) actualizado correctamente!`);
          this.cargarVehiculos();
          setTimeout(() => this.mensajeAccion.set(null), 4000);
        },
        error: (err) => {
          this.guardando.set(false);
          const detalle = err.error?.mensaje || 'Error al actualizar el vehículo. Verifica los datos.';
          this.mensajeError.set(detalle);
        }
      });
    } else {
      // Modo Creación
      this.vehiculoService.crearVehiculo(this.nuevoVehiculo).subscribe({
        next: (creado) => {
          this.guardando.set(false);
          this.mostrarModalCrear.set(false);
          this.mensajeAccion.set(`¡Vehículo ${creado.marca} ${creado.modelo} (${creado.placa}) registrado con éxito!`);
          this.cargarVehiculos();
          setTimeout(() => this.mensajeAccion.set(null), 4000);
        },
        error: (err) => {
          this.guardando.set(false);
          const detalle = err.error?.mensaje || 'Error al guardar el vehículo. Verifica los datos.';
          this.mensajeError.set(detalle);
        }
      });
    }
  }

  eliminarVehiculo(id: number, placa: string): void {
    if (!confirm(`¿Estás seguro de que deseas dar de baja el vehículo con placa ${placa}?`)) {
      return;
    }

    this.vehiculoService.eliminarVehiculo(id).subscribe({
      next: () => {
        this.mensajeAccion.set(`Vehículo con placa ${placa} eliminado.`);
        this.cargarVehiculos();
        setTimeout(() => this.mensajeAccion.set(null), 3500);
      },
      error: (err) => {
        const detalle = err.error?.mensaje || 'No se puede eliminar el vehículo porque tiene operaciones asociadas.';
        this.mensajeError.set(detalle);
      }
    });
  }

  solicitarVehiculo(vehiculo: Vehiculo, modalidad: 'compra' | 'renta'): void {
    this.router.navigate(['/ventas'],{
      queryParams:{
        tipo: 'vehiculo',
        id: vehiculo.id,
        modalidad: modalidad
      }
    });
  }

  private getVehiculoInicial(): VehiculoCreate {
    return {
      marca: '',
      modelo: '',
      anio: new Date().getFullYear(),
      placa: '',
      vin: '',
      color: '',
      tipo: 'SUV',
      kilometraje: 0,
      precioVenta: 50000000,
      precioRentaDia: 150000,
      estado: 'Disponible'
    };
  }
}
