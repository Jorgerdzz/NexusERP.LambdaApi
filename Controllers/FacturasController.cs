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
    public class FacturasController : ControllerBase
    {
        private RepositoryFacturacion repo;
        private IMapper mapper;
        private HelperSessionContextAccessor contextAccessor;

        public FacturasController(RepositoryFacturacion repo, IMapper mapper, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.contextAccessor = contextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<List<FacturaDTO>>> GetFacturas()
        {
            var facturas = await this.repo.GetFacturasAsync();
            return Ok(this.mapper.Map<List<FacturaDTO>>(facturas));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacturaDTO>> FindFactura(int id)
        {
            var factura = await this.repo.FindFacturaAsync(id);

            if (factura == null) return NotFound(new { mensaje = "Factura no encontrada." });

            return Ok(this.mapper.Map<FacturaDTO>(factura));
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<FacturaDTO>> Emitir(FacturaDTO facturaDTO)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();

            var factura = this.mapper.Map<Factura>(facturaDTO);
            var facturaGuardada = await this.repo.EmitirFacturaAsync(factura, idEmpresa);

            return Ok(this.mapper.Map<FacturaDTO>(facturaGuardada));
        }

        [HttpPut("[action]/{idFactura}")]
        public async Task<ActionResult> Cobrar(int idFactura)
        {
            int idEmpresa = this.contextAccessor.GetEmpresaIdSession();
            bool exito = await this.repo.CobrarFacturaAsync(idFactura, idEmpresa);

            if (!exito)
            {
                return BadRequest(new { mensaje = "No se pudo cobrar la factura. Puede que ya esté pagada o no exista." });
            }

            return Ok(new { mensaje = "Factura cobrada y contabilizada con éxito." });
        }

    }
}
