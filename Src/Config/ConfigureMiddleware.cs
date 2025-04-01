using Serilog;

namespace WebApplication.Src.Config
{
    public class ConfigureMiddleware
    {
        public static void Configure(Microsoft.AspNetCore.Builder.WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = "swagger/v1";
                });
            }
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            ConfigureExceptionHandler(app); // Chamada separada para configurar os erros
        }

        private static void ConfigureExceptionHandler(Microsoft.AspNetCore.Builder.WebApplication app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                    var (statusCode, message) = exception switch
                    {
                        ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Param"),
                        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                        BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid data"),
                        _ => (StatusCodes.Status500InternalServerError, "Internal Error")
                    };

                    context.Response.StatusCode = statusCode;
                    await context.Response.WriteAsJsonAsync(new { message });
                });
            });
        }
    }
}
