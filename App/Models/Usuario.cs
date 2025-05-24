using Microsoft.AspNetCore.Identity;

namespace App.Modelos
{
    public class Usuario : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
    }
}
