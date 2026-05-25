using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;

namespace ApiNexusERP.Repositories
{
    public class RepositoryNominas
    {
        private NexusContext context;

        public RepositoryNominas(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Empleado>> GetEmpleadosConNominasAsync(int mes, int anio)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.Nominas.Where(n => n.Mes == mes && n.Anio == anio))
                .Where(e => e.Activo == true)
                .ToListAsync();
        }

        public async Task<Empleado> GetEmpleadoParaNominaAsync(int empleadoId)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .Include(e => e.ConceptosFijosEmpleados.Where(c => c.Activo == true))
                    .ThenInclude(cf => cf.Concepto)
                .FirstOrDefaultAsync(e => e.Id == empleadoId && e.Activo == true);
        }

        public async Task<Nomina> FindNominaAsync(int idNomina)
        {
            return await this.context.Nominas
                .Include(n => n.Empleado)
                .Include(n => n.Empresa)
                .Include(n => n.NominaDetalles)
                .FirstOrDefaultAsync(n => n.Id == idNomina);
        }

        public async Task<Nomina> FindNominaEmpleadoByMesAsync(int idEmpleado, int mes, int anio)
        {
            return await this.context.Nominas
                .Include(e => e.Empleado)
                .Include(e => e.NominaDetalles)
                .FirstOrDefaultAsync(e => e.EmpleadoId == idEmpleado && e.Mes == mes && e.Anio == anio);
        }

        public async Task<Nomina> GuardarNominaCompletaAsync(Nomina nomina)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();

            try
            {

                await this.context.Nominas.AddAsync(nomina);
                await this.context.SaveChangesAsync();

                foreach (var detalle in nomina.NominaDetalles)
                {
                    var conceptoCatalogo = await this.context.ConceptosSalariales
                        .FirstOrDefaultAsync(c => c.Codigo == detalle.Codigo && c.EmpresaId == nomina.EmpresaId);

                    if (conceptoCatalogo == null)
                    {
                        throw new Exception($"El concepto salarial '{detalle.Codigo}' no existe en el catálogo de esta empresa. Revise la configuración.");
                    }

                    detalle.ConceptoId = conceptoCatalogo.Id;
                    this.context.NominaDetalles.Update(detalle);

                    if (detalle.Tipo == 1 && detalle.Codigo != "H_EXTRA")
                    {
                        var conceptoFijo = await this.context.ConceptosFijosEmpleados
                            .FirstOrDefaultAsync(cf => cf.EmpleadoId == nomina.EmpleadoId && cf.ConceptoId == conceptoCatalogo.Id);

                        if (conceptoFijo != null)
                        {
                            conceptoFijo.ImporteFijo = detalle.Importe;
                            conceptoFijo.Activo = true;
                            this.context.ConceptosFijosEmpleados.Update(conceptoFijo);
                        }
                        else
                        {
                            ConceptosFijosEmpleado nuevoFijo = new ConceptosFijosEmpleado
                            {
                                EmpresaId = nomina.EmpresaId,
                                EmpleadoId = nomina.EmpleadoId,
                                ConceptoId = conceptoCatalogo.Id,
                                ImporteFijo = detalle.Importe,
                                Activo = true
                            };
                            await this.context.ConceptosFijosEmpleados.AddAsync(nuevoFijo);
                        }
                    }
                }

                List<CuentasContable> cuentas = await this.context.CuentasContables
                    .Where(c => c.EmpresaId == nomina.EmpresaId &&
                               (c.Codigo == "6400000" || c.Codigo == "6420000" || c.Codigo == "4760000" || c.Codigo == "4751000" || c.Codigo == "4650000"))
                    .ToListAsync();

                CuentasContable c640 = cuentas.FirstOrDefault(c => c.Codigo == "6400000");
                CuentasContable c642 = cuentas.FirstOrDefault(c => c.Codigo == "6420000");
                CuentasContable c476 = cuentas.FirstOrDefault(c => c.Codigo == "4760000");
                CuentasContable c4751 = cuentas.FirstOrDefault(c => c.Codigo == "4751000");
                CuentasContable c465 = cuentas.FirstOrDefault(c => c.Codigo == "4650000");

                if (c640 == null || c642 == null || c476 == null || c4751 == null || c465 == null)
                {
                    throw new Exception("Error contable: Faltan cuentas maestras en la configuración de la empresa.");
                }

                Empleado empleado = await this.context.Empleados.FirstOrDefaultAsync(e => e.Id == nomina.EmpleadoId);
                string nombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}".Trim() : $"Empleado ID {nomina.EmpleadoId}";

                AsientosContable asiento = new AsientosContable
                {
                    EmpresaId = nomina.EmpresaId,
                    Fecha = nomina.FechaGeneracion ?? DateTime.Now,
                    Glosa = $"Nómina {nomina.Mes:D2}/{nomina.Anio} - {nombreEmpleado}"
                };

                await this.context.AsientosContables.AddAsync(asiento);
                await this.context.SaveChangesAsync();

                List<ApuntesContable> apuntes = new List<ApuntesContable>();

                // ---- DEBE (Gastos de la empresa) ----
                ApuntesContable salarioBruto = new ApuntesContable
                {
                    AsientoId = asiento.Id,
                    CuentaId = c640.Id,
                    Debe = nomina.TotalDevengado,
                    Haber = 0
                };

                apuntes.Add(salarioBruto);

                ApuntesContable costeSSempresa = new ApuntesContable
                {
                    AsientoId = asiento.Id,
                    CuentaId = c642.Id,
                    Debe = nomina.SsEmpresaTotal,
                    Haber = 0
                };

                apuntes.Add(costeSSempresa);

                // ---- HABER (Deudas generadas) ----
                decimal retencionIrpf = nomina.NominaDetalles.FirstOrDefault(d => d.Codigo == "DED_IRPF")?.Importe ?? 0;
                ApuntesContable irpf = new ApuntesContable
                {
                    AsientoId = asiento.Id,
                    CuentaId = c4751.Id,
                    Debe = 0,
                    Haber = retencionIrpf
                };
                apuntes.Add(irpf);

                decimal ssTrabajador = nomina.TotalDeducciones - retencionIrpf;
                decimal cuotaTotalSS = ssTrabajador + nomina.SsEmpresaTotal;
                ApuntesContable seguridadSocial = new ApuntesContable
                {
                    AsientoId = asiento.Id,
                    CuentaId = c476.Id,
                    Debe = 0,
                    Haber = cuotaTotalSS
                };
                apuntes.Add(seguridadSocial);

                ApuntesContable liquidoApagar = new ApuntesContable
                {
                    AsientoId = asiento.Id,
                    CuentaId = c465.Id,
                    Debe = 0,
                    Haber = nomina.LiquidoApercibir
                };
                apuntes.Add(liquidoApagar);

                await this.context.ApuntesContables.AddRangeAsync(apuntes);

                nomina.AsientoId = asiento.Id;
                this.context.Nominas.Update(nomina);

                await this.context.SaveChangesAsync();

                ControlGasto nuevoGasto = new ControlGasto
                {
                    EmpresaId = nomina.EmpresaId,
                    DepartamentoId = empleado.DepartamentoId,
                    EmpleadoId = nomina.EmpleadoId,
                    NominaId = nomina.Id,
                    Mes = nomina.Mes,
                    Anio = nomina.Anio,
                    ImporteGasto = nomina.TotalDevengado + nomina.SsEmpresaTotal,
                    FechaRegistro = DateTime.Now
                };

                await this.context.ControlGastos.AddAsync(nuevoGasto);
                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();

                return nomina;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<bool> PagarNominaAsync(int idNomina)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();

            try
            {
                Nomina nominaApagar = await this.context.Nominas
                    .Include(n => n.Empleado)
                    .FirstOrDefaultAsync(n => n.Id == idNomina);
                if (nominaApagar == null) return false;
                if (nominaApagar.Estado == "Pagada") return false;

                CuentasContable c572 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "5720000" && c.EmpresaId == nominaApagar.EmpresaId);
                CuentasContable c465 = await this.context.CuentasContables.FirstOrDefaultAsync(c => c.Codigo == "4650000" && c.EmpresaId == nominaApagar.EmpresaId);

                if (c572 == null || c465 == null)
                {
                    return false; // Faltan las cuentas contables maestras correspondientes (572 o 465) para proceder al pago
                }

                AsientosContable asientoPago = new AsientosContable
                {
                    EmpresaId = nominaApagar.EmpresaId,
                    Fecha = DateTime.Now,
                    Glosa = $"Pago Nómina {nominaApagar.Mes} {nominaApagar.Anio} - {nominaApagar.Empleado.Nombre} {nominaApagar.Empleado.Apellidos}"
                };

                await this.context.AsientosContables.AddAsync(asientoPago);
                await this.context.SaveChangesAsync();

                ApuntesContable cuenta465 = new ApuntesContable
                {
                    AsientoId = asientoPago.Id,
                    CuentaId = c465.Id,
                    Debe = nominaApagar.LiquidoApercibir,
                    Haber = 0
                };

                await this.context.ApuntesContables.AddAsync(cuenta465);

                ApuntesContable cuenta572 = new ApuntesContable
                {
                    AsientoId = asientoPago.Id,
                    CuentaId = c572.Id,
                    Debe = 0,
                    Haber = nominaApagar.LiquidoApercibir
                };

                await this.context.ApuntesContables.AddAsync(cuenta572);

                nominaApagar.Estado = "Pagada";

                this.context.Nominas.Update(nominaApagar);

                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;

            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }

        }

    }
}
