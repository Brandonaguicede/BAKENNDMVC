using AppPrint_and_Wear.Models;
using System.ComponentModel.DataAnnotations;

namespace TuProyecto.Models
{
    public class Producto
    {
       
        public int ProductoId { get; set; }

        [Required]
        public string Descripcion { get; set; }  

        public int Stock {  get; set; }

       
        [Required]
        public float Precio { get; set; }

        [Url]
        public string ImagenUrlFrende { get; set; }  // Link a la imagen del producto frente

       [Url]
        public string ImagenUrlEspalda { get; set; }  // Link a la imagen del producto espalda

        public List<CartItem> CartItems { get; set; }

        // Relación con Categoría
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
    }
}
