using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryDepartamentos
    {
        private NexusContext context;

        public RepositoryDepartamentos(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await this.context.Departamentos.ToListAsync();
        }

        public async Task<Departamento> FindDepartamentoAsync(int idDepartamento)
        {
            return await this.context.Departamentos
                .FirstOrDefaultAsync(d => d.Id == idDepartamento);
        }

        public async Task<Departamento> CreateDepartamentoAsync(Departamento departamento)
        {
            await this.context.Departamentos.AddAsync(departamento);
            await this.context.SaveChangesAsync();
            return departamento;
        }

        public async Task<Departamento> UpdateDepartamentoAsync(Departamento departamento)
        {
            Departamento original = await this.FindDepartamentoAsync(departamento.Id);
            
            if (original == null) return null;

            original.Nombre = departamento.Nombre;
            original.PresupuestoAnual = departamento.PresupuestoAnual;

            await this.context.SaveChangesAsync();
            return original;
        }

        public async Task<bool> DeleteDepartamentoAsync(int idDepartamento)
        {
            Departamento departamento = await this.FindDepartamentoAsync(idDepartamento);

            if (departamento == null) return false;

            this.context.Departamentos.Remove(departamento);
            await this.context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalDepartamentosAsync()
        {
            return await this.context.Departamentos.CountAsync();
        }

        public async Task<decimal> GetPresupuestoTotalAnualAsync()
        {
            return await this.context.Departamentos.SumAsync(d => (decimal?)d.PresupuestoAnual) ?? 0;
        }

        public async Task<List<(int Id, string Nombre, decimal PresupuestoAnual, int NumeroEmpleados, decimal SalarioPromedio)>> GetEstadisticasDepartamentosAsync()
        {
            var consulta = await this.context.Departamentos
                .Select(d => new
                {
                    d.Id,
                    d.Nombre,
                    d.PresupuestoAnual,
                    NumeroEmpleados = d.Empleados.Count(),
                    SalarioPromedio = d.Empleados.Any() ? d.Empleados.Average(e => e.SalarioBrutoAnual) : 0
                })
                .ToListAsync();

            return consulta.Select(c => (c.Id, c.Nombre, c.PresupuestoAnual, c.NumeroEmpleados, c.SalarioPromedio)).ToList();
        }

    }
}
