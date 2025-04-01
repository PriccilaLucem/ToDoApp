using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using WebApplication.Src.Config;
using WebApplication.Src.Dto.user;
using WebApplication.Src.Dto.User;
using WebApplication.Src.Interface;
using WebApplication.Src.Models;
using Xunit;

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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var userViewMock = new Mock<IUserView>();
                userViewMock.Setup(x => x.CreateUsers(It.IsAny<UserModel>()))
                          .ReturnsAsync("generated-id");
                
                services.AddScoped<IUserView>(_ => userViewMock.Object);
                services.AddSingleton(_testAuthSettings);
            });
        }

        public HttpClient GetAuthenticatedClient(
            string userId = "test-user", 
            string[] roles = null, 
            string email = "test@example.com")
        {
            var client = CreateClient();
            
            var token = JwtTokenHelper.GenerateTestToken(
                _testAuthSettings.SecretKey,
                _testAuthSettings.Issuer,
                _testAuthSettings.Audience,
                userId,
                roles ?? Array.Empty<string>(),
                email);

            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }

    public static class JwtTokenHelper
    {
        public static string GenerateTestToken(
            string secretKey, 
            string issuer, 
            string audience, 
            string userId,
            string[] roles,
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

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims = claims.Append(new Claim(ClaimTypes.Role, role)).ToArray();
                }
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UserControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.GetAuthenticatedClient();
        }

        [Fact]
        public async Task PostUser_ReturnsCreated_WithValidToken()
        {
            var userDto = new CreateUserDTO
            {
                Name = "Test User",
                Email = $"test-{Guid.NewGuid()}@example.com",
                BirthDate = DateTime.UtcNow,
                Password = "ValidPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/users", userDto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var client = _factory.GetAuthenticatedClient();

            var response = await client.GetAsync($"/api/v1/users/nonexistent-{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        [Fact]
        public async Task GetUsers_ReturnsUnauthorized_WithoutToken()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/users");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}