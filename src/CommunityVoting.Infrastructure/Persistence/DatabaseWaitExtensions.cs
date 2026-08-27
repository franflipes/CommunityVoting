using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Infrastructure.Persistence;

public static class DatabaseWaitExtensions
{
    public static async Task WaitForDatabaseAsync(this DbContext context, ILogger? logger = null, int maxRetries = 20, TimeSpan? delay = null)
    {
        var retryDelay = delay ?? TimeSpan.FromSeconds(2);
        var dbCreator = context.Database.GetService<IRelationalDatabaseCreator>();

        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                // Check if the database server is reachable and if the database exists
                bool exists = await dbCreator.ExistsAsync();
                if (!exists)
                {
                    logger?.LogInformation("[Database] PostgreSQL server is up, but target database does not exist. Creating database...");
                    try
                    {
                        await dbCreator.CreateAsync();
                        logger?.LogInformation("[Database] Target database created successfully.");
                    }
                    catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P04" || ex.Message.Contains("already exists"))
                    {
                        // 42P04 = duplicate_database (another process/API created it concurrently)
                        logger?.LogInformation("[Database] Target database was created by a concurrent process.");
                    }
                }
                else
                {
                    logger?.LogInformation("[Database] PostgreSQL database connection established successfully.");
                }
                return;
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogWarning(ex, "[Database] Attempt {Attempt}/{MaxRetries}: PostgreSQL server is not ready yet. Retrying in {Delay}s...", i, maxRetries, retryDelay.TotalSeconds);
                }
                else
                {
                    Console.WriteLine($"[Database] Attempt {i}/{maxRetries}: PostgreSQL server is not ready yet. Retrying in {retryDelay.TotalSeconds}s... Error: {ex.Message}");
                }
            }
            await Task.Delay(retryDelay);
        }

        throw new InvalidOperationException($"PostgreSQL database failed to become ready after {maxRetries} attempts.");
    }
}
