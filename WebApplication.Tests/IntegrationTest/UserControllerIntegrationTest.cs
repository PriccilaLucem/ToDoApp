using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WebApplication.Src.Config;
using WebApplication.Src.Dto.User;
using WebApplication.Src.Interface;
using WebApplication.Src.Models;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Json;
using WebApplication.WebApplication.Tests.Util;
using Xunit.Abstractions;

namespace WebApplication.Tests.Integration
{
    public class TestAuthSettings
    {
        public string SecretKey { get; set; } = "TestSecretKeyWithAtLeast32CharactersLong";
        public string Issuer { get; set; } = "TestIssuer";
        public string Audience { get; set; } = "TestAudience";
        public int ExpiryMinutes { get; set; } = 30;
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly TestAuthSettings _testAuthSettings = new();
        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Adicionar configurações de mock de serviço
                var userViewMock = new Mock<IUserView>();
                userViewMock.Setup(x => x.CreateUsers(It.IsAny<UserModel>()))
                            .ReturnsAsync("generated-id");
                services.AddScoped<IUserView>(_ => userViewMock.Object);
                services.AddSingleton(_testAuthSettings);

                // Aplica todas as configurações adicionais fornecidas
                foreach (var config in _serviceConfigurations)
                {
                    config(services);
                }
            });
        }

        public HttpClient GetAuthenticatedClient(
            string userId = "test-user",
            string email = "test@example.com")
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

        // Método para adicionar configurações de serviços extras
        public void AddServiceConfiguration(Action<IServiceCollection> configuration)
        {
            _serviceConfigurations.Add(configuration);
        }
    }

    public static class JwtTokenHelper
    {
        public static string GenerateTestToken(
            string secretKey,
            string issuer,
            string audience,
            string userId,
            string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly TestLogger _logger;

        public UserControllerIntegrationTest(ITestOutputHelper output)
        {
            _logger = new TestLogger(output);
            _factory = new CustomWebApplicationFactory();
            _client = _factory.GetAuthenticatedClient();
            _logger.LogInfo("Test initialized");
        }

        [Fact]
        public async Task PostUser_ReturnsCreated_WithValidToken()
        {
            try
            {
                _logger.LogInfo("Starting PostUser_ReturnsCreated_WithValidToken test");
                
                var userDto = new CreateUserDTO
                {
                    Name = "Test User",
                    Email = $"test-{Guid.NewGuid()}@example.com",
                    BirthDate = DateTime.UtcNow,
                    Password = "ValidPassword123!"
                };

                _logger.LogInfo($"Creating user with email: {userDto.Email}");
                
                var response = await _client.PostAsJsonAsync("/api/v1/users", userDto);
                
                _logger.LogInfo($"Received response status: {response.StatusCode}");
                
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(response.Headers.Location);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            try
            {
                _logger.LogInfo("Starting GetUserById_ReturnsNotFound_WhenUserDoesNotExist test");
                
                var client = _factory.GetAuthenticatedClient();
                var userId = $"nonexistent-{Guid.NewGuid()}";
                
                _logger.LogInfo($"Testing with non-existent user ID: {userId}");
                
                var response = await client.GetAsync($"/api/v1/users/{userId}");
                
                _logger.LogInfo($"Received response status: {response.StatusCode}");
                
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        [Fact]
        public async Task GetUsers_ReturnsUnauthorized_WithoutToken()
        {
            try
            {
                _logger.LogInfo("Starting GetUsers_ReturnsUnauthorized_WithoutToken test");
                
                var client = _factory.CreateClient();
                var response = await client.GetAsync("/api/v1/users");
                
                _logger.LogInfo($"Received response status: {response.StatusCode}");
                
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                
                _logger.LogInfo("Test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Test failed", ex);
                throw;
            }
        }

        public void Dispose()
        {
            _logger.LogInfo("Test cleanup");
            _client.Dispose();
        }
    }
    }
