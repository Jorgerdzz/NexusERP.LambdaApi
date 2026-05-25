using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly RepositoryUsuario repo;
        private readonly IMapper mapper;

        public UsuariosController(RepositoryUsuario repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> FindUsuario(int id)
        {
            Usuario usuario = await this.repo.FindUsuarioAsync(id);
            if (usuario == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado." });
            }

            UsuarioDTO dto = this.mapper.Map<UsuarioDTO>(usuario);
            return Ok(dto);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<UsuarioDTO>> Perfil(int id)
        {
            Usuario usuario = await this.repo.GetPerfilUsuarioAsync(id);
            if (usuario == null)
            {
                return NotFound(new { mensaje = "Perfil de usuario no encontrado." });
            }

            UsuarioDTO dto = this.mapper.Map<UsuarioDTO>(usuario);
            return Ok(dto);
        }

        [HttpPut("[action]/{idUsuario}")]
        public async Task<ActionResult<UsuarioDTO>> UpdatePerfil(int idUsuario, [FromBody] UsuarioDTO updateData)
        {
            if (string.IsNullOrEmpty(updateData.Nombre) || string.IsNullOrEmpty(updateData.Email))
            {
                return BadRequest(new { mensaje = "El nombre y correo son obligatorios." });
            }

            Usuario usuarioActualizado = await this.repo.UpdatePerfilUsuarioAsync(idUsuario, updateData.Nombre, updateData.Email);

            if (usuarioActualizado == null)
            {
                return BadRequest(new { mensaje = "No se pudo actualizar el perfil. Verifica que el usuario exista y que el email no esté en uso por otro usuario." });
            }

            UsuarioDTO dto = this.mapper.Map<UsuarioDTO>(usuarioActualizado);
            return Ok(dto);
        }
    }
}
