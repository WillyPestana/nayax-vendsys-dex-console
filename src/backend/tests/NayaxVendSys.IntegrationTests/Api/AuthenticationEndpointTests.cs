using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace NayaxVendSys.IntegrationTests.Api;

public sealed class AuthenticationEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Authenticate_ReturnsAuthorizationHeader_ForValidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/authenticate", new
        {
            username = "vendsys",
            password = "NFsZGmHAGWJSZ#RuvdiV"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.Authenticated.Should().BeTrue();
        body.AuthorizationHeader.Should().StartWith("Basic ");
    }

    [Fact]
    public async Task Authenticate_ReturnsUnauthorized_ForInvalidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/authenticate", new
        {
            username = "vendsys",
            password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record AuthResponse(bool Authenticated, string AuthorizationHeader, string Username);
}
