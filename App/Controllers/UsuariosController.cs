using App.Dtos;
using App.Repositorio.IRepositorio;
using App.Response;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace App.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar las operaciones relacionadas con los usuarios en la base de datos.
    /// Permite obtener, crear, actualizar, eliminar y verificar la existencia de usuarios.
    /// </summary>

    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/usuarios")]
    [ApiController]
    [ApiVersionNeutral]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepositorio _usRepo;
        protected RespuestaApi _respuestaApi;
        private readonly IMapper _mapper;

        public UsuariosController(IUsuarioRepositorio usRepo, IMapper mapper)
        {
            _usRepo = usRepo;
            _mapper = mapper;
            _respuestaApi = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var listaUsuarios = await _usRepo.GetUsuariosAsync();
                var listaUsuariosDto = _mapper.Map<List<UsuarioDto>>(listaUsuarios);
                return Ok(listaUsuariosDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el usuario: {ex.Message}");
            }
        }

        [HttpGet("{usuarioId}", Name = "GetUsuario")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUsuario(string usuarioId)
        {
            try
            {
                var itemUsuario = await _usRepo.GetUsuarioAsync(usuarioId);
                if (itemUsuario == null)
                {
                    return NotFound($"Usuario con ID {usuarioId} no encontrado.");
                }

                var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
                return Ok(itemUsuarioDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el usuario: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {
            try
            {
                bool usuarioUnico = await _usRepo.IsUniqueUserAsync(usuarioRegistroDto.NombreUsuario);
                if (!usuarioUnico)
                {
                    _respuestaApi.StatusCode = HttpStatusCode.BadRequest;
                    _respuestaApi.IsSuccess = false;
                    _respuestaApi.ErrorMessages.Add("El nombre de usuario ya existe.");
                    return BadRequest(_respuestaApi);
                }

                //Recibimos el resultado (usuario +  errores)
                var resultado = await _usRepo.Registro(usuarioRegistroDto);
                if (resultado.TieneErrores)
                {
                    _respuestaApi.StatusCode = HttpStatusCode.BadRequest;
                    _respuestaApi.IsSuccess = false;
                    _respuestaApi.ErrorMessages.AddRange(resultado.Errores);
                    return BadRequest(_respuestaApi);
                }

                _respuestaApi.StatusCode = HttpStatusCode.OK;
                _respuestaApi.IsSuccess = true;
                _respuestaApi.Result = resultado.Usuario;
                return Ok(_respuestaApi);
            }
            catch (Exception ex)
            {
                _respuestaApi.StatusCode = HttpStatusCode.InternalServerError;
                _respuestaApi.IsSuccess = false;
                _respuestaApi.ErrorMessages.Add($"Error interno del servidor: {ex.Message}");
                return StatusCode(500, _respuestaApi);
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
            try
            {
                var respuestaLogin = await _usRepo.LoginAsync(usuarioLoginDto);

                if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))
                {
                    _respuestaApi.StatusCode = HttpStatusCode.BadRequest;
                    _respuestaApi.IsSuccess = false;
                    _respuestaApi.ErrorMessages.Add("El nombre de usuario o password son incorrectos.");
                    return BadRequest(_respuestaApi);
                }

                _respuestaApi.StatusCode = HttpStatusCode.OK;
                _respuestaApi.IsSuccess = true;
                _respuestaApi.Result = respuestaLogin;
                return Ok(_respuestaApi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener el usuario: {ex.Message}");
            }
        }
    }
}
