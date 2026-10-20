export interface Cliente {
  id: number;
  nombres: string;
  apellidos: string;
  documento: string;
  correo: string;
  telefono: string;
  direccion?: string;
  edad: number;
}

export interface ClienteCreate {
  nombres: string;
  apellidos: string;
  documento: string;
  correo: string;
  telefono: string;
  direccion?: string;
  edad: number;
}
