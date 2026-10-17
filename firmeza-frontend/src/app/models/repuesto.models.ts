export interface Repuesto {
  id: number;
  nombre: string;
  descripcion?: string;
  categoria: string;
  unidadMedida: string;
  precioUnitario: number;
  stock: number;
  activo: boolean;
  fechaCreacion: string;
}

export interface RepuestoCreate {
  nombre: string;
  descripcion?: string;
  categoria: string;
  unidadMedida: string;
  precioUnitario: number;
  stock: number;
  activo: boolean;
}
