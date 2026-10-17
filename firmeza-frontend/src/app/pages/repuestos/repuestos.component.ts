import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RepuestoService } from '../../services/repuesto.service';
import { Repuesto } from '../../models/repuesto.models';

@Component({
  standalone: true,
  imports: [CurrencyPipe, FormsModule],
  selector: 'app-repuestos',
  templateUrl: './repuestos.component.html'
})
export class RepuestosComponent implements OnInit {
  private readonly repuestoService = inject(RepuestoService);

  repuestos = signal<Repuesto[]>([]);
  cargando = signal<boolean>(true);
  mensajeError = signal<string | null>(null);

  // Filtros
  buscar = '';
  categoria = '';

  // Mensaje de éxito al comprar / agregar
  mensajeCarrito = signal<string | null>(null);

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
      error: (err) => {
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

  agregarAlCarrito(repuesto: Repuesto): void {
    this.mensajeCarrito.set(`¡Se añadió "${repuesto.nombre}" al pedido!`);
    setTimeout(() => this.mensajeCarrito.set(null), 3500);
  }
}
