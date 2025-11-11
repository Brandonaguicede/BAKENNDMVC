namespace AppPrint_and_Wear.Models
{
    public class Carrito_De_Compra
    {
        public int Carrito_De_CompraId { get; set; }
        public double Total { get; set; }

        // Relación opcional con cliente
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // Relación opcional con envío

        // Relación con los ítems del carrito
        public List<CartItem> CartItems { get; set; } = new();

        // Relación opcional con método de pago
        public int? Metodo_De_PagoId { get; set; }
        public Metodo_De_Pago? Metodo_De_Pago { get; set; }
    }
}
