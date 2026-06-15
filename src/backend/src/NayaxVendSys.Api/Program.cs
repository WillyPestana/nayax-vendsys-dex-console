using NayaxVendSys.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app
    .UseApiPipeline()
    .MapApiEndpoints();

await app.RunAsync();

public partial class Program
{
}
