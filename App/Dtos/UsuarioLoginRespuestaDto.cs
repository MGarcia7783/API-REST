namespace App.Dtos
{
    public class UsuarioLoginRespuestaDto
    {
        public UsuarioDatosDto? Usuario { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
