import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
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
  private readonly router = inject(Router);
  readonly authService = inject(AuthService);

  repuestos = signal<Repuesto[]>([]);
  cargando = signal<boolean>(true);
  guardando = signal<boolean>(false);
  mensajeError = signal<string | null>(null);
  mensajeCarrito = signal<string | null>(null);

  // Modal para agregar o editar repuesto
  mostrarModalCrear = signal<boolean>(false);
  modoEdicion = signal<boolean>(false);
  repuestoIdEnEdicion = signal<number | null>(null);

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
    this.modoEdicion.set(false);
    this.repuestoIdEnEdicion.set(null);
    this.nuevoRepuesto = this.getRepuestoInicial();
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  abrirModalEditar(item: Repuesto): void {
    this.modoEdicion.set(true);
    this.repuestoIdEnEdicion.set(item.id);
    this.nuevoRepuesto = {
      nombre: item.nombre,
      descripcion: item.descripcion || '',
      categoria: item.categoria,
      unidadMedida: item.unidadMedida,
      precioUnitario: item.precioUnitario,
      stock: item.stock,
      activo: item.activo
    };
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  cerrarModalCrear(): void {
    this.mostrarModalCrear.set(false);
    this.modoEdicion.set(false);
    this.repuestoIdEnEdicion.set(null);
  }

  guardarRepuesto(): void {
    if (!this.authService.isAuthenticated()) {
      this.mensajeError.set('Debes iniciar sesión para registrar o editar un repuesto.');
      return;
    }

    if (!this.nuevoRepuesto.nombre?.trim() || !this.nuevoRepuesto.categoria || this.nuevoRepuesto.precioUnitario <= 0) {
      this.mensajeError.set('El nombre, la categoría y el precio son obligatorios.');
      return;
    }

    this.guardando.set(true);
    this.mensajeError.set(null);

    if (this.modoEdicion() && this.repuestoIdEnEdicion()) {
      // Modo Actualización / Edición
      this.repuestoService.actualizarRepuesto(this.repuestoIdEnEdicion()!, this.nuevoRepuesto).subscribe({
        next: () => {
          this.guardando.set(false);
          this.mostrarModalCrear.set(false);
          this.mensajeCarrito.set(`¡Repuesto "${this.nuevoRepuesto.nombre}" actualizado con éxito!`);
          this.cargarRepuestos();
          setTimeout(() => this.mensajeCarrito.set(null), 4000);
        },
        error: (err) => {
          this.guardando.set(false);
          const detalle = err.error?.mensaje || 'Error al actualizar el repuesto. Verifica los datos.';
          this.mensajeError.set(detalle);
        }
      });
    } else {
      // Modo Creación
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
    this.router.navigate(['/ventas'], {
      queryParams: {
        tipo: 'repuesto',
        id: repuesto.id
      }
    });
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
