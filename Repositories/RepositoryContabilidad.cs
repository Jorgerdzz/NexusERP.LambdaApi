using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryContabilidad
    {
        private NexusContext context;

        public RepositoryContabilidad(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<CuentasContable>> GetPlanContableAsync()
        {
            return await this.context.CuentasContables
                .OrderBy(c => c.Codigo)
                .ToListAsync();
        }

        public async Task<CuentasContable> FindCuentaContableAsync(int id)
        {
            return await this.context.CuentasContables
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CuentasContable> CrearCuentaContableAsync(CuentasContable cuenta)
        {
            bool existe = await this.context.CuentasContables
                               .AnyAsync(c => c.Codigo == cuenta.Codigo);

            if (existe) throw new Exception("Ya existe una cuenta con este código.");

            await this.context.CuentasContables.AddAsync(cuenta);
            await this.context.SaveChangesAsync();
            return cuenta;
        }

        public async Task<List<AsientosContable>> GetLibroDiarioAsync()
        {
            return await this.context.AsientosContables
                .Include(a => a.ApuntesContables)
                    .ThenInclude(ap => ap.Cuenta)
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<List<ApuntesContable>> GetExtractoCuentaAsync(int cuentaId, DateTime desde, DateTime hasta)
        {
            return await this.context.ApuntesContables
                .Include(ap => ap.Asiento)
                .Where(ap => ap.CuentaId == cuentaId && ap.Asiento.Fecha >= desde && ap.Asiento.Fecha <= hasta)
                .OrderBy(ap => ap.Asiento.Fecha)
                .ToListAsync();
        }

        public async Task<decimal> GetSaldoAnteriorAsync(int cuentaId, DateTime desde)
        {
            var apuntesAnteriores = await this.context.ApuntesContables
                .Include(ap => ap.Asiento)
                .Where(ap => ap.CuentaId == cuentaId && ap.Asiento.Fecha < desde)
                .ToListAsync();

            return apuntesAnteriores.Sum(ap => (ap.Debe ?? 0) - (ap.Haber ?? 0));
        }

    }
}
