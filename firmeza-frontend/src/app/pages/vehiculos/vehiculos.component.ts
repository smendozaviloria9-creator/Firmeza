import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
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

  // Pestaña de navegación para separar Venta vs Renta
  pestanaModalidad = signal<'todos' | 'venta' | 'renta'>('todos');

  // Modal para agregar o editar vehículo
  mostrarModalCrear = signal<boolean>(false);
  modoEdicion = signal<boolean>(false);
  vehiculoIdEnEdicion = signal<number | null>(null);

  // Modalidad seleccionada en el formulario: 'venta' | 'renta' | 'ambos'
  modalidadVehiculo = signal<'venta' | 'renta' | 'ambos'>('venta');

  // Indicador si es auto nuevo 0 Km sin placa fija (en trámite)
  esAutoNuevoSinPlaca = signal<boolean>(true);

  // Filtros de búsqueda
  buscar = '';
  tipo = '';
  estado = '';

  // Modelo del formulario
  nuevoVehiculo: VehiculoCreate = this.getVehiculoInicial();

  // Filtrado reactivo según la pestaña activa (Separación Venta vs Renta)
  vehiculosFiltrados = computed(() => {
    const pestana = this.pestanaModalidad();
    const lista = this.vehiculos();
    if (pestana === 'venta') {
      return lista.filter(v => v.precioVenta > 0);
    }
    if (pestana === 'renta') {
      return lista.filter(v => v.precioRentaDia > 0);
    }
    return lista;
  });

  totalVentaCount = computed(() => this.vehiculos().filter(v => v.precioVenta > 0).length);
  totalRentaCount = computed(() => this.vehiculos().filter(v => v.precioRentaDia > 0).length);

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

  esPlacaProvisional(placa?: string | null): boolean {
    if (!placa) return true;
    const p = placa.trim().toUpperCase();
    return p.startsWith('TMP-') || p.startsWith('PROV-') || p === 'EN TRÁMITE' || p === 'SIN PLACA' || p === 'EN TRAMITE';
  }

  abrirModalCrear(modalidadPorDefecto?: 'venta' | 'renta'): void {
    this.modoEdicion.set(false);
    this.vehiculoIdEnEdicion.set(null);
    this.nuevoVehiculo = this.getVehiculoInicial();

    const modalidadInicial = modalidadPorDefecto || (this.pestanaModalidad() === 'renta' ? 'renta' : 'venta');
    this.modalidadVehiculo.set(modalidadInicial);
    this.esAutoNuevoSinPlaca.set(modalidadInicial === 'venta');

    if (modalidadInicial === 'venta') {
      this.nuevoVehiculo.precioRentaDia = 0;
      this.nuevoVehiculo.kilometraje = 0;
    } else if (modalidadInicial === 'renta') {
      this.nuevoVehiculo.precioVenta = 0;
    }

    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  abrirModalEditar(auto: Vehiculo): void {
    this.modoEdicion.set(true);
    this.vehiculoIdEnEdicion.set(auto.id);

    // Determinar modalidad existente
    if (auto.precioVenta > 0 && auto.precioRentaDia === 0) {
      this.modalidadVehiculo.set('venta');
    } else if (auto.precioRentaDia > 0 && auto.precioVenta === 0) {
      this.modalidadVehiculo.set('renta');
    } else {
      this.modalidadVehiculo.set('ambos');
    }

    const esProvisional = this.esPlacaProvisional(auto.placa);
    this.esAutoNuevoSinPlaca.set(esProvisional);

    this.nuevoVehiculo = {
      marca: auto.marca,
      modelo: auto.modelo,
      anio: auto.anio,
      placa: esProvisional ? '' : auto.placa,
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

  onModalidadChange(modalidad: 'venta' | 'renta' | 'ambos'): void {
    this.modalidadVehiculo.set(modalidad);
    if (modalidad === 'venta') {
      this.nuevoVehiculo.precioRentaDia = 0;
      if (this.nuevoVehiculo.precioVenta <= 0) {
        this.nuevoVehiculo.precioVenta = 50000000;
      }
    } else if (modalidad === 'renta') {
      this.nuevoVehiculo.precioVenta = 0;
      this.esAutoNuevoSinPlaca.set(false);
      if (this.nuevoVehiculo.precioRentaDia <= 0) {
        this.nuevoVehiculo.precioRentaDia = 150000;
      }
    } else {
      if (this.nuevoVehiculo.precioVenta <= 0) this.nuevoVehiculo.precioVenta = 50000000;
      if (this.nuevoVehiculo.precioRentaDia <= 0) this.nuevoVehiculo.precioRentaDia = 150000;
    }
  }

  guardarVehiculo(): void {
    if (!this.authService.isAuthenticated()) {
      this.mensajeError.set('Debes iniciar sesión para registrar o editar un vehículo.');
      return;
    }

    if (!this.nuevoVehiculo.marca?.trim() || !this.nuevoVehiculo.modelo?.trim()) {
      this.mensajeError.set('Marca y modelo son campos obligatorios.');
      return;
    }

    const modalidad = this.modalidadVehiculo();

    // Validar precios según modalidad
    if (modalidad === 'venta' && this.nuevoVehiculo.precioVenta <= 0) {
      this.mensajeError.set('Por favor ingresa un precio de venta válido.');
      return;
    }
    if (modalidad === 'renta' && this.nuevoVehiculo.precioRentaDia <= 0) {
      this.mensajeError.set('Por favor ingresa una tarifa de renta por día válida.');
      return;
    }

    // Manejo de la Placa (Opcional para autos nuevos / En trámite)
    if (modalidad === 'renta' && (!this.nuevoVehiculo.placa || !this.nuevoVehiculo.placa.trim())) {
      this.mensajeError.set('Los vehículos de renta deben tener una placa registrada para circular legalmente.');
      return;
    }

    // Si es auto nuevo 0 Km sin placa fija o se dejó vacía en venta, generar identificador temporal único
    if (this.esAutoNuevoSinPlaca() || !this.nuevoVehiculo.placa?.trim()) {
      const sufijo = Math.random().toString(36).substring(2, 7).toUpperCase();
      this.nuevoVehiculo.placa = `TMP-${sufijo}`;
    } else {
      this.nuevoVehiculo.placa = this.nuevoVehiculo.placa.trim().toUpperCase();
    }

    // Ajustar precios
    if (modalidad === 'venta') {
      this.nuevoVehiculo.precioRentaDia = 0;
    } else if (modalidad === 'renta') {
      this.nuevoVehiculo.precioVenta = 0;
    }

    this.guardando.set(true);
    this.mensajeError.set(null);

    if (this.modoEdicion() && this.vehiculoIdEnEdicion()) {
      // Modo Actualización / Edición
      this.vehiculoService.actualizarVehiculo(this.vehiculoIdEnEdicion()!, this.nuevoVehiculo).subscribe({
        next: () => {
          this.guardando.set(false);
          this.mostrarModalCrear.set(false);
          const placaTexto = this.esPlacaProvisional(this.nuevoVehiculo.placa) ? '0 Km (En trámite)' : this.nuevoVehiculo.placa;
          this.mensajeAccion.set(`¡Vehículo ${this.nuevoVehiculo.marca} ${this.nuevoVehiculo.modelo} (${placaTexto}) actualizado correctamente!`);
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
          const placaTexto = this.esPlacaProvisional(creado.placa) ? '0 Km (En trámite)' : creado.placa;
          this.mensajeAccion.set(`¡Vehículo ${creado.marca} ${creado.modelo} (${placaTexto}) registrado con éxito!`);
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
    const textoPlaca = this.esPlacaProvisional(placa) ? '0 Km (En trámite)' : placa;
    if (!confirm(`¿Estás seguro de que deseas dar de baja el vehículo con placa ${textoPlaca}?`)) {
      return;
    }

    this.vehiculoService.eliminarVehiculo(id).subscribe({
      next: () => {
        this.mensajeAccion.set(`Vehículo (${textoPlaca}) eliminado del inventario.`);
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
    this.router.navigate(['/ventas'], {
      queryParams: {
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
      precioRentaDia: 0,
      estado: 'Disponible'
    };
  }
}
