import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegistroComponent } from './pages/registro/registro.component';
import { VehiculosComponent } from './pages/vehiculos/vehiculos.component';
import { RepuestosComponent } from './pages/repuestos/repuestos.component';
import { VentasComponent } from './pages/ventas/ventas.component';
import { ClientesComponent } from './pages/clientes/clientes.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/vehiculos', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'registro', component: RegistroComponent },
  { path: 'vehiculos', component: VehiculosComponent, canActivate: [authGuard] },
  { path: 'repuestos', component: RepuestosComponent, canActivate: [authGuard] },
  { path: 'ventas', component: VentasComponent, canActivate: [authGuard] },
  { path: 'clientes', component: ClientesComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '/vehiculos' }
];
