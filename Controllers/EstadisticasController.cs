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
    public class EstadisticasController : ControllerBase
    {
        private RepositoryEstadisticas repo;
        private HelperSessionContextAccessor contextAccessor;

        public EstadisticasController(RepositoryEstadisticas repo, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet("[action]/{anio}")]
        public async Task<ActionResult<List<ReporteMensualDTO>>> Ingresos(int anio)
        {
            var data = await this.repo.GetIngresosPorMesAsync(anio);
            return Ok(data);
        }

        [HttpGet("[action]/{anio}")]
        public async Task<ActionResult<List<ReporteMensualDTO>>> Gastos(int anio)
        {
            var data = await this.repo.GetGastosPorMesAsync(anio);
            return Ok(data);
        }

        [HttpGet("[action]/{anio}")]
        public async Task<ActionResult<List<ReporteDepartamentoDTO>>> CostesDepartamento(int anio)
        {
            var data = await this.repo.GetCostesPorDepartamentoAsync(anio);
            return Ok(data);
        }

        // Añade este endpoint dentro de tu ReportsController
        [HttpGet("[action]/{anio}")]
        public async Task<ActionResult<MetricasDashboardDTO>> MetricasGlobales(int anio)
        {
            var metricas = await this.repo.GetEstadisticasAsync(anio);
            return Ok(metricas);
        }
    }
}
