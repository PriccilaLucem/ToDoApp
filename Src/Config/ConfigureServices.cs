using WebApplication.View;
using WebApplication.Src.Config.Db;
using Serilog;
using WebApplication.Src.Controllers;
using WebApplication.Src.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApplication.Src.Config
{
    public class ServiceConfiguration
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            var jwtSettings = new JwtSettings(builder.Configuration); 
            ConfigureJwt(builder);
            ConfigureMongoDB(builder);
            ConfigureDependencyInjection(builder);
            ConfigureSwagger(builder);
            ConfigureControllers(builder);
            ConfigureLogger(builder);
            ConfigureAuthorization(builder, jwtSettings);
        }
        private static void ConfigureLogger(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console() 
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day) // Criar um novo arquivo por dia
            .CreateLogger();
            builder.Host.UseSerilog();
        }
        private static void ConfigureJwt(WebApplicationBuilder builder)
        {
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddSingleton<JwtSettings>();
        }

        private static void ConfigureMongoDB(WebApplicationBuilder builder)
        {
            builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
            builder.Services.AddSingleton<IDatabaseConfig, DatabaseConfig>();
        }

        private static void ConfigureDependencyInjection(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<TaskViews>(provider =>
            {
                var config = provider.GetRequiredService<IDatabaseConfig>();
                var logger = provider.GetRequiredService<ILogger<TaskViews>>();
                return new TaskViews(config, builder.Configuration, logger);
            });

            builder.Services.AddSingleton<UserViews>(provider =>
            {
                var config = provider.GetRequiredService<IDatabaseConfig>();
                var logger = provider.GetRequiredService<ILogger<UserViews>>();
                return new UserViews(config, builder.Configuration, logger);
            });

            // Controllers
            builder.Services.AddScoped<TaskController>();
            builder.Services.AddScoped<UserController>();
            builder.Services.AddScoped<LoginController>();
        }

        private static void ConfigureSwagger(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "To Do App",
                    Version = "v1",
                    Description = "API Description"
                });
            });
        }

        private static void ConfigureControllers(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(TaskMapper));
            builder.Services.AddAutoMapper(typeof(MappingProfile));
        }

        private static void ConfigureAuthorization(WebApplicationBuilder builder, JwtSettings jwtSettings)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, 
                        ValidateAudience = false,
                        ValidateLifetime = true, 
                        ValidateIssuerSigningKey = true, 
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });
        }
    }
}
