using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Helpers;
using NugetModelsNexusERP.Models;

namespace ApiNexusERP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContabilidadController : ControllerBase
    {
        private RepositoryContabilidad repo;
        private IMapper mapper;
        private HelperSessionContextAccessor contextAccessor;

        public ContabilidadController(RepositoryContabilidad repo, IMapper mapper, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<CuentaContableDTO>>> PlanContable()
        {
            var cuentas = await this.repo.GetPlanContableAsync();
            return Ok(this.mapper.Map<List<CuentaContableDTO>>(cuentas));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<CuentaContableDTO>> CrearCuenta(CuentaContableDTO dto)
        {
            var cuenta = this.mapper.Map<CuentasContable>(dto);
            cuenta.EmpresaId = this.contextAccessor.GetEmpresaIdSession();

            var nuevaCuenta = await this.repo.CrearCuentaContableAsync(cuenta);
            return Ok(this.mapper.Map<CuentaContableDTO>(nuevaCuenta));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<AsientoContableDTO>>> LibroDiario()
        {
            var asientos = await this.repo.GetLibroDiarioAsync();
            return Ok(this.mapper.Map<List<AsientoContableDTO>>(asientos));
        }

        [HttpGet("[action]/{idCuenta}")]
        public async Task<ActionResult> LibroMayor(int idCuenta, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            var saldoAnterior = await this.repo.GetSaldoAnteriorAsync(idCuenta, desde);
            var apuntes = await this.repo.GetExtractoCuentaAsync(idCuenta, desde, hasta);

            return Ok(new
            {
                SaldoAnterior = saldoAnterior,
                Movimientos = this.mapper.Map<List<ApunteContableDTO>>(apuntes)
            });
        }

    }
}
