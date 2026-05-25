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
    public class EmpleadosController : ControllerBase
    {
        private RepositoryEmpleados repo;
        private HelperSessionContextAccessor contextAccessor;
        private IMapper mapper;

        public EmpleadosController(RepositoryEmpleados repo, HelperSessionContextAccessor contextAccessor, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<List<EmpleadoDTO>>> GetEmpleados()
        {
            List<Empleado> empleados = await this.repo.GetEmpleadosAsync();
            List<EmpleadoDTO> lstDto = this.mapper.Map<List<EmpleadoDTO>>(empleados);
            return Ok(lstDto);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<List<EmpleadoDTO>>> EmpleadosDepartamento(int id)
        {
            List<Empleado> empleados = await this.repo.GetEmpleadosDepartamentoAsync(id);
            List<EmpleadoDTO> lstDto = this.mapper.Map<List<EmpleadoDTO>>(empleados);
            return Ok(lstDto);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<EmpleadoDTO>> FindEmpleado(int id)
        {
            Empleado empleado = await this.repo.FindEmpleadoAsync(id);
            EmpleadoDTO dto = this.mapper.Map<EmpleadoDTO>(empleado);
            return Ok(dto);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<int>> NumeroTotalEmpleados()
        {
            int numeroTotal = await this.repo.GetNumeroTotalEmpleadosAsync();
            return Ok(numeroTotal);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<decimal>> SalarioPromedioAnual()
        {
            decimal salarioPromedioAnual = await this.repo.GetSalarioPromedioAnualAsync();
            return Ok(salarioPromedioAnual);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<decimal>> SalarioPromedioAnualByDepartamento(int id)
        {
            decimal salarioPromedioAnual = await this.repo.GetSalarioPromedioAnualPorDepartamentoAsync(id);
            return Ok(salarioPromedioAnual);
        }

        [HttpPost]
        public async Task<ActionResult> Post(EmpleadoDTO dto)
        {
            Empleado empleado = this.mapper.Map<Empleado>(dto);
            empleado.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
            Empleado nuevoEmpleado = await this.repo.CreateEmpleadoAsync(empleado);
            dto.Id = nuevoEmpleado.Id;
            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult> Put(EmpleadoDTO dto)
        {
            Empleado empleado = this.mapper.Map<Empleado>(dto);
            Empleado empleadoActualizado = await this.repo.UpdateEmpleadoAsync(empleado);

            if (empleadoActualizado == null)
            {
                return NotFound(new { mensaje = "No se ha encontrado el empleado para modificar." });
            }

            return Ok(new { mensaje = "Modificado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            bool eliminado = await this.repo.DeleteEmpleadoAsync(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "No se ha encontrado el empleado para eliminar." });
            }

            return Ok(new { mensaje = "Eliminado correctamente." });
        }

    }
}
