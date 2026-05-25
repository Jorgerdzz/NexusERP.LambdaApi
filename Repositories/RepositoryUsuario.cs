using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Models;

namespace ApiNexusERP.Repositories
{
    public class RepositoryUsuario
    {
        private NexusContext context;

        public RepositoryUsuario(NexusContext context)
        {
            this.context = context;
        }

        public async Task<Usuario> FindUsuarioAsync(int id)
        {
            return await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario> GetPerfilUsuarioAsync(int id)
        {
            return await this.context.Usuarios
                .Include(u => u.Empleado)
                    .ThenInclude(u => u.Departamento)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario> UpdatePerfilUsuarioAsync(int idUsuario, string nombre, string email)
        {
            Usuario user = await this.context.Usuarios
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.Id == idUsuario);

            if (user == null) return null;

            bool emailExiste = await this.context.Usuarios
                .AnyAsync(u => u.Email == email && u.Id != idUsuario);

            if (emailExiste) return null;

            user.Nombre = nombre;
            user.Email = email;

            if (user.Empleado != null)
            {
                user.Empleado.EmailCorporativo = email;
                this.context.Empleados.Update(user.Empleado);
            }

            await this.context.SaveChangesAsync();
            return user;
        }

    }
}
