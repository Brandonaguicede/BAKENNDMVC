using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;
using AppPrint_and_Wear.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AppPrint_and_Wear.Services; // 👈 Importar

namespace AppPrint_and_Wear.Controllers
{
    public class PedidoController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly FacturaService _facturaService;


        public PedidoController(ApplicationDBContext context)
        {
            _context = context;
            _facturaService = new FacturaService(_context);
        }

        // ===========================================
        // GET: Pedido/Pedido
        // ===========================================
        public async Task<IActionResult> Pedido()
        {
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tu pedido";
                return RedirectToAction("Index", "Home");
            }

            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Productos)
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);

            if (carrito == null || !carrito.CartItems.Any())
            {
                TempData["Error"] = "Tu carrito está vacío";
                return RedirectToAction("Carrito", "Carrito_De_Compra");
            }

            // Cargar tarjetas del cliente
            ViewBag.TarjetasCliente = await _context.Metodo_De_Pagos
                .Where(mp => mp.ClienteId == clienteId.Value)
                .ToListAsync();

            return View(carrito);
        }

       
        // ===========================================
        // POST: Pedido/AgregarTarjeta
        // ===========================================
        [HttpPost]
        public async Task<IActionResult> AgregarTarjeta([FromBody] Metodo_De_Pago tarjeta)
        {
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            if (string.IsNullOrWhiteSpace(tarjeta.Numero_Tarjeta) ||
                string.IsNullOrWhiteSpace(tarjeta.CVV) ||
                string.IsNullOrWhiteSpace(tarjeta.Nombre))
            {
                return Json(new { success = false, message = "Todos los campos son obligatorios" });
            }

            if (tarjeta.Numero_Tarjeta.Length < 13 || tarjeta.Numero_Tarjeta.Length > 19)
            {
                return Json(new { success = false, message = "Número de tarjeta inválido" });
            }

            if (tarjeta.CVV.Length < 3 || tarjeta.CVV.Length > 4)
            {
                return Json(new { success = false, message = "CVV inválido" });
            }

            if (tarjeta.ExpirationDate < DateTime.Now)
            {
                return Json(new { success = false, message = "La tarjeta está vencida" });
            }

            // Asignar cliente
            tarjeta.ClienteId = clienteId.Value;

            // Determinar tipo de tarjeta basado en el primer dígito
            if (tarjeta.Numero_Tarjeta.StartsWith("4"))
                tarjeta.Metodo_Tipo = "Visa";
            else if (tarjeta.Numero_Tarjeta.StartsWith("5"))
                tarjeta.Metodo_Tipo = "Mastercard";
            else if (tarjeta.Numero_Tarjeta.StartsWith("3"))
                tarjeta.Metodo_Tipo = "American Express";
            else
                tarjeta.Metodo_Tipo = "Otra";

            _context.Metodo_De_Pagos.Add(tarjeta);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Tarjeta agregada exitosamente",
                tarjetaId = tarjeta.Metodo_De_PagoId,
                tarjeta = new
                {
                    tarjeta.Metodo_De_PagoId,
                    tarjeta.Nombre,
                    tarjeta.Metodo_Tipo,
                    UltimosDigitos = "****" + tarjeta.Numero_Tarjeta.Substring(tarjeta.Numero_Tarjeta.Length - 4)
                }
            });
        }

        // ===========================================
        // GET: Pedido/ObtenerTarjetas
        // ===========================================
        [HttpGet]
        public async Task<IActionResult> ObtenerTarjetas()
        {
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                return Json(new { success = false, message = "Debes iniciar sesión" });
            }

            var tarjetas = await _context.Metodo_De_Pagos
                .Where(mp => mp.ClienteId == clienteId.Value)
                .Select(mp => new {
                    mp.Metodo_De_PagoId,
                    mp.Nombre,
                    mp.Metodo_Tipo,
                    UltimosDigitos = "****" + mp.Numero_Tarjeta.Substring(mp.Numero_Tarjeta.Length - 4),
                    mp.ExpirationDate
                })
                .ToListAsync();

            return Json(new { success = true, tarjetas });
        }

        // ===========================================
        // GET: Pedido/Confirmacion/5
        // ===========================================
        public async Task<IActionResult> Confirmacion(int id)
        {
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión";
                return RedirectToAction("Index", "Home");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.Carrito_De_Compra)
                    .ThenInclude(c => c.Cliente)
                .Include(p => p.Carrito_De_Compra)
                    .ThenInclude(c => c.CartItems)
                        .ThenInclude(ci => ci.Productos)
                .Include(p => p.Metodo_De_Pago)
                .FirstOrDefaultAsync(p => p.PedidoId == id
                                       && p.Carrito_De_Compra.ClienteId == clienteId.Value);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido no encontrado o no tienes permiso para verlo";
                return RedirectToAction("Index", "Home");
            }

            return View(pedido);
        }

        // ===========================================
        // GET: Pedido/VerificarSesion
        // ===========================================
        [HttpGet]
        public IActionResult VerificarSesion()
        {
            var clienteId = HttpContext.Session.GetInt32("ClienteId");
            var clienteNombre = HttpContext.Session.GetString("ClienteNombre");

            if (clienteId == null)
            {
                return Json(new
                {
                    autenticado = false,
                    estaLogueado = false
                });
            }

            return Json(new
            {
                autenticado = true,
                estaLogueado = true,
                clienteId = clienteId.Value,
                clienteNombre = clienteNombre ?? "Usuario"
            });
        }




        [HttpPost]
        public async Task<IActionResult> ConfirmarPedido([FromBody] ConfirmarPedidoRequest request)
        {
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                return Json(new { success = false, message = "Debes iniciar sesión." });
            }

            // Cargar el carrito con TODOS los datos necesarios
            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Productos)
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == request.CarritoId && c.ClienteId == clienteId.Value);

            if (carrito == null || !carrito.CartItems.Any())
            {
                return Json(new { success = false, message = "Carrito no encontrado o vacío." });
            }

            // Validar método de pago
            int metodoPagoId;
            if (request.MetodoPagoId.HasValue)
            {
                var metodoPagoValido = await _context.Metodo_De_Pagos
                    .FirstOrDefaultAsync(mp => mp.Metodo_De_PagoId == request.MetodoPagoId.Value && mp.ClienteId == clienteId.Value);

                if (metodoPagoValido == null)
                {
                    return Json(new { success = false, message = "Método de pago inválido para este cliente." });
                }

                metodoPagoId = request.MetodoPagoId.Value;
            }
            else if (carrito.Metodo_De_PagoId != 0)
            {
                metodoPagoId = carrito.Metodo_De_PagoId.Value;
            }
            else
            {
                return Json(new { success = false, message = "Debe seleccionar un método de pago." });
            }

            // Verificar y disminuir stock
            foreach (var item in carrito.CartItems)
            {
                if (item.Productos != null)
                {
                    if (item.Productos.Stock < item.Cantidad)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"No hay suficiente stock para el producto: {item.Productos.Descripcion}"
                        });
                    }

                    item.Productos.Stock -= item.Cantidad;
                    _context.Productos.Update(item.Productos);
                }
            }

            await _context.SaveChangesAsync();

            //Crear el pedido
            var pedido = new Pedido
            {
                Carrito_De_CompraId = carrito.Carrito_De_CompraId,
                Metodo_De_PagoId = metodoPagoId,
                FechaPedido = DateTime.Now,
                Estado = "Confirmado"
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            try
            {
                // Generar factura ANTES de vaciar el carrito
                await _facturaService.GenerarFacturaAsync(pedido.PedidoId);

                // vaciar el carrito DESPUÉS de generar la factura
                _context.CartItems.RemoveRange(carrito.CartItems);
                carrito.Total = 0;
                _context.Carrito_De_Compras.Update(carrito);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "✅ ¡Compra realizada con éxito! Revisa tu correo para ver la factura.",
                    pedidoId = pedido.PedidoId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al generar factura: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = "Error al enviar la factura: " + ex.Message
                });
            }
        }






        [HttpPost]
        public async Task<IActionResult> FinalizarCompra(int pedidoId)
        {
            var facturaService = new FacturaService(_context);
            await facturaService.GenerarFacturaAsync(pedidoId);
            return RedirectToAction("Confirmacion");
        }

    }

    // ===========================================
    // Clases auxiliares
    // ===========================================
    public class ConfirmarPedidoRequest
    {
        public int CarritoId { get; set; }
        public int? MetodoPagoId { get; set; }
    }


}