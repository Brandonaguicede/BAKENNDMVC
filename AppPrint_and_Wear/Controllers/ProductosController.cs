using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppPrint_and_Wear.Data;
using TuProyecto.Models;

namespace AppPrint_and_Wear.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDBContext _context;

        public ProductosController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var applicationDBContext = _context.Productos.Include(p => p.Categoria);
            return View(await applicationDBContext.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.ProductoId == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductoId,Descripcion,Stock,Precio,ImagenUrlFrende,ImagenUrlEspalda,CategoriaId")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Id", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Id", producto.CategoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductoId,Descripcion,Stock,Precio,ImagenUrlFrende,ImagenUrlEspalda,CategoriaId")] Producto producto)
        {
            if (id != producto.ProductoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.ProductoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias, "Id", "Id", producto.CategoriaId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(m => m.ProductoId == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoId == id);
        }

        [HttpGet]
        public IActionResult ObtenerPorCategoria(int id)
        {
            var productos = _context.Productos
                            .Where(p => p.CategoriaId == id)
                            .Select(p => new {
                                p.ProductoId,
                                p.Descripcion,
                                p.Stock,
                                p.Precio,
                                p.ImagenUrlFrende,
                                p.ImagenUrlEspalda
                            })
                            .ToList();

            return Json(productos);
        }

        [HttpGet]
        public IActionResult Diseno(int? productoId)
        {
            if (productoId == null) return RedirectToAction("Index", "Home");

            var producto = _context.Productos
                .FirstOrDefault(p => p.ProductoId == productoId);

            if (producto == null) return NotFound();

            return View(producto);
        }

        [HttpGet]
        public IActionResult Producto(int productoId)
        {
            var producto = _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefault(p => p.ProductoId == productoId);

            if (producto == null)
                return NotFound();

            // 🔹 Redirige a la acción "Diseno" del controlador Home
            return RedirectToAction("Diseno", "Home", new { productoId = producto.ProductoId });
        }
    }
}
