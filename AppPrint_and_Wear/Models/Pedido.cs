namespace AppPrint_and_Wear.Models
{
    public class Pedido
    {
        public int PedidoId { get; set; }
        public int Carrito_De_CompraId { get; set; } 
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } 

        public int? Metodo_De_PagoId { get; set; }
        public Metodo_De_Pago Metodo_De_Pago { get; set; }

        // Relación
        public virtual Carrito_De_Compra Carrito_De_Compra { get; set; }
    }
}