using System.ComponentModel.DataAnnotations;

namespace App.Modelos
{
    /// <summary>
    /// Representa una categoría dentro de la aplicación de películas.
    /// </summary>
    public class Categoria
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
