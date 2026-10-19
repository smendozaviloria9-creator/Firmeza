export interface DetalleVenta {
  id: number;
  productoId: number;
  productoNombre: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

export interface Venta {
  id: number;
  clienteId: number;
  clienteNombre: string;
  fecha: string;
  subtotal: number;
  iva: number;
  total: number;
  reciboArchivo?: string;
  detalles: DetalleVenta[];
}

export interface DetalleVentaCreate {
  productoId: number;
  cantidad: number;
}

export interface VentaCreate {
  clienteId: number;
  fecha: string;
  items: DetalleVentaCreate[];
}

export interface VentaVehiculoCreate {
  vehiculoId: number;
  clienteId: number;
  vendedor: string;
  fecha: string;
  precioFinal: number;
  observaciones?: string;
}
