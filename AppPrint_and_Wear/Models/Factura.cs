using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppPrint_and_Wear.Models
{
    public class Factura
    {
        public int FacturaId { get; set; }

        [Required]
        public string Folio { get; set; } // Folio único

        [Required]
        public DateTime FechaEmision { get; set; }

        public double Total { get; set; }

        // Relación con Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        // Relación con Pedido
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        // Relación con los detalles
        public List<DetalleFactura> Detalles { get; set; } = new();
    }
}
