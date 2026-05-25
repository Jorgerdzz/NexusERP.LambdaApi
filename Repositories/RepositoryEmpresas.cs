using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryEmpresas
    {
        private NexusContext context;

        public RepositoryEmpresas(NexusContext context)
        {
            this.context = context;
        }

        public async Task<Empresa> FindEmpresaAsync(int idEmpresa)
        {
            return await this.context.Empresas
                .FirstOrDefaultAsync(e => e.Id == idEmpresa);
        }

        public async Task<Empresa> UpdateEmpresaAsync(Empresa empresa)
        {
            Empresa original = await this.FindEmpresaAsync(empresa.Id);
            if (original == null) return null;
            bool cifExiste = await this.context.Empresas.AnyAsync(e => e.Cif == empresa.Cif && e.Id != empresa.Id);
            if (cifExiste) return null;
            original.NombreComercial = empresa.NombreComercial;
            original.RazonSocial = empresa.RazonSocial;
            original.Cif = empresa.Cif;
            await this.context.SaveChangesAsync();
            return original;
        }

        public async Task<bool> DeleteEmpresaAsync(int idEmpresa)
        {
            Empresa empresa = await this.FindEmpresaAsync(idEmpresa);

            if (empresa == null) return false;

            this.context.Empresas.Remove(empresa);
            await this.context.SaveChangesAsync();
            return true;
        }
    }
}
