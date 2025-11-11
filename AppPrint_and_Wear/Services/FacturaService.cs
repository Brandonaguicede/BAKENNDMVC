using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AppPrint_and_Wear.Data;
using AppPrint_and_Wear.Models;
using iTextSharp.text;  // Librería para crear documentos PDF
using iTextSharp.text.pdf;

namespace AppPrint_and_Wear.Services
{
    //es service porque este código no maneja peticiones HTTP directamente,
    //sino que realiza la lógica de negocio (generar y enviar facturas)
    public class FacturaService
    {
        private readonly ApplicationDBContext _context;

        // Ruta del archivo plantilla HTML para la factura 
        private readonly string _templatePath;

        public FacturaService(ApplicationDBContext context)
        {
            _context = context;
            _templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "FacturaTemplate.html");
        }

        ///-----------------------------------------------------------------------------------------------------
        // generar la factura de un pedido dado con ID
        public async Task<Factura> GenerarFacturaAsync(int pedidoId)
        {
            // buscar el pedido en la base de datos, incluyendo:
            var pedido = await _context.Pedidos
                .Include(p => p.Carrito_De_Compra) // el carrito de compra asociado
                    .ThenInclude(c => c.CartItems)// los items del carrito y sus productos
                        .ThenInclude(ci => ci.Productos)
                .Include(p => p.Carrito_De_Compra.Cliente)//el cliente asociado al carrito
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pedido == null) throw new Exception("Pedido no encontrado.");

            // obtiene el cliente que hizo el pedido desde el carrito de compra
            var cliente = pedido.Carrito_De_Compra.Cliente;

            // crear una nueva instancia de factura con datos básicos
            var factura = new Factura
            {
                Folio = "FAC-" + DateTime.Now.ToString("yyyyMMddHHmmss"), //el folio es el año,mes,dia,hora y segundos
                FechaEmision = DateTime.Now, // fecha actual como fecha de emision
                ClienteId = cliente.ClienteId, //asocia el cliente a la factura
                PedidoId = pedido.PedidoId, //      ""    "" pedido "" ""   ""
                Total = pedido.Carrito_De_Compra.Total //total del carrito
            };

            // recorrer cada item en el carrito y agregar detalles a la factura
            foreach (var item in pedido.Carrito_De_Compra.CartItems)
            {
                factura.Detalles.Add(new DetalleFactura
                {
                    Nombre = item.Productos.Descripcion,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Productos.Precio,
                    Subtotal = item.Cantidad * item.Productos.Precio,
                    ProductoId = item.Productos.ProductoId
                });
            }

            //guarda la factura a la base de datos
            _context.Facturas.Add(factura);

            // guardar cambios en la base de datos de forma asíncrona
            await _context.SaveChangesAsync();

            // Generar PDF directamente (sin HTML)
            var pdfPath = await GenerarFacturaPDFDirectoAsync(factura, cliente, pedido.Carrito_De_Compra.CartItems);

            // Enviar por correo
            await EnviarFacturaPorCorreoAsync(cliente.Correo, pdfPath, factura.Folio);

