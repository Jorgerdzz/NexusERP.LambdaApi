using ApiNexusERP.DTOs;
using ApiNexusERP.Helpers;
using ApiNexusERP.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NugetModelsNexusERP.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiNexusERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryAuth repo;
        private HelperActionOAuthService helper;

        public AuthController(RepositoryAuth repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(LoginDTO model)
        {
            Usuario usuario = await this.repo.LogInUserAsync(model.Email, model.Password);
            if (usuario == null)
            {
                return Unauthorized(new {mensaje = "Email o contraseña incorrectos."});
            }
            else
            {
                Claim[] claims = new []
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim("EmpresaId", usuario.EmpresaId.ToString()),
                    new Claim("EmpleadoId", usuario.EmpleadoId?.ToString() ?? "0"),
                    new Claim("NombreEmpresa", usuario.Empresa?.NombreComercial ?? "Sin Empresa")
                };

                SigningCredentials credentials =
                    new SigningCredentials(this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token =
                    new JwtSecurityToken(
                        issuer: this.helper.Issuer,
                        audience: this.helper.Audience,
                        claims: claims,
                        signingCredentials: credentials,
                        expires: DateTime.UtcNow.AddMinutes(20),
                        notBefore: DateTime.UtcNow
                        );

                return Ok(new
                {
                    response = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Register(RegistroDTO model)
        {
            try
            {
                Usuario usuarioCreado = await this.repo.RegisterUserAsync(model);
                return Ok(new { mensaje = "Registro completado con éxito. Ya puedes iniciar sesión en tu nuevo entorno de NexusERP." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno al intentar registrar la empresa. Por favor, inténtalo de nuevo." });
            }
        }

    }
}
