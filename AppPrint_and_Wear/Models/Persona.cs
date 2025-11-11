using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace AppPrint_and_Wear.Models
{    //clase base 
    public abstract class Persona
    {
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Contraseña { get; set;}
        public int Telefono { get; set; }
        public string Correo { get; set; }
        public string Direccion {  get; set; }
        //Anotacion para que solo muestre la fecha sin la hora
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }
    }
}
