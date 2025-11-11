namespace AppPrint_and_Wear.Models
{
    public class Cliente : Persona
    {
        public int ClienteId { get; set; }

          // 1 cliente → 1 carrito (opcional, si quieres permitir varios carritos puedes cambiarlo a List<Carrito_De_Compra>)
        public Carrito_De_Compra? Carritos_De_Compras { get; set; }

        // 1 cliente → muchos métodos de pago
        public List<Metodo_De_Pago> Metodo_De_Pagos { get; set; } = new List<Metodo_De_Pago>();
    }
}
