using TuProyecto.Models;

namespace AppPrint_and_Wear.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int Cantidad { get; set; }

        // Relación con el carrito
        public int Carrito_De_Compra_Id { get; set; }
        public Carrito_De_Compra Carrito_De_Compra { get; set; }

        // Relación con el producto
        public int ProductoId { get; set; }
        public Producto Productos { get; set; }

        // Subtotal calculado automáticamente
        public double SubTotal { get; set; }

        public string Talla { get; set; }

        public string? ImagenPersonalizadaFrente { get; set; } // base64 del diseño frente
        public string? ImagenPersonalizadaEspalda { get; set; } // base64 del diseño espalda




    }

}
