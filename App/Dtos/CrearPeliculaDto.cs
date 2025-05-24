using App.Modelos.Enum;

namespace App.Dtos
{
    public class CrearPeliculaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Duracion { get; set; }
        public string? RutaImagen { get; set; }
        public required IFormFile Imagen { get; set; }
        public TipoClasificacion Clasificacion { get; set; }
        public int categoriaId { get; set; }
    }
}
