using Npgsql;

namespace Notite.Api;

public class NotesRepository(NpgsqlDataSource dataSource)
{
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _schemaReady;

    // Tabela e creata la prima utilizare, nu la pornire, ca /health sa nu depinda de DB.
    private async Task EnsureSchemaAsync(CancellationToken ct)
    {
        if (_schemaReady) return;
        await _schemaLock.WaitAsync(ct);
        try
        {
            if (_schemaReady) return;
            await using var cmd = dataSource.CreateCommand("""
                CREATE TABLE IF NOT EXISTS note (
                    id   SERIAL PRIMARY KEY,
                    text TEXT NOT NULL
                )
                """);
            await cmd.ExecuteNonQueryAsync(ct);
            _schemaReady = true;
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    public async Task<IReadOnlyList<Note>> GetAllAsync(CancellationToken ct)
    {
        await EnsureSchemaAsync(ct);
        await using var cmd = dataSource.CreateCommand("SELECT id, text FROM note ORDER BY id");
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var notes = new List<Note>();
        while (await reader.ReadAsync(ct))
            notes.Add(new Note(reader.GetInt32(0), reader.GetString(1)));
        return notes;
    }

    public async Task<int> CreateAsync(string text, CancellationToken ct)
    {
        await EnsureSchemaAsync(ct);
        await using var cmd = dataSource.CreateCommand("INSERT INTO note (text) VALUES ($1) RETURNING id");
        cmd.Parameters.AddWithValue(text);
        return (int)(await cmd.ExecuteScalarAsync(ct))!;
    }
}
