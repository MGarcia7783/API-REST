using App.Modelos;
using App.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using App.Dtos;
using App.Response;

namespace App.Controllers.V1
{
    /// <summary>
    /// Controlador encargado de gestionar las operaciones relacionadas con las películas en la base de datos.
    /// Permite obtener, crear, actualizar, eliminar y verificar la existencia de películas.
    /// </summary>

    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/peliculas")]
    [ApiController]
    [ApiVersion("1.0")]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;
        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todas las películas de la base de datos.
        /// </summary>
        /// <returns>Lista de películas con sus datos mapeados a un DTO.</returns>

        //V1
        //[AllowAnonymous]
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<IActionResult> GetPeliculas()
        //{
        //    try
        //    {
        //        var listaPeliculas = await _pelRepo.GetPeliculasAsync();
        //        var listaPeliculasDto = _mapper.Map<List<PeliculaDto>>(listaPeliculas);
        //        return Ok(listaPeliculasDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener las películas: {ex.Message}");
        //    }
        //}


        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalPeliculas = _pelRepo.GetTotalPeliculas();
                

                if(totalPeliculas == 0)
                {
                    return NotFound("No se encontraron películas.");
                }

                var peliculas = await _pelRepo.GetPeliculasAsync(pageNumber, pageSize);

                var peliculasDto = peliculas
                    .Select(p => _mapper.Map<PeliculaDto>(p))
                    .ToList();

                var response = new PagedResponse<PeliculaDto>(peliculasDto, totalPeliculas, pageNumber, pageSize);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error recuperando datos de la aplicación: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene una película específica por su ID.
        /// </summary>
        /// <param name="peliculaId">El ID de la película a obtener.</param>
        /// <returns>La película solicitada, o un estado 404 si no existe.</returns>

