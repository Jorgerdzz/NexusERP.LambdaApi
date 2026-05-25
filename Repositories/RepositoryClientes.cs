using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryClientes
    {
        private NexusContext context;

        public RepositoryClientes(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            return await this.context.Clientes
                .OrderBy(c => c.RazonSocial)
                .ToListAsync();
        }

        public async Task<Cliente> FindClienteAsync(int idCliente)
        {
            return await this.context.Clientes
                .FirstOrDefaultAsync(C => C.Id == idCliente);
        }

        public async Task<Cliente> CreateClienteAsync(Cliente cliente)
        {
            await this.context.AddAsync(cliente);
            await this.context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cliente> UpdateClienteAsync(Cliente cliente)
        {
            Cliente original = await this.FindClienteAsync(cliente.Id);

            if (original == null) return null;

            original.RazonSocial = cliente.RazonSocial;
            original.CifNif = cliente.CifNif;
            original.Email = cliente.Email;
            original.Activo = cliente.Activo;

            await this.context.SaveChangesAsync();
            return original;
        }

        public async Task<bool> DeleteClienteAsync(int idCliente)
        {
            Cliente cliente = await this.FindClienteAsync(idCliente);
            if (cliente == null) return false;
            this.context.Clientes.Remove(cliente);
            await this.context.SaveChangesAsync();
            return true;
        }

    }
}
