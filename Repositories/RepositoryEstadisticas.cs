using ApiNexusERP.DTOs;
using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;

namespace ApiNexusERP.Repositories
{
    public class RepositoryEstadisticas
    {
        private NexusContext context;

        public RepositoryEstadisticas(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<ReporteMensualDTO>> GetIngresosPorMesAsync(int anio)
        {
            return await this.context.Facturas
                .Where(f => f.FechaEmision.Year == anio)
                .GroupBy(f => f.FechaEmision.Month)
                .Select(g => new ReporteMensualDTO
                {
                    Mes = g.Key,
                    Total = g.Sum(f => f.TotalFactura)
                })
                .OrderBy(r => r.Mes)
                .ToListAsync();
        }

        public async Task<List<ReporteMensualDTO>> GetGastosPorMesAsync(int anio)
        {
            return await this.context.ControlGastos
                .Where(c => c.Anio == anio)
                .GroupBy(c => c.Mes)
                .Select(g => new ReporteMensualDTO
                {
                    Mes = g.Key,
                    Total = g.Sum(c => c.ImporteGasto)
                })
                .OrderBy(r => r.Mes)
                .ToListAsync();
        }

        public async Task<List<ReporteDepartamentoDTO>> GetCostesPorDepartamentoAsync(int anio)
        {
            return await this.context.ControlGastos
                .Include(c => c.Departamento)
                .Where(c => c.Anio == anio)
                .GroupBy(c => c.Departamento.Nombre)
                .Select(g => new ReporteDepartamentoDTO
                {
                    Departamento = g.Key ?? "Sin Departamento",
                    Total = g.Sum(c => c.ImporteGasto)
                })
                .OrderByDescending(r => r.Total)
                .ToListAsync();
        }

        public async Task<MetricasDashboardDTO> GetEstadisticasAsync(int anio)
        {
            MetricasDashboardDTO metricas = new MetricasDashboardDTO();

            // SEGURIDAD: Añadimos el filtro por empresa en los AnyAsync
            metricas.TieneDepartamentos = await this.context.Departamentos.AnyAsync();
            metricas.TieneClientes = await this.context.Clientes.AnyAsync();
            metricas.TieneEmpleados = await this.context.Empleados.AnyAsync();

            metricas.TotalFacturadoAnual = await this.context.Facturas
                .Where(f => f.FechaEmision.Year == anio)
                .SumAsync(f => f.TotalFactura);

            metricas.TotalGastoSalarial = await this.context.ControlGastos
                .Where(c => c.Anio == anio)
                .SumAsync(c => c.ImporteGasto);

            metricas.FacturasPendientes = await this.context.Facturas
                .CountAsync(f => f.Estado == "Pendiente");

            return metricas;
        }
    }
}
