using ApiNexusERP.DTOs;
using AutoMapper;
using NugetModelsNexusERP.Models;

namespace ApiNexusERP.Mappings
{
    public class NexusProfile: Profile
    {
        public NexusProfile()
        {
            // --- DEPARTAMENTOS ---

            CreateMap<Departamento, DepartamentoDTO>();
            CreateMap<DepartamentoDTO, Departamento>();

            // --- CLIENTES ---

            CreateMap<Cliente, ClienteDTO>();
            CreateMap<ClienteDTO, Cliente>();

            // --- EMPRESAS ---

            CreateMap<Empresa, EmpresaDTO>();
            CreateMap<EmpresaDTO, Empresa>();

            // --- USUARIOS ---
            CreateMap<Usuario, UsuarioDTO>();
            CreateMap<UsuarioDTO, Usuario>();

            // --- EMPLEADOS ---

            CreateMap<Empleado, EmpleadoDTO>()
                .ForMember(dest => dest.NombreDepartamento, opt => opt.MapFrom(src => src.Departamento.Nombre))
                .ForMember(dest => dest.IbanEnmascarado, opt => opt.MapFrom(src =>
                    string.IsNullOrEmpty(src.Iban) || src.Iban.Length < 4
                    ? src.Iban
                    : $"**** **** **** {src.Iban.Substring(src.Iban.Length - 4)}"));
            CreateMap<EmpleadoDTO, Empleado>();

            // --- NOMINAS ---

            CreateMap<NominaDetalle, NominaDetalleDTO>();
            CreateMap<NominaDetalleDTO, NominaDetalle>(MemberList.None);

            CreateMap<Nomina, NominaDTO>()
                .ForMember(dest => dest.NombreCompletoEmpleado, opt => opt.MapFrom(src => src.Empleado.Nombre + " " + src.Empleado.Apellidos))
                .ForMember(dest => dest.DniEmpleado, opt => opt.MapFrom(src => src.Empleado.Dni))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.NominaDetalles));

            CreateMap<NominaDTO, Nomina>(MemberList.None)
                .ForMember(dest => dest.NominaDetalles, opt => opt.MapFrom(src => src.Detalles));

            // --- CONTABILIDAD ---

            // 1. Cuentas
            CreateMap<CuentasContable, CuentaContableDTO>();
            CreateMap<CuentaContableDTO, CuentasContable>();

            // 2. Apuntes (Aplanando la cuenta relacionada)
            CreateMap<ApuntesContable, ApunteContableDTO>()
                .ForMember(dest => dest.CuentaCodigo, opt => opt.MapFrom(src => src.Cuenta.Codigo))
                .ForMember(dest => dest.CuentaNombre, opt => opt.MapFrom(src => src.Cuenta.Nombre));

            // 3. Asientos (Incluyendo su lista de apuntes)
            CreateMap<AsientosContable, AsientoContableDTO>()
                .ForMember(dest => dest.Apuntes, opt => opt.MapFrom(src => src.ApuntesContables));


            // --- FACTURACIÓN ---

            // 1. Detalles de Factura
            CreateMap<FacturaDetalle, FacturaDetalleDTO>();
            CreateMap<FacturaDetalleDTO, FacturaDetalle>(MemberList.None);

            // 2. Cabecera de Factura (Ida)
            CreateMap<Factura, FacturaDTO>()
                .ForMember(dest => dest.ClienteRazonSocial, opt => opt.MapFrom(src => src.Cliente.RazonSocial))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.FacturaDetalles));

            // 3. Cabecera de Factura (Vuelta)
            CreateMap<FacturaDTO, Factura>(MemberList.None)
                .ForMember(dest => dest.FacturaDetalles, opt => opt.MapFrom(src => src.Detalles));


            // --- ESTADISTICAS ---
            CreateMap<ReporteMensualDTO, ReporteMensualDTO>(); // Mapeos simples para colecciones
            CreateMap<ReporteDepartamentoDTO, ReporteDepartamentoDTO>();
            CreateMap<MetricasDashboardDTO, MetricasDashboardDTO>();
        }
    }
}
