import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegistroRequest } from '../../models/auth.models';

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink],
  selector: 'app-registro',
  templateUrl: './registro.component.html'
})
export class RegistroComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  modelo: RegistroRequest = {
    nombreCompleto: '',
    email: '',
    password: ''
  };

  confirmarPassword = '';
  cargando = signal<boolean>(false);
  mensajeError = signal<string | null>(null);

  onSubmit(): void {
    if (!this.modelo.nombreCompleto || !this.modelo.email || !this.modelo.password) {
      this.mensajeError.set('Por favor completa todos los campos.');
      return;
    }

    if (this.modelo.password !== this.confirmarPassword) {
      this.mensajeError.set('Las contraseñas no coinciden.');
      return;
    }

    if (this.modelo.password.length < 6) {
      this.mensajeError.set('La contraseña debe tener al menos 6 caracteres.');
      return;
    }

    this.cargando.set(true);
    this.mensajeError.set(null);

    this.authService.registro(this.modelo).subscribe({
      next: () => {
        this.cargando.set(false);
        this.router.navigate(['/vehiculos']);
      },
      error: (err) => {
        this.cargando.set(false);
        const detalle = err.error?.mensaje || (typeof err.error === 'string' ? err.error : 'Error al registrar la cuenta.');
        this.mensajeError.set(detalle);
      }
    });
  }
}
