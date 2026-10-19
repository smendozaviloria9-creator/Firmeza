import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VentaService } from '../../services/venta.service';
import { ClienteService } from '../../services/cliente.service';
import { RepuestoService } from '../../services/repuesto.service';
import { VehiculoService } from '../../services/vehiculo.service';
import { AuthService } from '../../services/auth.service';
import { Venta, VentaCreate, VentaVehiculoCreate } from '../../models/venta.models';
import { Cliente } from '../../models/cliente.models';
import { Repuesto } from '../../models/repuesto.models';
import { Vehiculo } from '../../models/vehiculo.models';

interface ItemVentaRow {
  productoId: number;
  cantidad: number;
  precioUnitario: number;
  stockMax: number;
  nombre: string;
}

@Component({
  standalone: true,
  imports: [CurrencyPipe, DatePipe, FormsModule, RouterLink],
  selector: 'app-ventas',
  templateUrl: './ventas.component.html'
})
export class VentasComponent implements OnInit {
  private readonly ventaService = inject(VentaService);
  private readonly clienteService = inject(ClienteService);
  private readonly repuestoService = inject(RepuestoService);
  private readonly vehiculoService = inject(VehiculoService);
  readonly authService = inject(AuthService);

  ventas = signal<Venta[]>([]);
  cargando = signal<boolean>(true);
  mensajeExito = signal<string | null>(null);
  mensajeError = signal<string | null>(null);

  descargandoExcel = signal<boolean>(false);
  descargandoPdf = signal<boolean>(false);

  filtro = '';

  // Modal para Crear Venta
  mostrarModalVenta = signal<boolean>(false);
  guardandoVenta = signal<boolean>(false);
  tipoVenta = signal<'repuestos' | 'vehiculo'>('repuestos');

  clientes = signal<Cliente[]>([]);
  repuestos = signal<Repuesto[]>([]);
  vehiculosDisponibles = signal<Vehiculo[]>([]);

  // Datos del formulario - Repuestos
  clienteId = 0;
  itemsVenta: ItemVentaRow[] = [];

  // Datos del formulario - Vehículo
  vehiculoId = 0;
  vendedor = 'Asesor Firmeza';
  precioFinalVehiculo = 0;
  observaciones = 'Venta en concesionario';

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

  abrirModalCrear(tipo: 'repuestos' | 'vehiculo' = 'repuestos'): void {
    this.tipoVenta.set(tipo);
    this.mensajeError.set(null);

    // Cargar clientes
    this.clienteService.getClientes().subscribe({
      next: (data) => {
        this.clientes.set(data);
        if (data.length > 0 && !this.clienteId) {
          this.clienteId = data[0].id;
        }
      }
    });

    // Cargar repuestos con stock disponible
    this.repuestoService.getRepuestos().subscribe({
      next: (data) => {
        const disponibles = data.filter(r => r.stock > 0);
        this.repuestos.set(disponibles);
        this.itemsVenta = [];
        if (disponibles.length > 0) {
          this.agregarFilaItem();
        }
      }
    });

    // Cargar vehículos disponibles para la venta
    this.vehiculoService.getVehiculos(undefined, undefined, 'Disponible').subscribe({
      next: (data) => {
        this.vehiculosDisponibles.set(data);
        if (data.length > 0) {
          this.vehiculoId = data[0].id;
          this.precioFinalVehiculo = data[0].precioVenta;
        }
      }
    });

    this.mostrarModalVenta.set(true);
  }

  cerrarModalCrear(): void {
    this.mostrarModalVenta.set(false);
  }

  agregarFilaItem(): void {
    const lista = this.repuestos();
    if (lista.length === 0) return;

    // Buscar el primer repuesto que aún no esté en la lista o el primero disponible
    const rep = lista[0];
    this.itemsVenta.push({
      productoId: rep.id,
      cantidad: 1,
      precioUnitario: rep.precioUnitario,
      stockMax: rep.stock,
      nombre: rep.nombre
    });
  }

