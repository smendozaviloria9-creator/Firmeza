import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RepuestoService } from '../../services/repuesto.service';
import { AuthService } from '../../services/auth.service';
import { Repuesto, RepuestoCreate } from '../../models/repuesto.models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, FormsModule, RouterLink],
  selector: 'app-repuestos',
  templateUrl: './repuestos.component.html'
})
export class RepuestosComponent implements OnInit {
  private readonly repuestoService = inject(RepuestoService);
  readonly authService = inject(AuthService);

  repuestos = signal<Repuesto[]>([]);
  cargando = signal<boolean>(true);
  guardando = signal<boolean>(false);
  mensajeError = signal<string | null>(null);
  mensajeCarrito = signal<string | null>(null);

  // Modal para agregar repuesto
  mostrarModalCrear = signal<boolean>(false);

  // Filtros
  buscar = '';
  categoria = '';

  // Modelo del formulario
  nuevoRepuesto: RepuestoCreate = this.getRepuestoInicial();

  ngOnInit(): void {
    this.cargarRepuestos();
  }

  cargarRepuestos(): void {
    this.cargando.set(true);
    this.mensajeError.set(null);

    this.repuestoService.getRepuestos(this.buscar, this.categoria).subscribe({
      next: (data) => {
        this.repuestos.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.mensajeError.set('Error al cargar el catálogo de repuestos.');
      }
    });
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.categoria = '';
    this.cargarRepuestos();
  }

  abrirModalCrear(): void {
    this.nuevoRepuesto = this.getRepuestoInicial();
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  cerrarModalCrear(): void {
    this.mostrarModalCrear.set(false);
  }

  guardarRepuesto(): void {
    if (!this.authService.isAuthenticated()) {
      this.mensajeError.set('Debes iniciar sesión para registrar un nuevo repuesto.');
      return;
    }

    if (!this.nuevoRepuesto.nombre || !this.nuevoRepuesto.categoria || this.nuevoRepuesto.precioUnitario <= 0) {
      this.mensajeError.set('El nombre, la categoría y el precio son obligatorios.');
      return;
    }

    this.guardando.set(true);
    this.mensajeError.set(null);

    this.repuestoService.crearRepuesto(this.nuevoRepuesto).subscribe({
      next: (creado) => {
        this.guardando.set(false);
        this.mostrarModalCrear.set(false);
        this.mensajeCarrito.set(`¡Repuesto "${creado.nombre}" añadido con éxito al inventario!`);
        this.cargarRepuestos();
        setTimeout(() => this.mensajeCarrito.set(null), 4000);
      },
      error: (err) => {
        this.guardando.set(false);
        const detalle = err.error?.mensaje || 'Error al guardar el repuesto. Verifica los datos.';
        this.mensajeError.set(detalle);
      }
    });
  }

  eliminarRepuesto(id: number, nombre: string): void {
    if (!confirm(`¿Estás seguro de que deseas eliminar el repuesto "${nombre}"?`)) {
      return;
    }

    this.repuestoService.eliminarRepuesto(id).subscribe({
      next: () => {
        this.mensajeCarrito.set(`Repuesto "${nombre}" eliminado del catálogo.`);
        this.cargarRepuestos();
        setTimeout(() => this.mensajeCarrito.set(null), 3500);
      },
      error: (err) => {
        const detalle = err.error?.mensaje || 'No se puede eliminar el repuesto porque está asociado a ventas.';
        this.mensajeError.set(detalle);
      }
    });
  }

  agregarAlCarrito(repuesto: Repuesto): void {
    this.mensajeCarrito.set(`¡Se añadió "${repuesto.nombre}" al pedido!`);
    setTimeout(() => this.mensajeCarrito.set(null), 3500);
  }

  private getRepuestoInicial(): RepuestoCreate {
    return {
      nombre: '',
      descripcion: '',
      categoria: 'Frenos',
      unidadMedida: 'unidad',
      precioUnitario: 50000,
      stock: 10,
      activo: true
    };
  }
}
