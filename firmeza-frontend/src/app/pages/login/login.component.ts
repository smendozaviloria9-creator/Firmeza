import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/auth.models';

@Component({
  standalone: true,
  imports: [FormsModule, RouterLink],
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  modelo: LoginRequest = {
    email: '',
    password: ''
  };

  cargando = signal<boolean>(false);
  mensajeError = signal<string | null>(null);

  onSubmit(): void {
    if (!this.modelo.email || !this.modelo.password) {
      this.mensajeError.set('Por favor completa todos los campos.');
      return;
    }

    this.cargando.set(true);
    this.mensajeError.set(null);

    this.authService.login(this.modelo).subscribe({
      next: () => {
        this.cargando.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/vehiculos';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.cargando.set(false);
        this.mensajeError.set(err.error?.mensaje || 'Credenciales incorrectas o error en el servidor.');
      }
    });
  }
}