            return factura;
        }

        ///-----------------------------------------------------------------------------------------------------
        private async Task<string> GenerarFacturaPDFDirectoAsync(Factura factura, Cliente cliente, ICollection<CartItem> cartItems)
        {
            // define la ruta temporal donde se guardará el PDF generado
            string path = Path.Combine(Path.GetTempPath(), $"Factura_{factura.Folio}.pdf");

            try
            {
                // Crear un archivo nuevo para escribir el PDF
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    // Crear el documento PDF con tamaño A4 y márgenes de 40 puntos
                    Document document = new Document(PageSize.A4, 40, 40, 40, 40);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    document.Open();

                    // Encabezado que llega al correo
                    try
                    {
                        // Usar ruta física en lugar de URL
                        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Imagenes", "Logo.png");

                        if (File.Exists(logoPath))
                        {
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                            logo.ScaleToFit(80f, 80f);
                            logo.Alignment = Element.ALIGN_CENTER;
                            document.Add(logo);
                        }
                        else
                        {
                            Console.WriteLine($"⚠ Logo no encontrado en: {logoPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"No se pudo cargar el logo: {ex.Message}");
                    }

                    // Crear y añadir el título de la factura 
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(243, 183, 0));
                    Paragraph title = new Paragraph("Factura Electrónica", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 5f; // Espacio debajo del título
                    document.Add(title);

                    // Añadir el folio de la factura con fuente más pequeña y color gris oscuro
                    var folioFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DARK_GRAY);
                    Paragraph folio = new Paragraph($"Folio: {factura.Folio}", folioFont);
                    folio.Alignment = Element.ALIGN_CENTER;
                    folio.SpacingAfter = 15f; // Espacio después del folio
                    document.Add(folio);

                    // Línea separadora
                    document.Add(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(2f, 100f, new BaseColor(243, 183, 0), Element.ALIGN_CENTER, -2)));
                    document.Add(new Paragraph("\n"));//salto de linea para la separacion

                    // ==================== INFORMACIÓN DEL CLIENTE ====================
                    // Definir fuentes para texto normal y en negrita
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);

                    // Crear una tabla con 2 columnas para mostrar datos del cliente
                    PdfPTable clienteTable = new PdfPTable(2);
                    clienteTable.WidthPercentage = 100; // Ancho completo
                    clienteTable.SpacingBefore = 10f;  // Espacio arriba
                    clienteTable.SpacingAfter = 15f; // Espacio abajo

                    // Agregar filas a la tabla con etiquetas y valores (nombre, correo, fecha)
                    AddClienteInfo(clienteTable, "Cliente:", cliente.Nombre, boldFont, normalFont);
                    AddClienteInfo(clienteTable, "Correo:", cliente.Correo, boldFont, normalFont);
                    AddClienteInfo(clienteTable, "Fecha:", factura.FechaEmision.ToString("dd/MM/yyyy HH:mm"), boldFont, normalFont);

                    // Añadir la tabla al documento PDF
                    document.Add(clienteTable);

                    // ==================== TABLA DE PRODUCTOS CON IMÁGENES ====================
                    // Crear tabla con 5 columnas para productos y detalles
                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100; // Ancho completo
                    table.SetWidths(new float[] { 28f, 28f, 12f, 16f, 16f }); //propiedades de cada columna

                    // Crear fuente para encabezados: negrita, tamaño pequeño, texto blanco
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);

                    // Añadir celdas de encabezado con títulos de cada columna
                    AddHeaderCell(table, "Producto", headerFont);
                    AddHeaderCell(table, "Diseños Personalizados", headerFont);
                    AddHeaderCell(table, "Cant.", headerFont);
                    AddHeaderCell(table, "Precio Unit.", headerFont);
                    AddHeaderCell(table, "Subtotal", headerFont);

                    // Fuentes para el contenido de las celdas
                    var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                    var smallFont = FontFactory.GetFont(FontFactory.HELVETICA, 7, BaseColor.DARK_GRAY);

                    // Recorrer cada producto del carrito para agregar fila con sus datos
                    foreach (var item in cartItems)
                    {
                        // Columna 1: Nombre del producto
                        PdfPCell cellProducto = new PdfPCell(new Phrase(item.Productos.Descripcion, cellFont));
                        cellProducto.Border = Rectangle.BOX; // Bordes visibles
                        cellProducto.Padding = 8f; // Espaciado interno
                        cellProducto.VerticalAlignment = Element.ALIGN_MIDDLE; // Centrado vertical
                        table.AddCell(cellProducto);

                        // Columna 2: imágenes personalizadas (frente y espalda)
                        PdfPCell cellImagenes = new PdfPCell();
                        cellImagenes.Border = Rectangle.BOX;
                        cellImagenes.Padding = 5f;
                        cellImagenes.HorizontalAlignment = Element.ALIGN_CENTER;
                        cellImagenes.VerticalAlignment = Element.ALIGN_MIDDLE;

                        // Crear una tabla interna para mostrar las imágenes lado a lado
                        PdfPTable imagenesTable = new PdfPTable(2);
                        imagenesTable.WidthPercentage = 100;

                        bool tieneImagenes = false;

                        // Si existe imagen personalizada para el frente, agregarla
                        if (!string.IsNullOrEmpty(item.ImagenPersonalizadaFrente))
                        {
                            try
                            {
                                var imgFrente = ConvertirBase64AImagen(item.ImagenPersonalizadaFrente);
                                if (imgFrente != null)
                                {
                                    imgFrente.ScaleToFit(60f, 60f);
                                    PdfPCell cellImgFrente = new PdfPCell();
                                    cellImgFrente.Border = Rectangle.NO_BORDER;
                                    cellImgFrente.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cellImgFrente.AddElement(imgFrente);
                                    cellImgFrente.AddElement(new Phrase("Frente", smallFont));
                                    imagenesTable.AddCell(cellImgFrente);
                                    tieneImagenes = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error cargando imagen frente: {ex.Message}");
                            }
                        }

                        // Si existe imagen personalizada para la espalda, agregarla
                        if (!string.IsNullOrEmpty(item.ImagenPersonalizadaEspalda))
                        {
                            try
                            {
                                var imgEspalda = ConvertirBase64AImagen(item.ImagenPersonalizadaEspalda);
                                if (imgEspalda != null)
                                {
                                    imgEspalda.ScaleToFit(60f, 60f);
                                    PdfPCell cellImgEspalda = new PdfPCell();
                                    cellImgEspalda.Border = Rectangle.NO_BORDER;
                                    cellImgEspalda.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cellImgEspalda.AddElement(imgEspalda);
                                    cellImgEspalda.AddElement(new Phrase("Espalda", smallFont));
                                    imagenesTable.AddCell(cellImgEspalda);
                                    tieneImagenes = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error cargando imagen espalda: {ex.Message}");
                            }
                        }

                        // Si se agregaron imágenes, añadir la tabla interna a la celda
                        if (tieneImagenes)
                        {
                            cellImagenes.AddElement(imagenesTable);
                        }
                        else
                        {
                            // Si no hay imágenes, mostrar texto que no hay diseño personalizado
                            var sinDisenoFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, Font.ITALIC, BaseColor.GRAY);
                            cellImagenes.AddElement(new Phrase("Sin diseño personalizado", sinDisenoFont));
                        }

                        // Añadir la celda con imágenes o texto a la tabla principal
                        table.AddCell(cellImagenes);

                        // Columna 3: Cantidad comprada
                        AddBodyCell(table, item.Cantidad.ToString(), cellFont);

                        // Columna 4: Precio Unitario
                        AddBodyCell(table, $"₡{item.Productos.Precio:N2}", cellFont);

                        // Columna 5: Subtotal
                        AddBodyCell(table, $"₡{(item.Cantidad * item.Productos.Precio):N2}", cellFont);
                    }

                    // Añadir la tabla con productos al documento PDF
                    document.Add(table);

                    // ==================== SECCIÓN DE TOTALES CON DESGLOSE ====================
                    document.Add(new Paragraph("\n")); // Salto de línea

                    // Calcular subtotal, IVA y envío
                    double subtotal = cartItems.Sum(item => item.Cantidad * item.Productos.Precio);
                    double iva = subtotal * 0.13;
                    double envio = subtotal >= 25000 ? 0 : 2500;
                    double totalFinal = subtotal + iva + envio;

                    // Crear tabla para mostrar el desglose de totales
                    PdfPTable totalesTable = new PdfPTable(2);
                    totalesTable.WidthPercentage = 45; // Ancho del 45%
                    totalesTable.HorizontalAlignment = Element.ALIGN_RIGHT; // Alineado a la derecha
                    totalesTable.SetWidths(new float[] { 60f, 40f }); // Columnas: 60% para etiqueta, 40% para valor

                    // Fuentes para los totales
                    var normalTotalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);
                    var boldTotalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);
                    var finalTotalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(243, 183, 0));

                    // **Subtotal**
                    AddTotalRow(totalesTable, "Subtotal:", $"₡{subtotal:N2}", normalTotalFont, normalTotalFont, false);

                    // **IVA (13%)**
                    AddTotalRow(totalesTable, "IVA (13%):", $"₡{iva:N2}", normalTotalFont, normalTotalFont, false);

                    // **Envío**
                    if (envio == 0)
                    {
                        var envioFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 150, 0)); // Verde
                        AddTotalRow(totalesTable, "Envío:", "GRATIS", normalTotalFont, envioFont, false);
                    }
                    else
                    {
                        AddTotalRow(totalesTable, "Envío:", $"₡{envio:N2}", normalTotalFont, normalTotalFont, false);
                    }

                    // **Total Final**
                    AddTotalRow(totalesTable, "TOTAL:", $"₡{totalFinal:N2}", finalTotalFont, finalTotalFont, true);

                    // Añadir la tabla de totales al documento
                    document.Add(totalesTable);

                    // ==================== PIE DE PÁGINA ====================
                    document.Add(new Paragraph("\n\n")); //saltos de linea

                    //texto agradecimiento
                    Paragraph footer = new Paragraph("Gracias por su compra.\nAppPrint & Wear © 2025",
                        FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY));
                    footer.Alignment = Element.ALIGN_CENTER;
                    document.Add(footer);

                    // Cerrar el documento para finalizar el PDF
                    document.Close();
                }

                return path;
            }
            catch (Exception ex)
            {
                // En caso de error, imprimir detalles en consola
                Console.WriteLine($"✗ Error al generar PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Error al generar PDF: {ex.Message}", ex);
            }
        }

        ///-----------------------------------------------------------------------------------------------------
        // ==================== MÉTODOS AUXILIARES ====================

        // Convierte un string en base64 (que representa una imagen) a un objeto imagen compatible con iTextSharp
        private iTextSharp.text.Image ConvertirBase64AImagen(string base64String)
        {
            try
            {
                // A veces el string base64 viene con un prefijo tipo "data:image/png;base64," 
                // Por eso se limpia para quedarse solo con la parte base64
                string base64Data = base64String;
                if (base64String.Contains(","))
                {
                    base64Data = base64String.Split(',')[1]; // Tomar solo lo que está después de la coma
                }

                // Convertir a bytes
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Crear imagen de iTextSharp
                return iTextSharp.text.Image.GetInstance(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error convirtiendo base64 a imagen: {ex.Message}");
                return null;
            }
        }

        ///-----------------------------------------------------------------------------------------------------
        // Añade una fila (dos celdas) con etiqueta y valor a una tabla de cliente, sin bordes y con padding
        private void AddClienteInfo(PdfPTable table, string label, string value, Font boldFont, Font normalFont)
        {
            // Celda con el texto de la etiqueta en negrita
            PdfPCell cellLabel = new PdfPCell(new Phrase(label, boldFont));
            cellLabel.Border = Rectangle.NO_BORDER; // Sin borde
            cellLabel.Padding = 5f;  // Espacio interno
            table.AddCell(cellLabel); // Añadir a la tabla

            // Celda con el texto del valor en fuente normal
            PdfPCell cellValue = new PdfPCell(new Phrase(value, normalFont));
            cellValue.Border = Rectangle.NO_BORDER;
            cellValue.Padding = 5f;
            table.AddCell(cellValue);
        }

        ///-----------------------------------------------------------------------------------------------------
        // Añade una celda de encabezado con fondo amarillo mostaza, texto centrado y borde visible
        private void AddHeaderCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = new BaseColor(243, 183, 0); // Color de fondo
            cell.HorizontalAlignment = Element.ALIGN_CENTER; // Texto centrado horizontalmente
            cell.VerticalAlignment = Element.ALIGN_MIDDLE; // Texto centrado verticalmente
            cell.Padding = 8f;  // Espacio interno
            cell.Border = Rectangle.BOX; // Borde visible
            table.AddCell(cell);
        }

        ///-----------------------------------------------------------------------------------------------------
        // Añade una celda con contenido de texto para filas normales (cuerpo de la tabla) con borde y centrado
        private void AddBodyCell(PdfPTable table, string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 8f;
            cell.Border = Rectangle.BOX;
            table.AddCell(cell);
        }

        ///-----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Añade una fila a la tabla de totales con etiqueta y valor
        /// </summary>
        private void AddTotalRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont, bool isTotal)
        {
            // Celda para la etiqueta
            PdfPCell cellLabel = new PdfPCell(new Phrase(label, labelFont));
            cellLabel.Border = isTotal ? Rectangle.TOP_BORDER : Rectangle.NO_BORDER;
            cellLabel.BorderWidthTop = isTotal ? 2f : 0f;
            cellLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellLabel.Padding = 8f;
            cellLabel.PaddingRight = 15f;
            table.AddCell(cellLabel);

            // Celda para el valor
            PdfPCell cellValue = new PdfPCell(new Phrase(value, valueFont));
            cellValue.Border = isTotal ? Rectangle.TOP_BORDER : Rectangle.NO_BORDER;
            cellValue.BorderWidthTop = isTotal ? 2f : 0f;
            cellValue.HorizontalAlignment = Element.ALIGN_RIGHT;
            cellValue.Padding = 8f;
            table.AddCell(cellValue);
        }

        ///-----------------------------------------------------------------------------------------------------
        private async Task EnviarFacturaPorCorreoAsync(string correoDestino, string pdfPath, string folio)
        {
            var from = "ecommercesystem123@gmail.com"; //coreo de donde se enviara
            var pass = "qbqd amqd liwq phbh"; // Contraseña de aplicación

            try
            {
                // Crear cliente SMTP configurado para Gmail y puerto 587 (TLS)
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    // Credenciales para autenticarse en el servidor SMTP
                    client.Credentials = new NetworkCredential(from, pass);
                    client.EnableSsl = true;  // Usar cifrado SSL/TLS para seguridad
                    client.Timeout = 30000; // Timeout de 30 segundos para la operación

                    // Crear el mensaje de correo
                    var mail = new MailMessage(from, correoDestino)
                    {
                        Subject = $"Factura {folio} - AppPrint & Wear",
                        IsBodyHtml = true
                    };

                    // Ruta física del logo en el servidor
                    string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Imagenes", "Logo.png");

                    // Crear vista HTML del mensaje
                    string htmlBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f5f5f5;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>

        <div style='text-align: center;'>
            <img src='cid:LogoApp' alt='Logo' style='width: 150px; margin-bottom: 20px;' />
        </div>

        <h2 style='color: #f3b700; text-align: center;'>¡Gracias por tu compra!</h2>
        <p>Estimado cliente,</p>
        <p>Adjuntamos tu <strong>factura electrónica</strong> con folio <strong style='color: #f3b700;'>{folio}</strong>.</p>
       
        <hr style='border: 1px solid #ddd; margin: 20px 0;'>
        <p style='color: #999; font-size: 12px; text-align: center;'>
            <strong style='color: #f3b700;'>AppPrint & Wear</strong> © 2025
        </p>
    </div>
</body>
</html>
";

                    // Crear la vista alternativa HTML
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");

                    // Crear el recurso vinculado (el logo)
                    LinkedResource logo = new LinkedResource(logoPath, "image/png");
                    logo.ContentId = "LogoApp"; // El mismo que pusimos en src='cid:LogoApp'
                    htmlView.LinkedResources.Add(logo);

                    // Agregar la vista al correo
                    mail.AlternateViews.Add(htmlView);


                    // Verificar que el archivo PDF exista antes de adjuntarlo
                    if (File.Exists(pdfPath))
                    {
                        mail.Attachments.Add(new Attachment(pdfPath));// Adjuntar el archivo PDF de la factura al correo
                        await client.SendMailAsync(mail);// Enviar el correo de forma asíncrona
                        Console.WriteLine($"✓ Factura {folio} enviada exitosamente a {correoDestino}");
                    }
                    else
                    {
                        throw new Exception($"El archivo PDF no existe: {pdfPath}");  // Si no existe el archivo, lanzar una excepción
                    }
                }
            }
            catch (Exception ex)
            {
                // Capturar y mostrar cualquier error que ocurra durante el envío del correo
                Console.WriteLine($"✗ Error al enviar factura: {ex.Message}");
                throw;
            }
        }
    }
}