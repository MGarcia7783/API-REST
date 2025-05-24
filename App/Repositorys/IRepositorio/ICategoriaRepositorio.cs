using App.Modelos;

namespace App.Repositorio.IRepositorio
{
    /// <summary>
    /// Interfaz que define las operaciones para interactuar con las categorías en la base de datos.
    /// Incluye operaciones para obtener, crear, actualizar, borrar y verificar la existencia de categorías.
    /// Todos los métodos de esta interfaz son asíncronos para mejorar el rendimiento y la escalabilidad.
    /// </summary>
    public interface ICategoriaRepositorio
    {
        Task<ICollection<Categoria>> GetCategoriasAsync();

        Task<Categoria?> GetCategoriaAsync(int categoriaId);

        Task<bool> ExisteCategoriaAsync(int id);

        Task<bool> ExisteCategoriaAsync(string nombre);

        Task<bool> CrearCategoriaAsync(Categoria categoria);

        Task<bool> ActualizarCategoriaAsync(Categoria categoria);

        Task<bool> BorrarCategoriaAsync(Categoria categoria);

        Task<bool> GuardarAsync();
    }
}
