using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiNexusERP.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Dejamos que la petición siga su camino hacia el Controlador
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // Si CUALQUIER controlador o repositorio explota, el error rebota y cae aquí
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Forzamos que la respuesta sea un JSON y el código sea 500 (Error de Servidor)
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // Creamos un objeto anónimo limpio para el Frontend
            var response = new
            {
                statusCode = context.Response.StatusCode,
                mensaje = "Ha ocurrido un error interno en el servidor. Por favor, contacte con soporte técnico.",
                detalle = exception.Message,
                stackTrace = exception.StackTrace
            };

            var jsonResponse = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}