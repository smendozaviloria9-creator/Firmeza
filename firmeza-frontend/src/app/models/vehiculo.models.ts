export interface Vehiculo {
  id: number;
  marca: string;
  modelo: string;
  anio: number;
  placa: string;
  vin?: string;
  color?: string;
  tipo: string;
  kilometraje: number;
  precioVenta: number;
  precioRentaDia: number;
  estado: string; // 'Disponible', 'Vendido', 'Rentado'
  fechaRegistro: string;
}

export interface VehiculoCreate {
  marca: string;
  modelo: string;
  anio: number;
  placa: string;
  vin?: string;
  color?: string;
  tipo: string;
  kilometraje: number;
  precioVenta: number;
  precioRentaDia: number;
  estado: string;
}
