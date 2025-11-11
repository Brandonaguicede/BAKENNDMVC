namespace AppPrint_and_Wear.Models
{
    public class Reporte
    {
        // Datos de Ventas
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }

        // Datos de Inventario
        public int TotalProductos { get; set; }
        public int ProductosBajoStock { get; set; }

        // Listas - SIN dynamic
        public List<VentaPorDia> VentasPorPeriodo { get; set; }
        public List<ProductoInventarioDto> ProductosInventario { get; set; }
        public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; }

        public Reporte()
        {
            VentasPorPeriodo = new List<VentaPorDia>();
            ProductosInventario = new List<ProductoInventarioDto>();
            ProductosMasVendidos = new List<ProductoMasVendidoDto>();
        }
    }

    // Clases auxiliares
    public class VentaPorDia
    {
        public DateTime Fecha { get; set; }
        public double Total { get; set; }
        public int Pedidos { get; set; }
    }

    public class ProductoInventarioDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public string Alerta { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public double Total { get; set; }
    }
}