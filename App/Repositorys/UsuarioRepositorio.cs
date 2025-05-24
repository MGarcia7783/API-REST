using App.Data;
using App.Modelos;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using App.Repositorio.IRepositorio;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using App.Dtos;
using App.Response;
using System.ComponentModel.DataAnnotations;

namespace App.Repositorio
{
    /// <summary>
    /// Implementación de la interfaz IUsuarioRepositorio para interactuar con los usuarios en la base de datos.
    /// Todos los métodos son asíncronos para optimizar el rendimiento en un entorno de múltiples solicitudes.
    /// </summary>
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly ApplicationDbContext _db;
        private string claveSecreta;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UsuarioRepositorio(ApplicationDbContext db, IConfiguration config, UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _db = db;
            claveSecreta = config.GetValue<string>("ApiSettings:Secreta")
                ?? throw new InvalidOperationException("La clave secreta no está configurada.");
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<Usuario?> GetUsuarioAsync(string usuarioId)
        {
            return await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        }

        public async Task<ICollection<Usuario>> GetUsuariosAsync()
        {
            return await _db.Usuarios.OrderBy(u => u.UserName).ToListAsync();
        }

        public async Task<bool> IsUniqueUserAsync(string usuario)
        {
            var usuarioBd = await _db.Usuarios.FirstOrDefaultAsync(u => u.UserName == usuario);
            if (usuarioBd == null)
            {
                return true;
            }
            return false;
        }

        public async Task<UsuarioLoginRespuestaDto> LoginAsync(UsuarioLoginDto usuarioLoginDto)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(
                u => u.UserName != null && u.UserName.ToLower() == usuarioLoginDto.NombreUsuario.ToLower());

            //Validamos que usuario y password no sean nulos
            if (usuario == null || !await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password))
            {
                return new UsuarioLoginRespuestaDto()
                {
                    Token = "",
                    Usuario = null
                };
            }

            //Aquí existe el usuario, entonces podemos procesar el login
            var roles = await _userManager.GetRolesAsync(usuario);
            var manejadorToken = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(claveSecreta);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? string.Empty),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = manejadorToken.CreateToken(tokenDescriptor);

            UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
            {
                Token = manejadorToken.WriteToken(token),
                Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
            };

            return usuarioLoginRespuestaDto;
        }

        public async Task<UsuarioRegistradoResultado> Registro(UsuarioRegistroDto usuarioRegistroDto)
        {

            var resultado = new UsuarioRegistradoResultado();

            //Validar email válido
            var emailValido = new EmailAddressAttribute();
            if(!emailValido.IsValid(usuarioRegistroDto.NombreUsuario))
            {
                resultado.Errores.Add("El nombre de usuario debe ser un correo electrónico válido.");
                return resultado;
            }

            //Valdar contraseña válida
            foreach(var validar in _userManager.PasswordValidators)
            {
                var validarPassword = await validar.ValidateAsync(_userManager, null!, usuarioRegistroDto.Password);
                if(!validarPassword.Succeeded)
                {
                    resultado.Errores.AddRange(validarPassword.Errors.Select(e => e.Description));
                    return resultado;
                }
            }

            //Crear usuario
            Usuario usuario = new Usuario()
            {
                UserName = usuarioRegistroDto.NombreUsuario,
                Email = usuarioRegistroDto.NombreUsuario,
                Nombre = usuarioRegistroDto.Nombre,
            };

            var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);
            if (!result.Succeeded)
            {
                resultado.Errores.AddRange(result.Errors.Select(e => e.Description));
                return resultado;
            }

            //Crear roles si no existen
            if (!await _roleManager.RoleExistsAsync(usuarioRegistroDto.Rol))
            {
                var crearRol = await _roleManager.CreateAsync(new IdentityRole(usuarioRegistroDto.Rol));
                if(!crearRol.Succeeded)
                {
                    resultado.Errores.AddRange(crearRol.Errors.Select(e => e.Description));
                    return resultado;
                }
            }

            //Asignar rol
            await _userManager.AddToRoleAsync(usuario, usuarioRegistroDto.Rol);

            //Mapear DTO
            resultado.Usuario = _mapper.Map<UsuarioDatosDto>(usuario);
            return resultado;
        }
    }
}
