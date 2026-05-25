using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Helpers;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentosController : ControllerBase
    {
        private RepositoryDepartamentos repo;
        private HelperSessionContextAccessor contextAccessor;
        private IMapper mapper;

        public DepartamentosController(RepositoryDepartamentos repo, HelperSessionContextAccessor contextAccessor, IMapper mapper)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<DepartamentoDTO>>> GetDepartamentos()
        {
            List<Departamento> departamentos = await this.repo.GetDepartamentosAsync();
            List<DepartamentoDTO> listDTO = this.mapper.Map<List<DepartamentoDTO>>(departamentos);
            return Ok(listDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartamentoDTO>> FindDepartamento(int id)
        {
            Departamento departamento = await this.repo.FindDepartamentoAsync(id);

            if (departamento == null)
            {
                return NotFound(new { mensaje = "El departamento no existe." });
            }

            DepartamentoDTO dto = this.mapper.Map<DepartamentoDTO>(departamento);

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Post(DepartamentoDTO dto)
        {
            Departamento departamento = this.mapper.Map<Departamento>(dto);
            departamento.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
            Departamento nuevoDep = await this.repo.CreateDepartamentoAsync(departamento);
            dto.Id = nuevoDep.Id;
            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult> Put(DepartamentoDTO dto)
        {
            Departamento departamento = this.mapper.Map<Departamento>(dto);
            Departamento departamentoActualizado = await this.repo.UpdateDepartamentoAsync(departamento);

            if (departamentoActualizado == null)
            {
                return NotFound(new { mensaje = "No se ha encontrado el departamento para modificar." });
            }

            return Ok(new { mensaje = "Modificado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            bool eliminado = await this.repo.DeleteDepartamentoAsync(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "No se ha encontrado el departamento para eliminar." });
            }

            return Ok(new { mensaje = "Eliminado correctamente." });
        }

        [HttpGet("numerototal")]
        public async Task<ActionResult<int>> GetNumeroTotalDepartamentos()
        {
            int total = await this.repo.GetTotalDepartamentosAsync();
            return Ok(total);
        }

        [HttpGet("presupuestototal")]
        public async Task<ActionResult<decimal>> GetPresupuestoTotal()
        {
            decimal total = await this.repo.GetPresupuestoTotalAnualAsync();
            return Ok(total);
        }

        [HttpGet("estadisticas")]
        public async Task<ActionResult> GetEstadisticas()
        {
            var estadisticas = await this.repo.GetEstadisticasDepartamentosAsync();

            var resultadoJson = estadisticas.Select(e => new
            {
                Id = e.Id,
                Nombre = e.Nombre,
                PresupuestoAnual = e.PresupuestoAnual,
                NumeroEmpleados = e.NumeroEmpleados,
                SalarioPromedio = e.SalarioPromedio
            });

            return Ok(resultadoJson);
        }
    }
}
