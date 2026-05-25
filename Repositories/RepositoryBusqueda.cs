using ApiNexusERP.DTOs;
using Microsoft.EntityFrameworkCore;
using NugetModelsNexusERP.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiNexusERP.Repositories
{
    public class RepositoryBusqueda
    {
        private NexusContext context;

        public RepositoryBusqueda(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<SearchResultDTO>> BuscarGlobalAsync(string query)
        {
            var resultados = new List<SearchResultDTO>();

            if (string.IsNullOrWhiteSpace(query)) return resultados;

            // 1. Buscar Empleados
            var empleados = await this.context.Empleados
                .Where(e => e.Activo &&
                           (e.Nombre.Contains(query) || e.Apellidos.Contains(query) || e.Dni.Contains(query)))
                .Take(3)
                .Select(e => new SearchResultDTO
                {
                    Categoria = "Empleado",
                    Titulo = $"{e.Nombre} {e.Apellidos}",
                    Subtitulo = $"DNI: {e.Dni}",
                    Url = $"/Empleados/Details/{e.Id}", // Ajusta esta URL a cómo sea en tu Frontend
                    Icono = "fas fa-user"
                })
                .ToListAsync();

            // 2. Buscar Clientes
            var clientes = await this.context.Clientes
                .Where(c => c.Activo &&
                           (c.RazonSocial.Contains(query) || c.CifNif.Contains(query)))
                .Take(3)
                .Select(c => new SearchResultDTO
                {
                    Categoria = "Cliente",
                    Titulo = c.RazonSocial,
                    Subtitulo = $"CIF: {c.CifNif}",
                    Url = $"/Clientes/Details/{c.Id}",
                    Icono = "fas fa-building"
                })
                .ToListAsync();

            // 3. Buscar Facturas
            var facturas = await this.context.Facturas
                .Where(f => f.NumeroFactura.Contains(query))
                .Take(3)
                .Select(f => new SearchResultDTO
                {
                    Categoria = "Factura",
                    Titulo = $"Factura {f.NumeroFactura}",
                    Subtitulo = $"Total: {f.TotalFactura}€",
                    Url = $"/Facturacion/Details/{f.Id}",
                    Icono = "fas fa-file-invoice-dollar"
                })
                .ToListAsync();

            // Unimos todo en una sola lista
            resultados.AddRange(empleados);
            resultados.AddRange(clientes);
            resultados.AddRange(facturas);

            return resultados;
        }
    }
}
