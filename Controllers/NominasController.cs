using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Helpers;
using NugetModelsNexusERP.Models;

namespace ApiNexusERP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NominasController : ControllerBase
    {
        private RepositoryNominas repo;
        private IMapper mapper;
        private HelperSessionContextAccessor contextAccessor;

        public NominasController(RepositoryNominas repo, IMapper mapper, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NominaDTO>> FindNomina(int id)
        {
            Nomina nomina = await this.repo.FindNominaAsync(id);
            if (nomina == null)
            {
                return NotFound(new { mensaje = "Nómina no encontrada." });
            }

            NominaDTO dto = this.mapper.Map<NominaDTO>(nomina);
            return Ok(dto);
        }

        [HttpGet("[action]/{idEmpleado}/{mes}/{anio}")]
        public async Task<ActionResult<NominaDTO>> FindNominaMes(int idEmpleado, int mes, int anio)
        {
            Nomina nomina = await this.repo.FindNominaEmpleadoByMesAsync(idEmpleado, mes, anio);
            if (nomina == null)
            {
                return NotFound(new { mensaje = "Nómina no encontrada para este periodo." });
            }

            NominaDTO dto = this.mapper.Map<NominaDTO>(nomina);
            return Ok(dto);
        }

        [HttpGet("[action]/{mes}/{anio}")]
        public async Task<ActionResult<List<EmpleadoDTO>>> Estado(int mes, int anio)
        {
            List<Empleado> empleados = await this.repo.GetEmpleadosConNominasAsync(mes, anio);
            List<EmpleadoDTO> lstDto = this.mapper.Map<List<EmpleadoDTO>>(empleados);
            return Ok(lstDto);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<NominaDTO>> Generar(NominaDTO nominaDTO)
        {
            Nomina nomina = this.mapper.Map<Nomina>(nominaDTO);

            nomina.EmpresaId = this.contextAccessor.GetEmpresaIdSession();

            Nomina nominaGuardada = await this.repo.GuardarNominaCompletaAsync(nomina);

            if (nominaGuardada == null)
            {
                return BadRequest(new { mensaje = "Error al intentar guardar la nómina y generar los asientos contables." });
            }

            NominaDTO dtoResult = this.mapper.Map<NominaDTO>(nominaGuardada);
            return Ok(dtoResult);
        }

        [HttpPut("[action]/{idNomina}")]
        public async Task<ActionResult> Pagar(int idNomina)
        {
            bool pagado = await this.repo.PagarNominaAsync(idNomina);

            if (!pagado)
            {
                return BadRequest(new { mensaje = "No se ha podido registrar el pago de la nómina. Verifica que exista, no esté ya pagada y existan las cuentas contables." });
            }

            return Ok(new { mensaje = "La nómina ha sido pagada y contabilizada correctamente." });
        }
    }
}