        [AllowAnonymous]
        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPelicula(int peliculaId)
        {
            try
            {
                var itemPelicula = await _pelRepo.GetPeliculaAsync(peliculaId);
                if (itemPelicula == null)
                {
                    return NotFound($"Película con ID {peliculaId} no encontrada.");
                }

                var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);
                return Ok(itemPeliculaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al obtener la película: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea una nueva película en la base de datos.
        /// </summary>
        /// <param name="crearPeliculaDto">Los datos necesarios para crear la nueva película.</param>
        /// <returns>El resultado de la creación de la película, incluyendo el estado HTTP.</returns>

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearPeliculaDto == null)
            {
                return BadRequest("La película no puede ser nula.");
            }

            try
            {
                if (await _pelRepo.ExistePeliculaAsync(crearPeliculaDto.Nombre))
                {
                    ModelState.AddModelError("", "La película ya existe.");
                    return BadRequest(ModelState);
                }

                var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

                //Subir imagen
                if (crearPeliculaDto.Imagen != null)
                {
                    string nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(crearPeliculaDto.Imagen.FileName)}";
                    string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                    var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);
                    FileInfo file = new FileInfo(ubicacionDirectorio);

                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                    {
                       await crearPeliculaDto.Imagen.CopyToAsync(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                    pelicula.RutaLocalImagen = rutaArchivo;
                }
                else
                {
                    pelicula.RutaImagen = "https://placehold.co/600x400";
                }

                var result = await _pelRepo.CrearPeliculaAsync(pelicula);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error al guardar el registro.");
                }

                var peliculaDto = _mapper.Map<PeliculaDto>(pelicula);
                return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al crear la película: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza parcialmente una película existente en la base de datos.
        /// </summary>
        /// <param name="peliculaId">El ID de la película a actualizar.</param>
        /// <param name="peliculaDto">Los datos actualizados de la película.</param>
        /// <returns>Un estado HTTP que refleja el resultado de la operación.</returns>

        [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPatchPelicula(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.Id)
            {
                return BadRequest("El ID de la película no coincide.");
            }

            try
            {
                var peliculaExistente = await _pelRepo.GetPeliculaAsync(peliculaId);
                if (peliculaExistente == null)
                {
                    return NotFound($"Película con ID {peliculaId} no encontrada.");
                }

                // Mapear campos del DTO a la entidad existente
                _mapper.Map(actualizarPeliculaDto, peliculaExistente);

                //Subir imagen
                if (actualizarPeliculaDto.Imagen != null)
                {
                    string nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(actualizarPeliculaDto.Imagen.FileName)}";
                    string rutaArchivo = @"wwwroot\ImagenesPeliculas\" + nombreArchivo;

                    var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                    //Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(peliculaExistente.RutaLocalImagen))
                    {
                        var rutaAnterior = Path.Combine(Directory.GetCurrentDirectory(), peliculaExistente.RutaLocalImagen);
                        if (System.IO.File.Exists(rutaAnterior))
                        {
                            System.IO.File.Delete(rutaAnterior);
                        }
                    }

                    using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                    {
                        await actualizarPeliculaDto.Imagen.CopyToAsync(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    peliculaExistente.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                    peliculaExistente.RutaLocalImagen = rutaArchivo;
                }
                else
                {
                    peliculaExistente.RutaImagen = "https://placehold.co/600x400";
                }

                //Guardams cambios en la base de datos
                var reultado = await _pelRepo.ActualizarPeliculaAsync(peliculaExistente);
                if(!reultado)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "No se puedo actualizar la película.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al actualizar la película: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una película de la base de datos.
        /// </summary>
        /// <param name="peliculaId">El ID de la película a eliminar.</param>
        /// <returns>Un estado HTTP que refleja el resultado de la operación.</returns>

        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorrarPelicula(int peliculaId)
        {
            try
            {
                var peliculaExistente = await _pelRepo.GetPeliculaAsync(peliculaId);
                if (peliculaExistente == null)
                {
                    return NotFound($"Película con ID {peliculaId} no encontrada.");
                }

                var result = await _pelRepo.BorrarPeliculaAsync(peliculaExistente);
                if (!result)
                {
                    return StatusCode(500, "Ocurrió un error al eliminar el registro.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar la película: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la lista de películas asociadas a una categoría específica.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a consultar.</param>
        /// <returns>Un resultado HTTP que contiene la lista de películas si existen, o un código de error correspondiente.</returns>

        [AllowAnonymous]
        [HttpGet("GetpeliculasEnCategoria/{categoriaId:int}")]
        //[ResponseCache(CacheProfileName = "PerfilCache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetpeliculasEnCategoria(int categoriaId)
        {
            try
            {
                var listaPeliculas = await _pelRepo.GetPeliculasEnCategoriaAsync(categoriaId);

                if (listaPeliculas == null || !listaPeliculas.Any())
                {
                    return NotFound($"No se encontraron películas en la categoría con ID {categoriaId}");
                }

                var itemPelicula = _mapper.Map<List<PeliculaDto>>(listaPeliculas);
                return Ok(itemPelicula);
            }
            catch (Exception ex)
            {
                // Puedes agregar logging aquí si usas algún sistema como Serilog, NLog, etc.
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error recuperando datos en la aplicación: {ex.Message}");
            }
        }


        /// <summary>
        /// Obtiene la lista de películas asociadas a una categoría específica.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a consultar.</param>
        /// <returns>Un resultado HTTP que contiene la lista de películas si existen, o un código de error correspondiente.</returns>

        [AllowAnonymous]
        [HttpGet("Buscar")]
        //[ResponseCache(CacheProfileName = "PerfilCache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Buscar(string nombre)
        {
            try
            {
                var peliculas = await _pelRepo.BuscarPeliculaAsync(nombre);
                if (!peliculas.Any())
                {
                    return NotFound($"No se econtraron películas que coincidan con los criterios de búsqueda: {nombre}");
                }

               var peliculasDto = _mapper.Map<List<PeliculaDto>>(peliculas);
                return Ok(peliculasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error recuperando datos: {ex.Message}");
            }
        }
    }
}
