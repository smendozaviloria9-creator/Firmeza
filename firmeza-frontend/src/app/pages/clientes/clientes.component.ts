import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ClienteService } from '../../services/cliente.service';
import { AuthService } from '../../services/auth.service';
import { Cliente, ClienteCreate } from '../../models/cliente.models';

@Component({
  standalone: true,
  imports: [FormsModule],
  selector: 'app-clientes',
  templateUrl: './clientes.component.html'
})
export class ClientesComponent implements OnInit {
  private readonly clienteService = inject(ClienteService);
  readonly authService = inject(AuthService);

  clientes = signal<Cliente[]>([]);
  cargando = signal<boolean>(true);
  guardando = signal<boolean>(false);
  mensajeExito = signal<string | null>(null);
  mensajeError = signal<string | null>(null);

  buscar = '';
  mostrarModalCrear = signal<boolean>(false);
  nuevoCliente: ClienteCreate = this.getClienteInicial();

  clientesFiltrados = computed(() => {
    const termino = this.buscar.trim().toLowerCase();
    if (!termino) return this.clientes();

    return this.clientes().filter(c =>
      c.nombres.toLowerCase().includes(termino) ||
      c.apellidos.toLowerCase().includes(termino) ||
      c.documento.toLowerCase().includes(termino) ||
      c.correo.toLowerCase().includes(termino) ||
      c.telefono.toLowerCase().includes(termino)
    );
  });

  ngOnInit(): void {
    this.cargarClientes();
  }

  cargarClientes(): void {
    this.cargando.set(true);
    this.mensajeError.set(null);

    this.clienteService.getClientes(this.buscar).subscribe({
      next: (data) => {
        this.clientes.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.cargando.set(false);
        this.mensajeError.set('Error al cargar la lista de clientes.');
      }
    });
  }

  abrirModalCrear(): void {
    this.nuevoCliente = this.getClienteInicial();
    this.mensajeError.set(null);
    this.mostrarModalCrear.set(true);
  }

  cerrarModalCrear(): void {
    this.mostrarModalCrear.set(false);
  }

  guardarCliente(): void {
    if (!this.nuevoCliente.nombres?.trim() || !this.nuevoCliente.apellidos?.trim()) {
      this.mensajeError.set('Ingresa nombres y apellidos válidos.');
      return;
    }
    if (!this.nuevoCliente.documento?.trim()) {
      this.mensajeError.set('El número de documento es obligatorio.');
      return;
    }
    if (!this.nuevoCliente.correo?.trim() || !this.nuevoCliente.correo.includes('@')) {
      this.mensajeError.set('Ingresa un correo electrónico válido.');
      return;
    }
    if (!this.nuevoCliente.telefono?.trim()) {
      this.mensajeError.set('El teléfono de contacto es obligatorio.');
      return;
    }
    if (!this.nuevoCliente.edad || this.nuevoCliente.edad < 18 || this.nuevoCliente.edad > 120) {
      this.mensajeError.set('La edad debe estar comprendida entre 18 y 120 años.');
      return;
    }

    this.guardando.set(true);
    this.mensajeError.set(null);

    this.clienteService.crearCliente(this.nuevoCliente).subscribe({
      next: (creado) => {
        this.guardando.set(false);
        this.mostrarModalCrear.set(false);
        this.mensajeExito.set(`Cliente "${creado.nombres} ${creado.apellidos}" registrado con éxito.`);
        this.cargarClientes();
        setTimeout(() => this.mensajeExito.set(null), 4000);
      },
      error: (err) => {
        this.guardando.set(false);
        const detalle = err.error?.mensaje || 'Error al registrar el cliente. Verifica que documento y correo no existan ya.';
        this.mensajeError.set(detalle);
      }
    });
  }

  eliminarCliente(id: number, nombreCompleto: string): void {
    if (!confirm(`¿Estás seguro de que deseas eliminar al cliente "${nombreCompleto}"?`)) {
      return;
    }

    this.clienteService.eliminarCliente(id).subscribe({
      next: () => {
        this.mensajeExito.set(`Cliente "${nombreCompleto}" eliminado.`);
        this.cargarClientes();
        setTimeout(() => this.mensajeExito.set(null), 4000);
      },
      error: (err) => {
        const detalle = err.error?.mensaje || 'No se pudo eliminar el cliente porque tiene ventas o registros asociados.';
        this.mensajeError.set(detalle);
      }
    });
  }

  private getClienteInicial(): ClienteCreate {
    return {
      nombres: '',
      apellidos: '',
      documento: '',
      correo: '',
      telefono: '',
      direccion: '',
      edad: 18
    };
  }
}
