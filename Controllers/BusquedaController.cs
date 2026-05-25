using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Helpers;

namespace ApiNexusERP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BusquedaController : ControllerBase
    {
        private RepositoryBusqueda repo;

        public BusquedaController(RepositoryBusqueda repo)
        {
            this.repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<SearchResultDTO>>> Get([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<SearchResultDTO>());
            }

            var resultados = await this.repo.BuscarGlobalAsync(q);

            return Ok(resultados);
        }
    }
}
