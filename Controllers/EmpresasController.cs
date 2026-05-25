using ApiNexusERP.DTOs;
using ApiNexusERP.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsNexusERP.Helpers;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresasController : ControllerBase
    {
        private RepositoryEmpresas repo;
        private HelperSessionContextAccessor contextAccessor;
        private IMapper mapper;

        public EmpresasController(RepositoryEmpresas repo, HelperSessionContextAccessor contextAccessor, IMapper mapper)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<EmpresaDTO>> FindEmpresa()
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            Empresa empresa = await this.repo.FindEmpresaAsync(idEmpresa);

            if (empresa == null) 
            {
                return NotFound(new { mensaje = "La empresa no existe." });
            }

            EmpresaDTO dto = this.mapper.Map<EmpresaDTO>(empresa);

            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult> Put(EmpresaDTO dto)
        {
            Empresa empresa = this.mapper.Map<Empresa>(dto);
            Empresa empresaActualizada = await this.repo.UpdateEmpresaAsync(empresa);

            if (empresaActualizada == null)
            {
                return NotFound(new { mensaje = "No se ha encontrado la empresa para modificar." });
            }

            return Ok(new { mensaje = "Modificado correctamente." });
        }

        [HttpDelete]
        public async Task<ActionResult> Delete()
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            bool eliminado = await this.repo.DeleteEmpresaAsync(idEmpresa);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "No se ha encontrado la empresa para eliminar." });
            }

            return Ok(new { mensaje = "Eliminado correctamente." });
        }

    }
}
