using System.ComponentModel.DataAnnotations;
using TuProyecto.Models;

namespace AppPrint_and_Wear.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        // public string ImagenUrl { get; set; } // opcional, para mostrar una imagen en la interfaz

        public List<Producto> Productos { get; set; } = new List<Producto>();
    }

}
