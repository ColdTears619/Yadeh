using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Yadeh.Infrastructure.Persistence;

namespace Yadeh.Infrastructure.Tests.Persistence;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public YadehDbContext Context { get; }

    private SqliteTestDatabase(
        SqliteConnection connection,
        YadehDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<YadehDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new YadehDbContext(options);

        try
        {
            await context.Database.EnsureCreatedAsync();

            return new SqliteTestDatabase(
                connection,
                context);
        }
        catch
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}