using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace NayaxVendSys.IntegrationTests.Api;

public sealed class DexEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetDex_ReturnsUnauthorized_WithoutBasicAuth()
    {
        var response = await _client.GetAsync("/dex");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDex_ReturnsMeters_WithBasicAuth()
    {
        _client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();

        var response = await _client.GetAsync("/dex");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("100077238");
    }

    [Fact]
    public async Task UploadDex_ParsesFileAndReturnsCreated_WithBasicAuth()
    {
        _client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();
        await using var sampleStream = File.OpenRead(FindSample("dex-machine-a.txt"));
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(sampleStream), "file", "machine-a.txt");

        var response = await _client.PostAsync("/dex", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("dexMeterId").GetInt32().Should().Be(42);
        document.RootElement.GetProperty("machineId").GetString().Should().Be("100077238");
        document.RootElement.GetProperty("laneCount").GetInt32().Should().Be(38);
    }

    [Fact]
    public async Task UploadDex_ReturnsBadRequest_ForUnsupportedExtension()
    {
        _client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();
        await using var sampleStream = File.OpenRead(FindSample("dex-machine-a.txt"));
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(sampleStream), "file", "machine-a.pdf");

        var response = await _client.PostAsync("/dex", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDex_ReturnsNoContent_WithBasicAuth()
    {
        _client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader();

        var response = await _client.DeleteAsync("/dex");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader()
    {
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes("vendsys:NFsZGmHAGWJSZ#RuvdiV"));
        return new AuthenticationHeaderValue("Basic", token);
    }

    private static string FindSample(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "samples", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find sample DEX file {fileName}.");
    }
}
