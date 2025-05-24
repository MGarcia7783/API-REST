using App.Data;
using App.Modelos;
using App.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace App.Repositorio
{
    /// <summary>
    /// Implementación de la interfaz IPeliculaRepositorio para interactuar con las categorías en la base de datos.
    /// Todos los métodos son asíncronos para optimizar el rendimiento en un entorno de múltiples solicitudes.
    /// </summary>
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly ApplicationDbContext _db;

        public PeliculaRepositorio(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ActualizarPeliculaAsync(Pelicula pelicula)
        {
            var peliculaExistente = await _db.Peliculas.FindAsync(pelicula.Id);
            if (peliculaExistente != null)
            {
                _db.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _db.Peliculas.Update(pelicula);
            }
            return await GuardarAsync();
        }

        public async Task<bool> BorrarPeliculaAsync(Pelicula pelicula)
        {
            _db.Peliculas.Remove(pelicula);
            return await GuardarAsync();
        }

        public async Task<IEnumerable<Pelicula>> BuscarPeliculaAsync(string nombre)
        {
            IQueryable<Pelicula> query = _db.Peliculas
                .Include(p => p.Categoria);

            if (!string.IsNullOrEmpty(nombre))
            {
                nombre = nombre.ToLower();

                query = query.Where(p =>
                    EF.Functions.ILike(p.Nombre, $"%{nombre}%") ||
                    EF.Functions.ILike(p.Descripcion, $"%{nombre}%"));

            }
            return await query.ToListAsync();
        }

        public async Task<bool> CrearPeliculaAsync(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.UtcNow;
            await _db.Peliculas.AddAsync(pelicula);
            return await GuardarAsync();
        }

        public async Task<bool> ExistePeliculaAsync(int id)
        {
            return await _db.Peliculas.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> ExistePeliculaAsync(string nombre)
        {
            bool valor = await _db.Peliculas.AnyAsync(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public async Task<Pelicula?> GetPeliculaAsync(int peliculaId)
        {
            return await _db.Peliculas.FirstOrDefaultAsync(c => c.Id == peliculaId);
        }

        //V1
        //public async Task<ICollection<Pelicula>> GetPeliculasAsync()
        //{
        //    return await _db.Peliculas.OrderBy(c => c.Nombre).ToListAsync();
        //}

        //V2
        public async Task<ICollection<Pelicula>> GetPeliculasAsync(int pageNumber, int pageSize)
        {
            return await _db.Peliculas.OrderBy(c => c.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public int GetTotalPeliculas()
        {
            return _db.Peliculas.Count();
        }

        public async Task<ICollection<Pelicula>> GetPeliculasEnCategoriaAsync(int categoriaId)
        {
            return await _db.Peliculas.Include(ca => ca.Categoria).Where(ca => ca.categoriaId == categoriaId).ToListAsync();
        }

        public async Task<bool> GuardarAsync()
        {
            return await _db.SaveChangesAsync() >= 0;
        }
    }
}
