using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;
using Microsoft.AspNetCore.Http;

namespace AppPrint_and_Wear.Controllers
{
    public class Carrito_De_CompraController : Controller
    {
        private readonly ApplicationDBContext _context;

        public Carrito_De_CompraController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: Carrito_De_Compra
        public async Task<IActionResult> Index()
        {
            // Trae los carritos con Cliente y Metodo_De_Pago relacionados
            var carritos = await _context.Carrito_De_Compras
                .Include(c => c.Cliente)
                .Include(c => c.Metodo_De_Pago)
                .Include(c => c.CartItems)
                .ToListAsync();

            // Actualiza el total de cada carrito (en caso de que haya cambios en los items)
            foreach (var carrito in carritos)
            {
                ActualizarTotalCarrito(carrito);
            }

            // Guarda los totales actualizados en la base de datos
            await _context.SaveChangesAsync();

            return View(carritos);
        }


        // GET: Carrito_De_Compra/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.Cliente)
                .Include(c => c.Metodo_De_Pago)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Productos)
                .FirstOrDefaultAsync(m => m.Carrito_De_CompraId == id);

            if (carrito == null)
                return NotFound();

            ActualizarTotalCarrito(carrito);
            await _context.SaveChangesAsync();

            return View(carrito);
        }

        // GET: Carrito_De_Compra/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre");
            ViewData["Metodo_De_PagoId"] = new SelectList(_context.Metodo_De_Pagos, "Metodo_De_PagoId", "Nombre");
            return View();
        }

        // POST: Carrito_De_Compra/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Carrito_De_CompraId,ClienteId,Metodo_De_PagoId")] Carrito_De_Compra carrito)
        {

            // Primero valida que el modelo recibido cumpla con las reglas definidas
            if (!ModelState.IsValid)
            {

                // Si no es válido, carga las listas desplegables para Cliente y Método de Pago
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", carrito.ClienteId);
                ViewData["Metodo_De_PagoId"] = new SelectList(_context.Metodo_De_Pagos, "Metodo_De_PagoId", "Nombre", carrito.Metodo_De_PagoId);
                return View(carrito);
            }

            // Busca en la base de datos un método de pago que coincida con el Id recibido y que pertenezca al cliente indicado
            var metodoPago = await _context.Metodo_De_Pagos
                .FirstOrDefaultAsync(m => m.Metodo_De_PagoId == carrito.Metodo_De_PagoId && m.ClienteId == carrito.ClienteId);


            // Si no encuentra ese método de pago o no pertenece al cliente
            if (metodoPago == null)
            {
                // Agrega un error general al modelo (no ligado a un campo específico)
                ModelState.AddModelError("", "El método de pago seleccionado no pertenece al cliente.");

                // Recarga las listas para que el usuario pueda corregir su selección
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", carrito.ClienteId);
                ViewData["Metodo_De_PagoId"] = new SelectList(_context.Metodo_De_Pagos, "Metodo_De_PagoId", "Nombre", carrito.Metodo_De_PagoId);
                return View(carrito);
            }

            // Inicializa el total del carrito a cero (porque es un nuevo carrito)
            carrito.Total = 0;

            
            _context.Add(carrito);// Agrega el carrito al contexto (prepara para insertar en la BD)
            await _context.SaveChangesAsync();    // Guarda los cambios en la base de datos de forma asíncrona

            return RedirectToAction(nameof(Index));    // Redirige al usuario a la acción Index (normalmente listado de carritos)

        }

        // GET: Carrito_De_Compra/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == id);

            if (carrito == null)
                return NotFound();

            ActualizarTotalCarrito(carrito);
            await _context.SaveChangesAsync();

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", carrito.ClienteId);
            ViewData["Metodo_De_PagoId"] = new SelectList(_context.Metodo_De_Pagos.Where(mp => mp.ClienteId == carrito.ClienteId), "Metodo_De_PagoId", "Nombre", carrito.Metodo_De_PagoId);

            return View(carrito);
        }

        // POST: Carrito_De_Compra/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Carrito_De_CompraId,ClienteId,Metodo_De_PagoId")] Carrito_De_Compra carrito)
        {
            if (id != carrito.Carrito_De_CompraId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ClienteId"] = new SelectList(_context.Clientes, "ClienteId", "Nombre", carrito.ClienteId);
                ViewData["Metodo_De_PagoId"] = new SelectList(_context.Metodo_De_Pagos, "Metodo_De_PagoId", "Nombre", carrito.Metodo_De_PagoId);
                return View(carrito);
            }

            try
            {
                var carritoConItems = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == id);

                if (carritoConItems != null)
                {
                    carritoConItems.ClienteId = carrito.ClienteId;
                    carritoConItems.Metodo_De_PagoId = carrito.Metodo_De_PagoId;
                    ActualizarTotalCarrito(carritoConItems);
                    _context.Update(carritoConItems);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Carrito_De_CompraExists(carrito.Carrito_De_CompraId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Carrito_De_Compra/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.Cliente)
                .Include(c => c.Metodo_De_Pago)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(m => m.Carrito_De_CompraId == id);

            if (carrito == null)
                return NotFound();

            ActualizarTotalCarrito(carrito);
            await _context.SaveChangesAsync();

            return View(carrito);
        }

        // POST: Carrito_De_Compra/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carrito = await _context.Carrito_De_Compras.FindAsync(id);
            if (carrito != null)
                _context.Carrito_De_Compras.Remove(carrito);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void ActualizarTotalCarrito(Carrito_De_Compra carrito)
        {

            // Verifica que el carrito no sea nulo y que tenga items en la lista CartItems
            if (carrito != null && carrito.CartItems != null && carrito.CartItems.Any())
            {
                double subtotal = carrito.CartItems.Sum(item => item.SubTotal); // Suma todos los subtotales de los items del carrito
                double iva = subtotal * 0.13; // Calcula el IVA (impuesto) del 13% sobre el subtotal
                double envio = subtotal >= 25000 ? 0 : 2500;  // Determina el costo de envío
                carrito.Total = subtotal + iva + envio;// Asigna el total del carrito como la suma de subtotal + IVA + envío

            }
            else if (carrito != null)
            {
                carrito.Total = 0; // Si no hay items, el total del carrito es 0

            }

        }

        private bool Carrito_De_CompraExists(int id)
        {
            return _context.Carrito_De_Compras.Any(e => e.Carrito_De_CompraId == id);
        }
        public async Task<IActionResult> Carrito()
        {

            // Obtener el ID del cliente almacenado en sesión, si existe
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            // Obtener el ID del carrito almacenado en sesión, si existe
            int? carritoIdSesion = HttpContext.Session.GetInt32("CarritoId");

            Carrito_De_Compra carrito = null;


            // Si el usuario está logueado (tiene clienteId)
            if (clienteId.HasValue)
            {
                // Buscar en la base de datos el carrito asignado a ese cliente, incluyendo los items y los productos relacionados
                carrito = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Productos)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);

                if (carrito == null)
                {
                  
                }

                if (carrito != null)
                    HttpContext.Session.SetInt32("CarritoId", carrito.Carrito_De_CompraId);
                else
                    HttpContext.Session.Remove("CarritoId");  // Si no hay carrito, remover cualquier carrito guardado en sesión

            }


            // Si el usuario no está logueado
            else
            {
                if (carritoIdSesion.HasValue)   // Si hay un carrito guardado en sesión (un carrito anónimo)
                {

                    // Buscar ese carrito anónimo en la base de datos (carrito sin cliente asignado)
                    carrito = await _context.Carrito_De_Compras
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Productos)
                        .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoIdSesion.Value && c.ClienteId == null);
                }



                // Si no existe carrito anónimo en la base de datos, crear uno nuevo vacío
                if (carrito == null)
                {
                    // Crear carrito anónimo nuevo
                    carrito = new Carrito_De_Compra
                    {
                        CartItems = new List<CartItem>(),
                        Total = 0
                    };

                    // Agregar el carrito nuevo a la base de datos y guardar cambios
                    _context.Carrito_De_Compras.Add(carrito);
                    await _context.SaveChangesAsync();


                    // Guardar el ID del nuevo carrito anónimo en sesión
                    HttpContext.Session.SetInt32("CarritoId", carrito.Carrito_De_CompraId);
                }
            }


            // Si hay un carrito (sea del cliente o anónimo)
            if (carrito != null)
            {
                // Actualizar el total del carrito (subtotal + impuestos + envío)
                ActualizarTotalCarrito(carrito);
                await _context.SaveChangesAsync(); // Guardar los cambios en la base de datos

            }

            return View(carrito);// Retornar la vista con el carrito para mostrarlo al usuario

        }



        [HttpGet]
        public async Task<JsonResult> ObtenerCarrito()
        {

            // Obtener el ID del cliente almacenado en sesión (si está logueado)
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            // Obtener el ID del carrito almacenado en sesión (puede ser carrito anónimo)
            int? carritoIdSesion = HttpContext.Session.GetInt32("CarritoId");

            Carrito_De_Compra carrito = null;

            if (clienteId.HasValue)
            {
                // Si el usuario está logueado, buscar el carrito asignado a ese cliente
                carrito = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems) //incluit items
                        .ThenInclude(ci => ci.Productos)//incluir productos
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);//buscar carrito por client
            }
            else if (carritoIdSesion.HasValue)
            {

                // Si el usuario no está logueado pero tiene carrito en sesión,
                // buscar el carrito anónimo (sin cliente asignado) por su ID
                carrito = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Productos)
                    .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoIdSesion.Value && c.ClienteId == null);
            }

            // Si no se encontró carrito o no tiene items, devolver respuesta JSON vacía y éxito false
            if (carrito == null || !carrito.CartItems.Any())
                return Json(new { success = false, items = new List<object>() });


            // Proyectar los items del carrito a un objeto anónimo con los datos necesarios para el frontend
            var items = carrito.CartItems.Select(ci => new
            {
                ci.CartItemId,
                ci.Cantidad,
                ci.Talla,
                ci.ImagenPersonalizadaFrente,
                ci.ImagenPersonalizadaEspalda,
                Producto = new
                {
                    ci.Productos.ProductoId,
                    ci.Productos.Descripcion,
                    ci.Productos.Precio,
                    ci.Productos.ImagenUrlFrende,
                    ci.Productos.Stock,
                    ci.Productos.ImagenUrlEspalda
                }
            }).ToList();



            // Devolver respuesta JSON con éxito true, id del carrito, total y lista de items
            return Json(new
            {
                success = true,
                carritoId = carrito.Carrito_De_CompraId,
                total = carrito.Total,
                items
            });
        }




        [HttpPost]
        public async Task<JsonResult> ActualizarCantidadItem([FromBody] CantidadUpdateModel model)
        {

            // Validar que el modelo no sea nulo y que los valores de CartItemId y Cantidad sean válidos (> 0)
            if (model == null || model.CartItemId <= 0 || model.Cantidad <= 0)
                return Json(new { success = false, message = "Datos inválidos." });


            // Buscar el item del carrito por su ID, incluyendo el carrito y el producto relacionado
            var cartItem = await _context.CartItems
                .Include(ci => ci.Carrito_De_Compra)
                .Include(ci => ci.Productos)
                .FirstOrDefaultAsync(ci => ci.CartItemId == model.CartItemId);


            // Si no se encuentra el item, devolver error
            if (cartItem == null)
                return Json(new { success = false, message = "Item no encontrado." });

            int nuevaCantidad = model.Cantidad;

            // No permitir menos de 1
            if (nuevaCantidad < 1)
                nuevaCantidad = 1;

            // Obtener el stock disponible del producto
            int stockDisponible = cartItem.Productos.Stock;

            // No permitir cantidad mayor al stock disponible
            if (nuevaCantidad > stockDisponible)
                nuevaCantidad = stockDisponible;



            // Actualizar la cantidad del item y recalcular el subtotal (cantidad * precio)
            cartItem.Cantidad = nuevaCantidad;
            cartItem.SubTotal = cartItem.Cantidad * (cartItem.Productos?.Precio ?? 0);

            _context.Update(cartItem); // Marcar el item como modificado para actualizar en base de datos


            var carrito = cartItem.Carrito_De_Compra;
            if (carrito != null)
            {
                await _context.Entry(carrito).Collection(c => c.CartItems).LoadAsync(); // Cargar los items del carrito para recalcular el total

                double subtotal = carrito.CartItems.Sum(i => i.SubTotal);// Calcular subtotal sumando todos los subtotales de los items
                double iva = subtotal * 0.13;// Calcular IVA al 13%
                double envio = subtotal >= 25000 ? 0 : 2500;// Calcular costo de envío: gratis si subtotal >= 25000, sino 2500
                carrito.Total = subtotal + iva + envio; // Actualizar el total del carrito
                _context.Update(carrito); // Marcar carrito como modificado para actualizar en base de datos
            }

            await _context.SaveChangesAsync(); // Guardar todos los cambios en la base de datos

            return Json(new { success = true, cantidad = cartItem.Cantidad, stock = stockDisponible }); // Devolver respuesta JSON con éxito, cantidad actualizada y stock disponible

        }


        public class CantidadUpdateModel
        {
            public int CartItemId { get; set; }
            public int Cantidad { get; set; }
        }





        [HttpPost]
        public async Task<JsonResult> AgregarDesdeFrontend([FromBody] CartItem item)
        {

            // Validar que el objeto recibido no sea nulo
            if (item == null)
                return new JsonResult(new { success = false, message = "Datos inválidos." });


            // Buscar el producto en la base por su ID para validar que exista
            var producto = await _context.Productos.FindAsync(item.ProductoId);
            if (producto == null)
                return new JsonResult(new { success = false, message = "Producto no encontrado." });


            // Obtener ClienteId y CarritoId desde sesión (si hay)
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");
            int? carritoIdSesion = HttpContext.Session.GetInt32("CarritoId");

            Carrito_De_Compra carrito = null;

            if (clienteId.HasValue)
            {
                // Usuario logueado: buscar carrito asociado al cliente
                carrito = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);

                if (carrito == null)
                {

                    // Si no existe carrito para el cliente, crear uno nuevo
                    carrito = new Carrito_De_Compra
                    {
                        ClienteId = clienteId.Value,
                        Total = 0
                    };
                    _context.Carrito_De_Compras.Add(carrito);
                    await _context.SaveChangesAsync();


                    // Guardar el ID del carrito en sesión para futuras consultas
                    HttpContext.Session.SetInt32("CarritoId", carrito.Carrito_De_CompraId);
                }
            }
            else
            {
                // Usuario no logueado: usar carrito anónimo guardado en sesión
                if (carritoIdSesion.HasValue)
                {
                    carrito = await _context.Carrito_De_Compras
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoIdSesion.Value && c.ClienteId == null);
                }

                if (carrito == null)
                {


                    // Si no hay carrito anónimo, crear uno nuevo
                    carrito = new Carrito_De_Compra { Total = 0 };
                    _context.Carrito_De_Compras.Add(carrito);
                    await _context.SaveChangesAsync();


                    // Guardar el nuevo carrito anónimo en sesión
                    HttpContext.Session.SetInt32("CarritoId", carrito.Carrito_De_CompraId);
                }
            }


            // Validar cantidad (mínimo 1)
            var cantidad = item.Cantidad > 0 ? item.Cantidad : 1;



            // Crear nuevo item para el carrito con la info del producto y personalizado
            var nuevoItem = new CartItem
            {
                ProductoId = producto.ProductoId,
                Cantidad = cantidad,
                Talla = item.Talla,
                Carrito_De_Compra_Id = carrito.Carrito_De_CompraId,
                SubTotal = producto.Precio * cantidad,
                ImagenPersonalizadaFrente = item.ImagenPersonalizadaFrente,
                ImagenPersonalizadaEspalda = item.ImagenPersonalizadaEspalda
            };


            // Agregar el item al contexto para que sea guardado en la base de datos
            _context.CartItems.Add(nuevoItem);
            await _context.SaveChangesAsync();

            // Actualizar total del carrito
            ActualizarTotalCarrito(carrito);
            await _context.SaveChangesAsync();

            var respuesta = new
            {
                success = true,
                nuevoItem = new
                {
                    nuevoItem.CartItemId,
                    nuevoItem.Cantidad,
                    nuevoItem.Talla,
                    nuevoItem.SubTotal,
                    nuevoItem.ImagenPersonalizadaFrente,
                    nuevoItem.ImagenPersonalizadaEspalda,
                    Productos = new
                    {
                        producto.ProductoId,
                        producto.Descripcion,
                        producto.Precio,
                        producto.ImagenUrlFrende,
                        producto.ImagenUrlEspalda
                    }
                }
            };

            return new JsonResult(respuesta); // Retornar JSON con éxito y datos del item agregado

        }

        [HttpPost]
        public async Task<JsonResult> VaciarCarrito()
        {


            // Buscar un carrito, incluyendo sus items (aquí podría mejorarse para buscar el carrito específico del usuario o sesión)
            var carrito = await _context.Carrito_De_Compras
                            .Include(c => c.CartItems)
                            .FirstOrDefaultAsync();


            // Si no se encontró ningún carrito, devolver error
            if (carrito == null)
                return Json(new { success = false, message = "Carrito no encontrado." });


            // Remover todos los items asociados a ese carrito de la base de datos
            _context.CartItems.RemoveRange(carrito.CartItems);


            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> EliminarDesdeFrontend([FromBody] int id)
        {
            if (id <= 0) // Validar que el ID recibido sea válido (mayor que cero)

                return Json(new { success = false, message = "ID inválido." });


            // Buscar el item del carrito por su ID
            var cartItem = await _context.CartItems.FindAsync(id);


            // Si no se encontró el item, devolver error
            if (cartItem == null)
                return Json(new { success = false, message = "Item no encontrado." });

            _context.CartItems.Remove(cartItem);  // Eliminar el item del contexto (base de datos)

            await _context.SaveChangesAsync();  // Guardar los cambios en la base de datos de forma asíncrona


            return Json(new { success = true });
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Login([FromBody] Cliente cliente)
        {
            try
            {


                // Validar que el objeto cliente y sus datos esenciales no estén vacíos
                if (cliente == null || string.IsNullOrWhiteSpace(cliente.Correo) || string.IsNullOrWhiteSpace(cliente.Contraseña))
                    return Json(new { success = false, message = "Datos incompletos." });



                // Buscar en la base de datos un cliente con el correo y contraseña ingresados
                var user = await _context.Clientes.FirstOrDefaultAsync(c =>
                    c.Correo == cliente.Correo && c.Contraseña == cliente.Contraseña);

                if (user == null)
                    return Json(new { success = false, message = "Correo o contraseña incorrectos." });

                // Guardar información del cliente en la sesión para identificarlo en futuras peticiones
                HttpContext.Session.SetInt32("ClienteId", user.ClienteId);
                HttpContext.Session.SetString("ClienteNombre", user.Nombre);



                // Revisar si hay un carrito anónimo en sesión (antes de loguearse)
                int? carritoAnonimo = HttpContext.Session.GetInt32("CarritoId");
                Carrito_De_Compra carritoAnonimoObj = null;

                if (carritoAnonimo.HasValue)
                {

                    // Cargar el carrito anónimo con sus items para migrarlo si es necesario
                    carritoAnonimoObj = await _context.Carrito_De_Compras
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoAnonimo.Value && c.ClienteId == null);
                }

                // Buscar o crear el carrito asignado al cliente ya logueado
                var carritoUsuario = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.ClienteId == user.ClienteId);

                if (carritoUsuario == null)
                {


                    // Si no existe, crear uno nuevo con el ClienteId asignado
                    carritoUsuario = new Carrito_De_Compra
                    {
                        ClienteId = user.ClienteId,
                        CartItems = new List<CartItem>(),
                        Total = 0
                    };
                    _context.Carrito_De_Compras.Add(carritoUsuario);
                    await _context.SaveChangesAsync();
                }



                // Si hay un carrito anónimo con items, migrarlos al carrito del usuario
                if (carritoAnonimoObj != null && carritoAnonimoObj.CartItems != null && carritoAnonimoObj.CartItems.Any())
                {
                    foreach (var item in carritoAnonimoObj.CartItems)
                    {
                        var nuevoItem = new CartItem
                        {
                            ProductoId = item.ProductoId,
                            Cantidad = item.Cantidad,
                            Talla = item.Talla,
                            Carrito_De_Compra_Id = carritoUsuario.Carrito_De_CompraId,
                            SubTotal = item.SubTotal,
                            ImagenPersonalizadaFrente = item.ImagenPersonalizadaFrente,
                            ImagenPersonalizadaEspalda = item.ImagenPersonalizadaEspalda
                        };

                        carritoUsuario.CartItems.Add(nuevoItem);
                    }

                    // Actualizar total del carrito del usuario con los items migrados
                    ActualizarTotalCarrito(carritoUsuario);


                    // Eliminar los items y carrito anónimo para evitar duplicados
                    _context.CartItems.RemoveRange(carritoAnonimoObj.CartItems);
                    _context.Carrito_De_Compras.Remove(carritoAnonimoObj);

                    await _context.SaveChangesAsync();
                }


                // Actualizar la sesión con el carrito del usuario logueado
                HttpContext.Session.Remove("CarritoId");
                HttpContext.Session.SetInt32("CarritoId", carritoUsuario.Carrito_De_CompraId);

                return Json(new { success = true, message = "Inicio de sesión exitoso." });
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                string mensajes = "";

                while (inner != null)
                {
                    mensajes += inner.Message + " | ";
                    inner = inner.InnerException;
                }

                return Json(new { success = false, message = "Error interno: " + mensajes });
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromBody] Cliente nuevo)
        {
            // Validar que el objeto recibido no sea nulo
            if (nuevo == null)
                return Json(new { success = false, message = "Datos inválidos." });

            // Validar que los campos obligatorios no estén vacíos o en blanco
            if (string.IsNullOrWhiteSpace(nuevo.Nombre) ||
                string.IsNullOrWhiteSpace(nuevo.Apellidos) ||
                string.IsNullOrWhiteSpace(nuevo.Correo) ||
                string.IsNullOrWhiteSpace(nuevo.Contraseña))
            {
                return Json(new { success = false, message = "Todos los campos son obligatorios." });
            }

            // Verificar que el correo no esté ya registrado en la base de datos
            if (_context.Clientes.Any(c => c.Correo == nuevo.Correo))
                return Json(new { success = false, message = "El correo ya está registrado." });

            // Agregar el nuevo cliente a la base de datos
            _context.Clientes.Add(nuevo);
            await _context.SaveChangesAsync();

            // ===== NUEVO: INICIAR SESIÓN AUTOMÁTICAMENTE =====
            HttpContext.Session.SetInt32("ClienteId", nuevo.ClienteId);
            HttpContext.Session.SetString("ClienteNombre", nuevo.Nombre);

            // Revisar si hay un carrito anónimo en sesión
            int? carritoAnonimo = HttpContext.Session.GetInt32("CarritoId");
            Carrito_De_Compra carritoAnonimoObj = null;

            if (carritoAnonimo.HasValue)
            {
                carritoAnonimoObj = await _context.Carrito_De_Compras
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.Carrito_De_CompraId == carritoAnonimo.Value && c.ClienteId == null);
            }

            // Crear carrito para el nuevo usuario
            var carritoUsuario = new Carrito_De_Compra
            {
                ClienteId = nuevo.ClienteId,
                CartItems = new List<CartItem>(),
                Total = 0
            };
            _context.Carrito_De_Compras.Add(carritoUsuario);
            await _context.SaveChangesAsync();

            // Si hay carrito anónimo con items, migrarlos al carrito del nuevo usuario
            if (carritoAnonimoObj != null && carritoAnonimoObj.CartItems != null && carritoAnonimoObj.CartItems.Any())
            {
                foreach (var item in carritoAnonimoObj.CartItems)
                {
                    var nuevoItem = new CartItem
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        Talla = item.Talla,
                        Carrito_De_Compra_Id = carritoUsuario.Carrito_De_CompraId,
                        SubTotal = item.SubTotal,
                        ImagenPersonalizadaFrente = item.ImagenPersonalizadaFrente,
                        ImagenPersonalizadaEspalda = item.ImagenPersonalizadaEspalda
                    };

                    carritoUsuario.CartItems.Add(nuevoItem);
                }

                // Actualizar total del carrito
                ActualizarTotalCarrito(carritoUsuario);

                // Eliminar carrito anónimo
                _context.CartItems.RemoveRange(carritoAnonimoObj.CartItems);
                _context.Carrito_De_Compras.Remove(carritoAnonimoObj);

                await _context.SaveChangesAsync();
            }

            // Actualizar sesión con el carrito del usuario
            HttpContext.Session.Remove("CarritoId");
            HttpContext.Session.SetInt32("CarritoId", carritoUsuario.Carrito_De_CompraId);

            return Json(new
            {
                success = true,
                message = "Registro exitoso. Redirigiendo a finalizar compra...",
                redirect = true
            });
        }


        [HttpGet]
        public IActionResult VerificarSesion()
        {
            var logueado = HttpContext.Session.GetInt32("ClienteId") != null;// Revisar si en la sesión existe un ClienteId, eso indica si el usuario está logueado
            var nombre = HttpContext.Session.GetString("ClienteNombre"); // Obtener el nombre del cliente desde la sesión, puede ser null si no está logueado
            return Json(new { estaLogueado = logueado, nombre });
        }


        [HttpPost]
        public async Task<IActionResult> CrearOAsignarCarrito()
        {
            var clienteId = ObtenerClienteIdDesdeSesionOContexto();
            if (clienteId == null)
                return Unauthorized();

            // Buscar carrito activo para cliente, cualquiera que exista sin importar estado
            var carrito = await _context.Carrito_De_Compras
                .Include(c => c.CartItems)  
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

            if (carrito == null)
            {
                carrito = new Carrito_De_Compra
                {
                    ClienteId = clienteId.Value,
                    CartItems = new List<CartItem>(), 
                    Total = 0 
                };
                _context.Carrito_De_Compras.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, carritoId = carrito.Carrito_De_CompraId });
        }

        private int? ObtenerClienteIdDesdeSesionOContexto()
        {
            return HttpContext.Session.GetInt32("ClienteId");
        }



       
        [HttpPost]
        public async Task<JsonResult> AsignarMetodoPagoAlCarrito([FromBody] AsignarMetodoPagoRequest request)
        {
            try
            {
                int? clienteId = HttpContext.Session.GetInt32("ClienteId");

                if (!clienteId.HasValue)
                    return Json(new { success = false, message = "Debes iniciar sesión" });

                // Validar que la tarjeta pertenezca al cliente
                var metodoPago = await _context.Metodo_De_Pagos
                    .FirstOrDefaultAsync(mp => mp.Metodo_De_PagoId == request.MetodoPagoId
                                            && mp.ClienteId == clienteId.Value);

                if (metodoPago == null)
                    return Json(new { success = false, message = "Método de pago no válido" });

                // Buscar el carrito del cliente
                var carrito = await _context.Carrito_De_Compras
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);

                if (carrito == null)
                    return Json(new { success = false, message = "Carrito no encontrado" });

                // Asignar el método de pago al carrito
                carrito.Metodo_De_PagoId = request.MetodoPagoId;

                _context.Carrito_De_Compras.Update(carrito);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Método de pago asignado correctamente",
                    metodoPagoId = request.MetodoPagoId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Clase auxiliar para recibir el request
        public class AsignarMetodoPagoRequest
        {
            public int MetodoPagoId { get; set; }
        }





        // GET: Carrito_De_Compra/MisCompras
        public async Task<IActionResult> MisCompras()
        {
            // Verificar que el usuario esté logueado
            int? clienteId = HttpContext.Session.GetInt32("ClienteId");

            if (!clienteId.HasValue)
            {
                TempData["Error"] = "Debes iniciar sesión para ver tu historial de compras";
                return RedirectToAction("Carrito");
            }

            // Obtener datos del cliente
            var cliente = await _context.Clientes.FindAsync(clienteId.Value);

            if (cliente == null)
            {
                TempData["Error"] = "Cliente no encontrado";
                return RedirectToAction("Carrito");
            }

            // Obtener todas las facturas del cliente con sus detalles
            var facturas = await _context.Facturas
                .Include(f => f.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(f => f.Pedido)
                .Where(f => f.ClienteId == clienteId.Value)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            // Crear el modelo para la vista
            var modelo = new HistorialComprasCliente
            {
                ClienteNombre = cliente.Nombre,
                ClienteEmail = cliente.Correo,
                TotalCompras = facturas.Count,
                TotalGastado = (decimal)facturas.Sum(f => f.Total),
                Facturas = facturas.Select(f => new FacturaClienteDto
                {
                    FacturaId = f.FacturaId,
                    Folio = f.Folio,
                    FechaEmision = f.FechaEmision,
                    Total = f.Total,
                    Estado = f.Pedido?.Estado ?? "Completado",
                    CantidadProductos = f.Detalles.Sum(d => d.Cantidad),
                    Detalles = f.Detalles.Select(d => new ItemFacturaClienteDto
                    {
                        ProductoNombre = d.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        ImagenUrl = d.Producto?.ImagenUrlFrende ?? "/images/no-image.png"
                    }).ToList()
                }).ToList()
            };

            return View(modelo);
        }



       

    }
}
