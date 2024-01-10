using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace FusionLite;

internal class FusionLiteCommandManager
{
    private readonly string connectionString;
    private readonly FusionLiteOperations dbCommands;
    private readonly ILogger logger;

    public FusionLiteCommandManager(string connectionString, ILogger logger)
    {
        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.dbCommands = new FusionLiteOperations();
    }

    public void ExecuteCommand(Operation operation, Action<SqliteCommand> execute)
    {
        ExecuteCommandInternal(operation, command =>
        {
            execute(command);
            return true; // return value is not used.
        });
    }

    public T ExecuteCommand<T>(Operation operation, Func<SqliteCommand, T> execute)
    {
        return ExecuteCommandInternal(operation, execute);
    }

    private T ExecuteCommandInternal<T>(Operation operation, Func<SqliteCommand, T> execute)
    {
        using var connection = new SqliteConnection(connectionString);
        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = dbCommands.GetCommandText(operation);
            return execute(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute SQLite command for operation {Operation}.", operation);
            throw;
        }
    }
    
}