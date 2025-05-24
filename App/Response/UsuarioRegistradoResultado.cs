using App.Dtos;

namespace App.Response
{
    public class UsuarioRegistradoResultado
    {
        public UsuarioDatosDto? Usuario { get; set; }
        public List<string> Errores { get; set; } = new();

        //Any: Determina si una colección contiene al mens un elemento
        //Si la lista está vacía, Any() devuelve false
        public bool TieneErrores => Errores.Any();
    }
}
