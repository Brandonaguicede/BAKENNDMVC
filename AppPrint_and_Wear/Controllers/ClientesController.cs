using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;

namespace AppPrint_and_Wear.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ClientesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,Nombre,Apellidos,Contraseña,Telefono,Correo,Direccion,FechaNacimiento")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Clientes.AnyAsync(c => c.Correo == cliente.Correo);
                if (existe)
                {
                    TempData["MensajeError"] = "El correo ya está registrado. Por favor, inicie sesión.";
                    return RedirectToAction("Index", "Home");
                }

                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Limpiar carrito en sesión para evitar que quede uno de otra persona
                HttpContext.Session.Remove("CarritoId");

                TempData["MensajeExito"] = "¡Registro exitoso! Ahora inicia sesión.";
                return RedirectToAction("Index", "Home");
            }

            return View(cliente);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contraseña)
        {
            string empresaCorreo = "empresa@printwear.com";
            string empresaContraseña = "Empresa123";

            if (correo == empresaCorreo && contraseña == empresaContraseña)
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "HomeEmpresa") });
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Correo == correo);

            if (cliente == null)
                return Json(new { success = false, message = "El correo no está registrado. Por favor, regístrese." });

            if (cliente.Contraseña != contraseña)
                return Json(new { success = false, message = "Contraseña incorrecta." });

            // Guardar info cliente en sesión
            HttpContext.Session.SetInt32("ClienteId", cliente.ClienteId);
            HttpContext.Session.SetString("ClienteNombre", cliente.Nombre);

            // Obtener carrito en sesión (posiblemente anónimo)
            int? carritoSesionId = HttpContext.Session.GetInt32("CarritoId");

            // Buscar carrito asignado al cliente
            var carritoCliente = await _context.Carrito_De_Compras
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.ClienteId == cliente.ClienteId);

            if (carritoSesionId.HasValue)
            {
                var carritoSesion = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoSesionId.Value);

                if (carritoSesion != null)
                {
                    // Si carrito en sesión no tiene cliente y cliente no tiene carrito asignado: asignar
                    if (carritoSesion.ClienteId == null && carritoCliente == null)
                    {
                        carritoSesion.ClienteId = cliente.ClienteId;
                        await _context.SaveChangesAsync();
                        HttpContext.Session.SetInt32("CarritoId", carritoSesion.Carrito_De_CompraId);
                    }
                    // Si cliente tiene carrito diferente al carrito en sesión, fusionar o elegir uno
                    else if (carritoCliente != null && carritoCliente.Carrito_De_CompraId != carritoSesion.Carrito_De_CompraId)
                    {
                        // Aquí decides qué hacer con la fusión, o eliminar carrito anónimo
                        // Por ejemplo: eliminar carrito anónimo y usar carritoCliente

                        _context.Carrito_De_Compras.Remove(carritoSesion);
                        await _context.SaveChangesAsync();

                        HttpContext.Session.SetInt32("CarritoId", carritoCliente.Carrito_De_CompraId);
                    }
                    else
                    {
                        // El carrito en sesión ya pertenece al cliente
                        HttpContext.Session.SetInt32("CarritoId", carritoSesion.Carrito_De_CompraId);
                    }
                }
                else
                {
                    // Si carrito en sesión no existe (quizás fue eliminado), asignar carritoCliente
                    if (carritoCliente != null)
                        HttpContext.Session.SetInt32("CarritoId", carritoCliente.Carrito_De_CompraId);
                    else
                        HttpContext.Session.Remove("CarritoId");
                }
            }
            else
            {
                // No hay carrito en sesión: asignar carritoCliente o crear ninguno (como pediste)
                if (carritoCliente != null)
                {
                    HttpContext.Session.SetInt32("CarritoId", carritoCliente.Carrito_De_CompraId);
                }
                else
                {
                    HttpContext.Session.Remove("CarritoId");
                }
            }

            return Json(new { success = true, message = $"¡Bienvenido, {cliente.Nombre}!" });
        }


        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClienteId,Nombre,Apellidos,Contraseña,Telefono,Correo,Direccion,FechaNacimiento")] Cliente cliente)
        {
            if (id != cliente.ClienteId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.ClienteId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.ClienteId == id);
        }



    }
}
