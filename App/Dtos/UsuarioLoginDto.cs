using System.ComponentModel.DataAnnotations;

namespace App.Dtos
{
    public class UsuarioLoginDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El password es obligatorio")]
        public string Password { get; set; } = string.Empty;
    }
}
