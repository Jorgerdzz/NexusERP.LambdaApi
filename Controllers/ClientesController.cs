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
    public class ClientesController : ControllerBase
    {
        private RepositoryClientes repo;
        private HelperSessionContextAccessor contextAccessor;
        private IMapper mapper;

        public ClientesController(RepositoryClientes repo, HelperSessionContextAccessor contextAccessor, IMapper mapper)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ClienteDTO>>> GetClientes()
        {
            List<Cliente> clientes = await this.repo.GetClientesAsync();
            List<ClienteDTO> clientesDTO = this.mapper.Map<List<ClienteDTO>>(clientes);
            return Ok(clientesDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> FindCliente(int id)
        {
            Cliente cliente = await this.repo.FindClienteAsync(id);

            if (cliente == null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
            }

            ClienteDTO dto = this.mapper.Map<ClienteDTO>(cliente);

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult> Post(ClienteDTO dto)
        {
            Cliente cliente = this.mapper.Map<Cliente>(dto);
            cliente.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
            Cliente nuevoCliente = await this.repo.CreateClienteAsync(cliente);
            dto.Id = nuevoCliente.Id;
            return Ok(dto);
        }

        [HttpPut]
        public async Task<ActionResult> Put(ClienteDTO dto)
        {
            Cliente cliente = this.mapper.Map<Cliente>(dto);
            Cliente clienteActualizado = await this.repo.UpdateClienteAsync(cliente);

            if (clienteActualizado == null)
            {
                return NotFound(new { mensaje = "No se ha encontrado el cliente para modificar." });
            }

            return Ok(new { mensaje = "Modificado correctamente." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            bool eliminado = await this.repo.DeleteClienteAsync(id);

            if (!eliminado)
            {
                return NotFound(new { mensaje = "No se ha encontrado el cliente para eliminar." });
            }

            return Ok(new { mensaje = "Eliminado correctamente." });
        }

    }
}
