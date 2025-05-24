using App.Modelos.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace App.Modelos
{
    public class Pelicula
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Duracion { get; set; }
        public string? RutaImagen { get; set; } 
        public string? RutaLocalImagen { get; set; } 
        public TipoClasificacion Clasificacion { get; set; }
        public DateTime FechaCreacion { get; set; }

        //Relación con categoría
        public int categoriaId { get; set; }

        [ForeignKey("categoriaId")]
        public Categoria? Categoria { get; set; }
    }
}
