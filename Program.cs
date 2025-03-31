using Microsoft.AspNetCore.Diagnostics;
using WebApplication.Config.Db;
using WebApplication.Controllers;
using WebApplication.View;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add configuration validation first
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// Register DatabaseConfig as IDatabaseConfig
builder.Services.AddSingleton<IDatabaseConfig, DatabaseConfig>();

// Register UserViews with its dependencies
builder.Services.AddSingleton<UserViews>(provider => 
{
    var config = provider.GetRequiredService<IDatabaseConfig>();
    var logger = provider.GetRequiredService<ILogger<UserViews>>();
    return new UserViews(config, builder.Configuration, logger);
});

// Register controllers
builder.Services.AddScoped<UserController>();

// Add other services
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "To Do App",
        Version = "v1",
        Description = "API Description"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger/v1";
    });
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Param"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            BadHttpRequestException =>(StatusCodes.Status400BadRequest, "Invalid data"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Error")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { message });
    });
});
app.Run();