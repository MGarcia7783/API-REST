using System.ComponentModel.DataAnnotations;

namespace App.Dtos
{
    /// <summary>
    /// DTO para la creación de una nueva categoría.
    /// </summary>
    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El número máximo de caracteres es de 100.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
