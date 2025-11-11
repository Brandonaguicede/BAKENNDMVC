using System;
using System.Collections.Generic;

namespace AppPrint_and_Wear.Models
{
    public class HistorialComprasCliente
    {
        public string ClienteNombre { get; set; }
        public string ClienteEmail { get; set; }
        public int TotalCompras { get; set; }
        public decimal TotalGastado { get; set; }
        public List<FacturaClienteDto> Facturas { get; set; }

        public HistorialComprasCliente()
        {
            Facturas = new List<FacturaClienteDto>();
        }
    }

    public class FacturaClienteDto
    {
        public int FacturaId { get; set; }
        public string Folio { get; set; }
        public DateTime FechaEmision { get; set; }
        public double Total { get; set; }
        public string Estado { get; set; }
        public int CantidadProductos { get; set; }
        public List<ItemFacturaClienteDto> Detalles { get; set; }
    }

    public class ItemFacturaClienteDto
    {
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
        public string ImagenUrl { get; set; }
    }
}