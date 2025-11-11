using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppPrint_and_Wear.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ReportesController(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string periodo = "diario", DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.Now;
            var modelo = new Reporte();

            // Calcular fechas según el período
            DateTime inicio, fin;
            switch (periodo.ToLower())
            {
                case "semanal":
                    inicio = fechaConsulta.AddDays(-(int)fechaConsulta.DayOfWeek);
                    fin = inicio.AddDays(7);
                    break;
                case "mensual":
                    inicio = new DateTime(fechaConsulta.Year, fechaConsulta.Month, 1);
                    fin = inicio.AddMonths(1);
                    break;
                default: // diario
                    inicio = fechaConsulta.Date;
                    fin = fechaConsulta.Date.AddDays(1);
                    break;
            }

            modelo.FechaInicio = inicio;
            modelo.FechaFin = fin;

            // ===========================================
            // VENTAS - Usando Facturas
            // ===========================================
            var facturas = await _context.Facturas
                .Include(f => f.Detalles)
                .Include(f => f.Pedido)
                .Where(f => f.FechaEmision >= inicio && f.FechaEmision < fin)
                .ToListAsync();

            modelo.TotalVentas = (decimal)facturas.Sum(f => f.Total);
            modelo.CantidadPedidos = facturas.Select(f => f.PedidoId).Distinct().Count();

            // Agrupar ventas por día
            modelo.VentasPorPeriodo = facturas
                .GroupBy(f => f.FechaEmision.Date)
                .Select(g => new VentaPorDia
                {
                    Fecha = g.Key,
                    Total = g.Sum(f => f.Total),
                    Pedidos = g.Select(f => f.PedidoId).Distinct().Count()
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            // ===========================================
            // INVENTARIO
            // ===========================================
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();

            modelo.TotalProductos = productos.Count;
            modelo.ProductosBajoStock = productos.Count(p => p.Stock < 10);

            modelo.ProductosInventario = productos
                .Select(p => new ProductoInventarioDto
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Descripcion,
                    Stock = p.Stock,
                    Categoria = p.Categoria?.Nombre ?? "Sin categoría",
                    Alerta = p.Stock < 5 ? "⚠️ Stock Bajo" : "✅ Disponible"
                })
                .OrderBy(p => p.Stock)
                .ToList();

            // ===========================================
            // PRODUCTOS MÁS VENDIDOS
            // ===========================================
            modelo.ProductosMasVendidos = await _context.DetalleFacturas
                .Include(df => df.Producto)
                .GroupBy(df => new { df.ProductoId, df.Producto.Descripcion })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    Nombre = g.Key.Descripcion,
                    Cantidad = g.Sum(df => df.Cantidad),
                    Total = g.Sum(df => df.Subtotal)
                })
                .OrderByDescending(p => p.Cantidad)
                .Take(10)
                .ToListAsync();

            ViewBag.Periodo = periodo;
            return View(modelo);
        }
    }
}