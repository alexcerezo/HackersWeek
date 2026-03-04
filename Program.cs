using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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
}

var urls = new Dictionary<string, UrlEntry>();


app.MapGet("/", () => "URL SHORTENER");

app.MapPost("/urls", (CreateUrlRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
        return Results.BadRequest(new { error = "La URL no puede estar vacía" });

    var code = Guid.NewGuid().ToString("N")[..6];

    var entry = new UrlEntry(code, request.Url, DateTime.UtcNow);
    urls[code] = entry;

    return Results.Created($"/urls/{code}", entry);
})
.WithName("CrearUrlCorta")
.WithSummary("Crea un código corto para una URL larga");


app.MapGet("/urls", () =>
{
    return Results.Ok(urls.Values);
})
.WithName("ListarUrls")
.WithSummary("Lista todas las URLs registradas");


app.MapGet("/urls/{code}", (string code) =>
{
    return urls.TryGetValue(code, out var entry)
        ? Results.Ok(entry)
        : Results.NotFound(new { error = $"No existe ninguna URL con el código '{code}'" });
})
.WithName("ObtenerUrl")
.WithSummary("Obtiene los detalles de una URL por su código");


app.MapDelete("/urls/{code}", (string code) =>
{
    return urls.Remove(code)
        ? Results.NoContent()
        : Results.NotFound(new { error = $"No existe ninguna URL con el código '{code}'" });
})
.WithName("EliminarUrl")
.WithSummary("Elimina una URL del sistema");


app.MapGet("/{code}", (string code) =>
{
    return urls.TryGetValue(code, out var entry)
        ? Results.Redirect(entry.OriginalUrl)
        : Results.NotFound(new { error = $"El código '{code}' no existe o ha expirado" });
})
.WithName("Redirigir")
.WithSummary("Redirige al usuario a la URL original");


app.Run();


record UrlEntry(string Code, string OriginalUrl, DateTime CreatedAt);

record CreateUrlRequest(string Url);
