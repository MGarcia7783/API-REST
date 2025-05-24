using App.Modelos;
using AutoMapper;
using App.Dtos;

namespace App.PeliculasMappers
{
    /// <summary>
    /// Perfil de AutoMapper para mapear entre entidades de dominio y sus respectivos DTOs.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapea entre Categoria y CategoriaDto
            CreateMap<Categoria, CategoriaDto>().ReverseMap();

            // Mapea entre Categoria y CrearCategoria
            CreateMap<Categoria, CrearCategoriaDto>().ReverseMap();

            //Mapea entre Pelicula y PeliculaDto
            CreateMap<Pelicula, PeliculaDto>().ReverseMap();

            // Mapea entre Pelicula y CrearPelicula
            CreateMap<Pelicula, CrearPeliculaDto>().ReverseMap();

            CreateMap<Pelicula, ActualizarPeliculaDto>().ReverseMap();

            CreateMap<Usuario, UsuarioDatosDto>().ReverseMap();

            CreateMap<Usuario, UsuarioDto>().ReverseMap();
        }
    }
}
