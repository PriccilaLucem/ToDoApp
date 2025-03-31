using WebApplication.Util;
using WebApplication.Controllers;
using WebApplication.View;
using WebApplication.Config.Db;

namespace WebApplication.Config
{
    public class ServiceConfiguration
    {
        public static void Configure(WebApplicationBuilder builder)
        {
            ConfigureJwt(builder);
            ConfigureMongoDB(builder);
            ConfigureDependencyInjection(builder);
            ConfigureSwagger(builder);
            ConfigureControllers(builder);
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
    }
}
