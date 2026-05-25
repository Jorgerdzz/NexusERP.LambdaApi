using ApiNexusERP.Helpers;
using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryEmpleados
    {
        private NexusContext context;

        public RepositoryEmpleados(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .ToListAsync();
        }

        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .FirstOrDefaultAsync(e => e.Id == idEmpleado);
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync(int idDepartamento)
        {
            return await this.context.Empleados
                .Include(e => e.Departamento)
                .Where(e => e.DepartamentoId == idDepartamento)
                .ToListAsync();
        }

        public async Task<int> GetNumeroTotalEmpleadosAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<decimal> GetSalarioPromedioAnualAsync()
        {
            return await this.context.Empleados.AverageAsync(e => (decimal?)e.SalarioBrutoAnual) ?? 0;
        }

        public async Task<decimal> GetSalarioPromedioAnualPorDepartamentoAsync(int idDepartamento)
        {
            return await this.context.Empleados
                .Where(e => e.DepartamentoId == idDepartamento)
                .AverageAsync(e => (decimal?)e.SalarioBrutoAnual) ?? 0;
        }

        public async Task<Empleado> CreateEmpleadoAsync(Empleado empleado)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await this.context.Empleados.AddAsync(empleado);
                await this.context.SaveChangesAsync();

                string passwordPorDefecto = "1234";

                Usuario user = new Usuario
                {
                    EmpresaId = empleado.EmpresaId,
                    Nombre = empleado.Nombre + " " + empleado.Apellidos,
                    Email = empleado.EmailCorporativo,
                    Rol = 2, 
                    EmpleadoId = empleado.Id,
                    Activo = true,
                    Password = passwordPorDefecto
                };

                await this.context.Usuarios.AddAsync(user);
                await this.context.SaveChangesAsync();

                SeguridadUsuario userSecurity = new SeguridadUsuario
                {
                    IdUsuario = user.Id,
                    Salt = HelperTools.GenerateSalt(),
                };

                userSecurity.PasswordHash = HelperCryptography.EncryptPassword(passwordPorDefecto, userSecurity.Salt);

                await this.context.SeguridadUsuarios.AddAsync(userSecurity);
                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();
                return empleado;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Empleado> UpdateEmpleadoAsync(Empleado emp)
        {
            Empleado original = await this.context.Empleados.FirstOrDefaultAsync(e => e.Id == emp.Id);

            if (original == null) return null; 

            original.Nombre = emp.Nombre;
            original.Apellidos = emp.Apellidos;
            original.Dni = emp.Dni;
            original.EmailCorporativo = emp.EmailCorporativo;
            original.Telefono = emp.Telefono;
            original.DepartamentoId = emp.DepartamentoId;
            original.SalarioBrutoAnual = emp.SalarioBrutoAnual;

            if (!string.IsNullOrEmpty(emp.Iban))
            {
                original.Iban = emp.Iban;
            }

            original.Activo = emp.Activo;

            this.context.Empleados.Update(original);
            await this.context.SaveChangesAsync(); 

            return original;
        }

        public async Task<bool> DeleteEmpleadoAsync(int id)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                Empleado emp = await this.context.Empleados.FirstOrDefaultAsync(e => e.Id == id);
                if (emp == null) return false;

                Usuario user = await this.context.Usuarios.FirstOrDefaultAsync(u => u.EmpleadoId == emp.Id);
                if (user != null)
                {
                    SeguridadUsuario userSecurity = await this.context.SeguridadUsuarios.FirstOrDefaultAsync(s => s.IdUsuario == user.Id);
                    if (userSecurity != null) this.context.SeguridadUsuarios.Remove(userSecurity);
                    this.context.Usuarios.Remove(user);
                }

                var conceptosFijos = await this.context.ConceptosFijosEmpleados.Where(c => c.EmpleadoId == emp.Id).ToListAsync();
                if (conceptosFijos.Any()) this.context.ConceptosFijosEmpleados.RemoveRange(conceptosFijos);

                this.context.Empleados.Remove(emp);
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
