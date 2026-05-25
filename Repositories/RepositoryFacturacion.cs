using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryFacturacion
    {
        private NexusContext context;

        public RepositoryFacturacion(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Factura>> GetFacturasAsync()
        {
            return await this.context.Facturas
                .Include(f => f.Cliente)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();
        }

        public async Task<Factura> FindFacturaAsync(int idFactura)
        {
            return await this.context.Facturas
                .Include(f => f.FacturaDetalles)
                .Include(f => f.Cliente)
                .FirstOrDefaultAsync(f => f.Id == idFactura);
        }

        public async Task<Factura> EmitirFacturaAsync(Factura factura, int empresaId)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                factura.EmpresaId = empresaId;

                // 1. OBTENER CUENTAS
                var c430 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "4300000");
                var c700 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "7000000");
                var c477 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == "4770000");

                if (c430 == null || c700 == null || c477 == null)
                    throw new Exception("Faltan cuentas maestras (430, 700 o 477) en el Plan Contable.");

                var cliente = await this.context.Clientes.FindAsync(factura.ClienteId);

                // 2. ASIENTO CONTABLE
                AsientosContable asiento = new AsientosContable
                {
                    EmpresaId = empresaId,
                    Fecha = factura.FechaEmision,
                    Glosa = $"Factura {factura.NumeroFactura} - {cliente.RazonSocial}"
                };

                await this.context.AsientosContables.AddAsync(asiento);
                await this.context.SaveChangesAsync();

                // 3. APUNTES (Cliente, Ventas, IVA)
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c430.Id, Debe = factura.TotalFactura, Haber = 0 });
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c700.Id, Debe = 0, Haber = factura.BaseImponible });
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asiento.Id, CuentaId = c477.Id, Debe = 0, Haber = factura.IvaTotal });

                await this.context.SaveChangesAsync();

                // 4. GUARDAR FACTURA
                factura.AsientoId = asiento.Id;
                await this.context.Facturas.AddAsync(factura);
                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();

                return factura; // Devolvemos la factura con su nuevo ID
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CobrarFacturaAsync(int idFactura, int empresaId)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                Factura factura = await this.context.Facturas
                    .Include(f => f.Cliente)
                    .FirstOrDefaultAsync(f => f.Id == idFactura);

                if (factura == null || factura.Estado == "Pagada") return false;

                var c572 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "5720000");
                var c430 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "4300000");

                if (c572 == null || c430 == null)
                    throw new Exception("Faltan cuentas maestras de cobro (572 o 430).");

                AsientosContable asientoCobro = new AsientosContable
                {
                    EmpresaId = empresaId,
                    Fecha = DateTime.Now,
                    Glosa = $"Cobro Factura {factura.NumeroFactura} - {factura.Cliente.RazonSocial}"
                };

                await this.context.AsientosContables.AddAsync(asientoCobro);
                await this.context.SaveChangesAsync();

                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asientoCobro.Id, CuentaId = c572.Id, Debe = factura.TotalFactura, Haber = 0 });
                await this.context.ApuntesContables.AddAsync(new ApuntesContable { AsientoId = asientoCobro.Id, CuentaId = c430.Id, Debe = 0, Haber = factura.TotalFactura });

                factura.Estado = "Pagada";
                this.context.Facturas.Update(factura);

                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
