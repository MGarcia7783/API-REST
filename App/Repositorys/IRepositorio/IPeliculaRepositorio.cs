using App.Modelos;

namespace App.Repositorio.IRepositorio
{
    /// <summary>
    /// Interfaz que define las operaciones para interactuar con las películas en la base de datos.
    /// Incluye operaciones para obtener, crear, actualizar, borrar y verificar la existencia de películas.
    /// Todos los métodos de esta interfaz son asíncronos para mejorar el rendimiento y la escalabilidad.
    /// </summary>
    public interface IPeliculaRepositorio
    {
        //V1
        //Task<ICollection<Pelicula>> GetPeliculasAsync();

        //V2
        Task<ICollection<Pelicula>> GetPeliculasAsync(int pageNumber, int pageSize);
        int GetTotalPeliculas();
        Task<ICollection<Pelicula>> GetPeliculasEnCategoriaAsync(int categoriaId);

        Task<IEnumerable<Pelicula>> BuscarPeliculaAsync(string nombre);

        Task<Pelicula?> GetPeliculaAsync(int peliculaId);

        Task<bool> ExistePeliculaAsync(int id);

        Task<bool> ExistePeliculaAsync(string nombre);

        Task<bool> CrearPeliculaAsync(Pelicula pelicula);

        Task<bool> ActualizarPeliculaAsync(Pelicula pelicula);

        Task<bool> BorrarPeliculaAsync(Pelicula pelicula);

        Task<bool> GuardarAsync();
    }
}
