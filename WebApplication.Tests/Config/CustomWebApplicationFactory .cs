using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using WebApplication.Src.Interface;
using WebApplication.Src.Models;
using WebApplication.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication.WebApplication.Tests.Config
{   
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly TestAuthSettings _testAuthSettings = new();
        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();

        public void AddServiceConfiguration(Action<IServiceCollection> configuration)
        {
            _serviceConfigurations.Add(configuration);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUserView>();
                services.RemoveAll<ITaskViews>();

                var userViewMock = new Mock<IUserView>();
                userViewMock.Setup(x => x.CreateUsers(It.IsAny<UserModel>()))
                        .ReturnsAsync("generated-id");
                services.AddScoped<IUserView>(_ => userViewMock.Object);

                foreach (var config in _serviceConfigurations)
                {
                    config(services);
                }

                services.AddSingleton(_testAuthSettings);
            });
        }
    public HttpClient GetAuthenticatedClient(string userId = "test-user", string email = "test@example.com")
    {
        var client = CreateClient();
        
        var token = JwtTokenHelper.GenerateTestToken(
            _testAuthSettings.SecretKey,
            _testAuthSettings.Issuer,
            _testAuthSettings.Audience,
            userId,
            email);

        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
    }
}