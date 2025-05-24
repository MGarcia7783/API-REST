using App.Modelos;
using App.Dtos;
using App.Response;

namespace App.Repositorio.IRepositorio
{
    /// <summary>
    /// Interfaz que define las operaciones para interactuar con los usuarios en la base de datos.
    /// Incluye métodos para obtener usuarios, verificar unicidad, iniciar sesión y registrar nuevos usuarios.
    /// Todos los métodos son asíncronos para mejorar el rendimiento y la escalabilidad.
    /// </summary>
    public interface IUsuarioRepositorio
    {
        Task<ICollection<Usuario>> GetUsuariosAsync();

        Task<Usuario?> GetUsuarioAsync(string usuarioId);

        Task<bool> IsUniqueUserAsync(string usuario);
        Task<UsuarioLoginRespuestaDto> LoginAsync(UsuarioLoginDto usuarioLoginDto);
        Task<UsuarioRegistradoResultado> Registro(UsuarioRegistroDto usuarioRegistroDto);
    }
}