  eliminarFilaItem(index: number): void {
    if (this.itemsVenta.length > 1) {
      this.itemsVenta.splice(index, 1);
    }
  }

  onProductoChange(item: ItemVentaRow): void {
    const rep = this.repuestos().find(r => r.id === +item.productoId);
    if (rep) {
      item.precioUnitario = rep.precioUnitario;
      item.stockMax = rep.stock;
      item.nombre = rep.nombre;
      if (item.cantidad > rep.stock) {
        item.cantidad = rep.stock;
      }
    }
  }

  onVehiculoChange(): void {
    const auto = this.vehiculosDisponibles().find(v => v.id === +this.vehiculoId);
    if (auto) {
      this.precioFinalVehiculo = auto.precioVenta;
    }
  }

  getSubtotal(): number {
    return this.itemsVenta.reduce((acc, i) => acc + ((i.cantidad || 0) * (i.precioUnitario || 0)), 0);
  }

  getIva(): number {
    return Math.round(this.getSubtotal() * 0.19);
  }

  getTotal(): number {
    return this.getSubtotal() + this.getIva();
  }

  guardarVenta(): void {
    if (!this.authService.isAuthenticated()) {
      this.mensajeError.set('Debes iniciar sesión para registrar una venta.');
      return;
    }

    if (!this.clienteId) {
      this.mensajeError.set('Debes seleccionar un cliente.');
      return;
    }

    this.guardandoVenta.set(true);
    this.mensajeError.set(null);

    if (this.tipoVenta() === 'repuestos') {
      if (this.itemsVenta.length === 0) {
        this.mensajeError.set('Debes agregar al menos un repuesto a la orden.');
        this.guardandoVenta.set(false);
        return;
      }

      const payload: VentaCreate = {
        clienteId: +this.clienteId,
        fecha: new Date().toISOString(),
        items: this.itemsVenta.map(i => ({
          productoId: +i.productoId,
          cantidad: +i.cantidad
        }))
      };

      this.ventaService.crearVenta(payload).subscribe({
        next: (creada) => {
          this.guardandoVenta.set(false);
          this.mostrarModalVenta.set(false);
          this.mensajeExito.set(`¡Venta #${creada.id} registrada con éxito! Total facturado: $${creada.total.toLocaleString('es-CO')} COP.`);
          this.cargarVentas();
          setTimeout(() => this.mensajeExito.set(null), 5000);
        },
        error: (err) => {
          this.guardandoVenta.set(false);
          const detalle = err.error?.mensaje || 'Error al procesar la venta. Verifica el stock disponible.';
          this.mensajeError.set(detalle);
        }
      });
    } else {
      // Venta de Vehículo
      if (!this.vehiculoId || this.precioFinalVehiculo <= 0) {
        this.mensajeError.set('Debes seleccionar un vehículo válido con su precio final.');
        this.guardandoVenta.set(false);
        return;
      }

      const payload: VentaVehiculoCreate = {
        clienteId: +this.clienteId,
        vehiculoId: +this.vehiculoId,
        vendedor: this.vendedor || 'Asesor Firmeza',
        fecha: new Date().toISOString(),
        precioFinal: +this.precioFinalVehiculo,
        observaciones: this.observaciones
      };

      this.ventaService.crearVentaVehiculo(payload).subscribe({
        next: () => {
          this.guardandoVenta.set(false);
          this.mostrarModalVenta.set(false);
          this.mensajeExito.set('¡Venta de vehículo registrada con éxito! El vehículo ahora figura como Vendido.');
          this.cargarVentas();
          setTimeout(() => this.mensajeExito.set(null), 5000);
        },
        error: (err) => {
          this.guardandoVenta.set(false);
          const detalle = err.error?.mensaje || 'Error al registrar la venta del vehículo.';
          this.mensajeError.set(detalle);
        }
      });
    }
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
