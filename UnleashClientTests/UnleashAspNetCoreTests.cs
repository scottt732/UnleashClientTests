using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using UnleashClientTests.AspNetCore;
using UnleashClientTests.AspNetCore.Testing;
using Xunit;

namespace UnleashClientTests
{
    public class UnleashAspNetCoreTests : IClassFixture<TestWebApplicationFactory<Startup>>
    {
        private WebApplicationFactory<Startup> Factory { get; }

        public UnleashAspNetCoreTests(TestWebApplicationFactory<Startup> factory)
        {
            Factory = factory;
        }

        [Fact]
        public async Task TestControllerTestRequest_ShouldExecute_ButDiesWithDIIssue()
        {
            var client = Factory.CreateClient();
            var response = await client.GetAsync("Test");

            // I don't really have anything to test here, but this test won't/can't pass anyway b/c of DI issues.
            // The controller can't be constructed
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task TestControllerPropertiesEndopoint_ShouldReturnMiddlewareAndActionFilterAttributes()
        {
            var client = Factory.CreateClient();
            var response = await client.GetAsync("Keys");

            // This test will also fail due to DI issues... but if it passed, I would expect to see the 2 keys from the
            // middleware and the MVC ActionFilter key.  They'd be writing to the same Dictionary<string, string> in the
            // ScopedDictionaryContextProvider b/c that provider was registered using AddScoped<> and ASP.NET Core
            // creates a scope at the beginning of the HTTP request (very early in the request processing).

            // The controller can't be constructed
            response.EnsureSuccessStatusCode();

            var dict = await response.Content.ReadAsAsync<Dictionary<string, string>>();

            Assert.True(dict.ContainsKey("Method"));
            Assert.True(dict.ContainsKey("Path"));
            Assert.True(dict.ContainsKey("ActionDisplayName"));
        }
    }
}
