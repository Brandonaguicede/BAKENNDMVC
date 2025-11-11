using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;

namespace AppPrint_and_Wear.Controllers
{
    public class Metodo_De_PagoController : Controller
    {
        private readonly ApplicationDBContext _context;

        public Metodo_De_PagoController(ApplicationDBContext context)
        {
            _context = context;
        }

        // =============================
        // LISTAR MÉTODOS DE PAGO
        // =============================
        public async Task<IActionResult> Index()
        {
            var metodos = await _context.Metodo_De_Pagos
                .Include(m => m.Cliente)
                .OrderBy(m => m.Cliente.Nombre)
                .ToListAsync();

            return View(metodos);
        }

        // =============================
        // DETALLES
        // =============================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var metodo = await _context.Metodo_De_Pagos
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.Metodo_De_PagoId == id);

            if (metodo == null)
                return NotFound();

            return View(metodo);
        }

        // =============================
        // CREAR MÉTODO DE PAGO
        // =============================
        public IActionResult Create(int? clienteId)
        {
            if (clienteId.HasValue)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", clienteId);
            }
            else
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Metodo_De_PagoId,Nombre,Metodo_Tipo,Numero_Tarjeta,ExpirationDate,CVV,ClienteId")] Metodo_De_Pago metodo)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", metodo.ClienteId);
                return View(metodo);
            }

            // Validar duplicados
            bool existe = await _context.Metodo_De_Pagos.AnyAsync(m =>
                m.ClienteId == metodo.ClienteId &&
                m.Numero_Tarjeta == metodo.Numero_Tarjeta);

            if (existe)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", metodo.ClienteId);
                return View(metodo);
            }

            _context.Add(metodo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // EDITAR MÉTODO DE PAGO
        // =============================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var metodo = await _context.Metodo_De_Pagos.FindAsync(id);
            if (metodo == null)
                return NotFound();

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", metodo.ClienteId);
            return View(metodo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Metodo_De_PagoId,Nombre,Metodo_Tipo,Numero_Tarjeta,ExpirationDate,CVV,ClienteId")] Metodo_De_Pago metodo)
        {
            if (id != metodo.Metodo_De_PagoId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", metodo.ClienteId);
                return View(metodo);
            }

            try
            {
                _context.Update(metodo);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Metodo_De_PagoExists(metodo.Metodo_De_PagoId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // ELIMINAR MÉTODO DE PAGO
        // =============================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var metodo = await _context.Metodo_De_Pagos
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.Metodo_De_PagoId == id);

            if (metodo == null)
                return NotFound();

            return View(metodo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var metodo = await _context.Metodo_De_Pagos.FindAsync(id);
            if (metodo != null)
            {
                _context.Metodo_De_Pagos.Remove(metodo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool Metodo_De_PagoExists(int id)
        {
            return _context.Metodo_De_Pagos.Any(e => e.Metodo_De_PagoId == id);
        }
    }
}
