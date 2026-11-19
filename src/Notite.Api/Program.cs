using Notite.Api;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Notes")
    ?? "Host=localhost;Port=5432;Database=notite;Username=postgres";

// NpgsqlDataSource nu deschide conexiuni la creare, deci aplicatia porneste si fara baza de date.
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
builder.Services.AddSingleton<NotesRepository>();

var app = builder.Build();

// Variabilele de mediu fac parte din configuratie, deci VERSIUNE se citeste de aici.
var versiune = app.Configuration["VERSIUNE"] is { Length: > 0 } v ? v : "1.0";

// 1. Health check - nu atinge baza de date.
app.MapGet("/health", () => Results.Ok(new { status = "ok", versiune }));

// 2. Readiness - verifica daca PostgreSQL e accesibil.
app.MapGet("/ready", async (NpgsqlDataSource dataSource, CancellationToken ct) =>
{
    try
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception ex) when (ex is NpgsqlException or System.Net.Sockets.SocketException or TimeoutException)
    {
        return Results.Json(
            new { status = "indisponibil", mesaj = $"Baza de date nu este accesibila: {ex.Message}" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

// 3. Toate notitele din tabelul note.
app.MapGet("/note", async (NotesRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.GetAllAsync(ct)));

// 4. Creare notita: 400 pentru text gol, altfel 201 cu id-ul nou.
app.MapPost("/note", async (CreateNoteRequest request, NotesRepository repo, CancellationToken ct) =>
{
    var validation = NoteValidator.Validate(request.Text);
    if (!validation.IsValid)
        return Results.BadRequest(new { eroare = validation.Error });

    var id = await repo.CreateAsync(request.Text!.Trim(), ct);
    return Results.Created($"/note/{id}", new { id });
});

app.Run();

public record CreateNoteRequest(string? Text);

// Expus pentru WebApplicationFactory<Program> din teste.
public partial class Program;
