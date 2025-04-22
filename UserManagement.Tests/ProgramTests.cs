
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UserManagement.Services;
using Xunit;

namespace UserManagement.Tests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ENVIRONMENT", "Development");

            });
        }

        [Fact]
        public async Task App_StartsSuccessfully_AndResponds()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/index.html");

            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Redirect
            );
        }

        [Fact]
        public void IJwtService_ShouldBeRegisteredIn_DI()
        {
            using var scope = _factory.Services.CreateScope();
            var jwtService = scope.ServiceProvider.GetService<IJwtService>();
            Assert.NotNull(jwtService);
        }

        [Fact]
        public async Task ProtectedEndpoint_Returns401_WhenNoToken()
        {
            var client = _factory.CreateClient();

            var payload = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.PutAsync("/api/User/d4a7b3a1-2b6b-4372-9f25-123456789abc", payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
