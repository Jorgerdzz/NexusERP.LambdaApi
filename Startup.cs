using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using ApiNexusERP.Helpers;
using ApiNexusERP.Mappings;
using ApiNexusERP.Middlewares;
using ApiNexusERP.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NugetModelsNexusERP.Data;
using NugetModelsNexusERP.Helpers;
using Scalar.AspNetCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace NexusERP.LambdaApi;

public class Startup
{
    public Startup( IConfiguration configuration )
    {
        // 1. SECRETS MANAGER (Síncrono para el constructor)
        var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.USEast1);

        var request = new GetSecretValueRequest
        {
            SecretId = "NexusERP/Backend/Secrets"
        };

        // Forzamos la espera síncrona de la petición
        var response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();
        var secretosJson = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

        // Inyectamos los secretos en la configuración global
        configuration["ConnectionStrings:DefaultConnection"] = secretosJson["secretsqlnexus"];
        configuration["Jwt:Key"] = secretosJson["secretkeynexus"];

        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices( IServiceCollection services )
    {
        // --- 1. AUTENTICACIÓN Y HELPER OAUTH ---
        string secretKeyNexus = Configuration["Jwt:Key"];
        HelperActionOAuthService helper = new HelperActionOAuthService(Configuration, secretKeyNexus);

        services.AddSingleton<HelperActionOAuthService>(helper);

        services.AddAuthentication(helper.GetAuthenticationSchema())
            .AddJwtBearer(helper.GetJwtBearerOptions());

        // --- 2. BASE DE DATOS ---
        string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
        services.AddDbContext<NexusContext>(options =>
        {
            options.UseMySQL(connectionString);
            options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCustomizer, ApiNexusERP.Helpers.DateOnlyModelCustomizer>();
        });

        // --- 3. INYECCIONES PARA EL HELPER ---
        services.AddHttpContextAccessor();
        services.AddScoped<HelperSessionContextAccessor>();

        // --- 4. REPOSITORIOS ---
        services.AddTransient<RepositoryDepartamentos>();
        services.AddTransient<RepositoryClientes>();
        services.AddTransient<RepositoryEmpresas>();
        services.AddTransient<RepositoryAuth>();
        services.AddTransient<RepositoryEmpleados>();
        services.AddTransient<RepositoryNominas>();
        services.AddTransient<RepositoryContabilidad>();
        services.AddTransient<RepositoryFacturacion>();
        services.AddTransient<RepositoryEstadisticas>();
        services.AddTransient<RepositoryUsuario>();
        services.AddTransient<RepositoryBusqueda>();

        // --- 5. MAPPINGS ---
        services.AddAutoMapper(typeof(NexusProfile));

        // --- 6. CONTROLADORES Y OpenAPI ---
        services.AddControllers();
        services.AddOpenApi();

        // (Nota: No hace falta AddAWSLambdaHosting aquí. La plantilla de AWS 
        // ya lo gestiona internamente en el archivo LambdaEntryPoint.cs)
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
    {
        if ( env.IsDevelopment() )
        {
            app.UseDeveloperExceptionPage();
        }

        // Nuestro escudo protector global (Middleware personalizado al principio)
        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseRouting();

        // EL ORDEN ES VITAL: Autenticación antes que Autorización
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            // Mapeo de OpenAPI y Scalar
            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference();

            // Mapeo de Controladores de la API
            endpoints.MapControllers();

            // Redirección por defecto de la raíz hacia Scalar respetando el Stage de AWS
            endpoints.MapGet("/", context =>
            {
                var target = string.IsNullOrEmpty(context.Request.PathBase) ? "/scalar" : $"{context.Request.PathBase}/scalar";
                context.Response.Redirect(target);
                return Task.CompletedTask;
            });
        });
    }
}