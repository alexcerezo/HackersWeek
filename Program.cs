using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        var codespaceName = Environment.GetEnvironmentVariable("CODESPACE_NAME");
        var domain = Environment.GetEnvironmentVariable("CODESPACES_PORT_FORWARDING_DOMAIN")
                     ?? "app.github.dev";

        if (!string.IsNullOrEmpty(codespaceName))
        {
            var port = 5163;
            options.AddServer($"https://{codespaceName}-{port}.{domain}");
        }
    });
// }


app.MapGet("/", () => "URL SHORTENER");

app.Run();
