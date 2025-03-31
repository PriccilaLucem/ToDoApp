using Microsoft.AspNetCore.Diagnostics;
using WebApplication.Config;
using WebApplication.Config.Db;
using WebApplication.Controllers;
using WebApplication.Util;
using WebApplication.View;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtSettings>();

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));


builder.Services.AddSingleton<IDatabaseConfig, DatabaseConfig>();
builder.Services.AddSingleton<TaskViews>(provider =>
{

    var config = provider.GetRequiredService<IDatabaseConfig>();
    var logger = provider.GetRequiredService<ILogger<TaskViews>>();
    return new TaskViews(config, builder.Configuration, logger); 
}
);

builder.Services.AddSingleton<UserViews>(provider => 
{
    var config = provider.GetRequiredService<IDatabaseConfig>();
    var logger = provider.GetRequiredService<ILogger<UserViews>>();
    return new UserViews(config, builder.Configuration, logger);
});


builder.Services.AddScoped<TaskController>();
builder.Services.AddScoped<UserController>();
builder.Services.AddScoped<LoginController>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(TaskMapper));
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