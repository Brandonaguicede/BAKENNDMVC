using System.ComponentModel.DataAnnotations;
using TuProyecto.Models;

namespace AppPrint_and_Wear.Models
{
    public class DetalleFactura
    {
        public int DetalleFacturaId { get; set; }

        [Required]
        public string Nombre { get; set; }

        public int Cantidad { get; set; }

        public double PrecioUnitario { get; set; }

        public double Subtotal { get; set; }

        // Relaciones
        public int FacturaId { get; set; }
        public Factura Factura { get; set; }

        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
    }
}
