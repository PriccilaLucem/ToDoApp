
using WebApplication.Src.Config;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

ServiceConfiguration.Configure(builder);
var app = builder.Build();

ConfigureMiddleware.Configure(app);
app.Run();
