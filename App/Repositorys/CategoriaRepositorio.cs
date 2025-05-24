using App.Data;
using App.Modelos;
using App.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace App.Repositorio
{
    /// <summary>
    /// Implementación de la interfaz ICategoriaRepositorio para interactuar con las categorías en la base de datos.
    /// Todos los métodos son asíncronos para optimizar el rendimiento en un entorno de múltiples solicitudes.
    /// </summary>
    public class CategoriaRepositorio : ICategoriaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public CategoriaRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ActualizarCategoriaAsync(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.UtcNow;

            var categoriaExistente = await _db.Categorias.FindAsync(categoria.Id);
            if (categoriaExistente != null)
            {
                _db.Entry(categoriaExistente).CurrentValues.SetValues(categoria);
            }
            else
            {
                _db.Categorias.Update(categoria);
            }
            return await GuardarAsync();
        }

        public async Task<bool> BorrarCategoriaAsync(Categoria categoria)
        {
            _db.Categorias.Remove(categoria);
            return await GuardarAsync();
        }

        public async Task<bool> CrearCategoriaAsync(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.UtcNow;
            await _db.Categorias.AddAsync(categoria);
            return await GuardarAsync();
        }

        public async Task<bool> ExisteCategoriaAsync(int id)
        {
            return await _db.Categorias.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExisteCategoriaAsync(string nombre)
        {
            bool valor = await _db.Categorias.AnyAsync(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public async Task<Categoria?> GetCategoriaAsync(int categoriaId)
        {
            return await _db.Categorias
                .FirstOrDefaultAsync(c => c.Id == categoriaId);
        }

        public async Task<ICollection<Categoria>> GetCategoriasAsync()
        {
            return await _db.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<bool> GuardarAsync()
        {
            return await _db.SaveChangesAsync() >= 0;
        }
    }
}
