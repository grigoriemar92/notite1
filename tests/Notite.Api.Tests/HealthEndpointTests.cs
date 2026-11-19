using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Notite.Api.Tests;

// Ruleaza aplicatia in memorie; /health nu acceseaza baza de date, deci nu e nevoie de PostgreSQL.
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_ReturnsOkWithDefaultVersion()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("ok", body?.Status);
        Assert.Equal("1.0", body?.Versiune);
    }

    [Fact]
    public async Task Health_UsesVersiuneFromConfiguration()
    {
        var client = factory
            .WithWebHostBuilder(b => b.UseSetting("VERSIUNE", "2.3"))
            .CreateClient();

        var body = await client.GetFromJsonAsync<HealthResponse>("/health");

        Assert.Equal("2.3", body?.Versiune);
    }

    private record HealthResponse(string Status, string Versiune);
}
