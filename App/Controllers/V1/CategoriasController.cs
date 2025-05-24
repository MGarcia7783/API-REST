using App.Modelos;
using App.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using App.Dtos;

namespace App.Controllers.V1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/categorias")]
    [ApiVersion("1.0")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;
        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene todas las categorías de la base de datos.
        /// </summary>
        /// <returns>Lista de categorías con sus datos mapeados a un DTO.</returns>

        [AllowAnonymous]
        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var listaCategorias = await _ctRepo.GetCategoriasAsync();
                var listaCategoriasDto = _mapper.Map<List<CategoriaDto>>(listaCategorias);
                return Ok(listaCategoriasDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las categorías: {ex.Message}");
            }
        }

        //Metodo de prueba para múltiples versiones
        [HttpGet("GetString")]
        [Obsolete]
        public IEnumerable<string> Get()
        {
            return new string[] { "valor1", "valor2", "valor3" };
        }

        /// <summary>
        /// Obtiene una categoría específica por su ID.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a obtener.</param>
        /// <returns>La categoría solicitada, o un estado 404 si no existe.</returns>

        [AllowAnonymous]
        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoria(int categoriaId)
        {
            try
            {
                var itemCategoria = await _ctRepo.GetCategoriaAsync(categoriaId);
                if (itemCategoria == null)
                {
                    return NotFound($"Categoría con ID {categoriaId} no encontrada.");
                }

                var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);
                return Ok(itemCategoriaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la categoría: {ex.Message}");
            }
        }

        /// <summary>
        /// Crea una nueva categoría en la base de datos.
        /// </summary>
        /// <param name="crearCategoriaDto">Los datos necesarios para crear la nueva categoría.</param>
        /// <returns>El resultado de la creación de la categoría, incluyendo el estado HTTP.</returns>

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearCategoriaDto == null)
            {
                return BadRequest("La categoría no puede ser nula.");
            }

            try
            {
                if (await _ctRepo.ExisteCategoriaAsync(crearCategoriaDto.Nombre))
                {
                    ModelState.AddModelError("", "La categoría ya existe.");
                    return BadRequest(ModelState);
                }

                var categoria = _mapper.Map<Categoria>(crearCategoriaDto);
                var result = await _ctRepo.CrearCategoriaAsync(categoria);

                if (!result)
                {
                    return StatusCode(500, "Ocurrió un error al guardar el registro.");
                }

                return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la categoría: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza parcialmente una categoría existente en la base de datos.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a actualizar.</param>
        /// <param name="categoriaDto">Los datos actualizados de la categoría.</param>
        /// <returns>Un estado HTTP que refleja el resultado de la operación.</returns>

        [Authorize(Roles = "Admin")]
        [HttpPatch("{categoriaId:int}", Name = "ActualizarPatchCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPatchCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || categoriaId != categoriaDto.Id)
            {
                return BadRequest("El ID de la categoría no coincide.");
            }

            try
            {
                var categoriaExistente = await _ctRepo.GetCategoriaAsync(categoriaId);
                if (categoriaExistente == null)
                {
                    return NotFound($"Categoría con ID {categoriaId} no encontrada.");
                }

                var categoria = _mapper.Map<Categoria>(categoriaDto);
                var result = await _ctRepo.ActualizarCategoriaAsync(categoria);

                if (!result)
                {
                    return StatusCode(500, "Ocurrió un error al actualizar el registro.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la categoría: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza una categoría existente completamente en la base de datos.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a actualizar.</param>
        /// <param name="categoriaDto">Los datos actualizados de la categoría.</param>
        /// <returns>Un estado HTTP que refleja el resultado de la operación.</returns>

        [Authorize(Roles = "Admin")]
        [HttpPut("{categoriaId:int}", Name = "ActualizarPutCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPutCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || categoriaId != categoriaDto.Id)
            {
                return BadRequest("El ID de la categoría no coincide.");
            }

            try
            {
                var categoriaExistente = await _ctRepo.GetCategoriaAsync(categoriaId);
                if (categoriaExistente == null)
                {
                    return NotFound($"Categoría con ID {categoriaId} no encontrada.");
                }

                var categoria = _mapper.Map<Categoria>(categoriaDto);
                var result = await _ctRepo.ActualizarCategoriaAsync(categoria);

                if (!result)
                {
                    return StatusCode(500, "Ocurrió un error al actualizar el registro.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la categoría: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una categoría de la base de datos.
        /// </summary>
        /// <param name="categoriaId">El ID de la categoría a eliminar.</param>
        /// <returns>Un estado HTTP que refleja el resultado de la operación.</returns>

        [Authorize(Roles = "Admin")]
        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BorrarCategoria(int categoriaId)
        {
            try
            {
                var categoriaExistente = await _ctRepo.GetCategoriaAsync(categoriaId);
                if (categoriaExistente == null)
                {
                    return NotFound($"Categoría con ID {categoriaId} no encontrada.");
                }

                var result = await _ctRepo.BorrarCategoriaAsync(categoriaExistente);
                if (!result)
                {
                    return StatusCode(500, "Ocurrió un error al eliminar el registro.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar la categoría: {ex.Message}");
            }
        }
    }
}
