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
