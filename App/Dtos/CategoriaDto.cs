using System.ComponentModel.DataAnnotations;

namespace App.Dtos
{
    /// <summary>
    /// DTO que representa los datos de una categoría.
    /// </summary>
    public class CategoriaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El número máximo de caracteres es de 100.")]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
    }
}
