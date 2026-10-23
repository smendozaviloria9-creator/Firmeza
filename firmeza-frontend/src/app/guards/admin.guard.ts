import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.hasRole('Administrador')) {
    return true; // Acceso concedido al administrador
  }

  // Si es un cliente o no tiene rol administrador, lo redirige al catálogo de vehículos
  router.navigate(['/vehiculos']);
  return false;
};
