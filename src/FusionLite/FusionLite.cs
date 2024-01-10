using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FusionLite;

public sealed class FusionLite : IDistributedCache
{
    private readonly ILogger logger;
    private readonly FusionLiteCommandManager commandManager;

    public FusionLite(FusionLiteOptions options, ILogger<FusionLite>? logger = null)
    {
        this.logger = logger ?? NullLogger<FusionLite>.Instance;
        commandManager = new FusionLiteCommandManager(options.ConnectionString, this.logger);

        commandManager.ExecuteCommand(Operation.Initialize, command=> command.ExecuteNonQuery());
    }

    public byte[]? Get(string key)
    {
        return commandManager.ExecuteCommand(
            Operation.Get,
            command =>
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.Ticks);
                return command.ExecuteScalar() as byte[];
            }
        );
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        return await commandManager.ExecuteCommand(
            Operation.Get,
            async command =>
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@now", DateTimeOffset.UtcNow.Ticks);
                return await command.ExecuteScalarAsync(token) as byte[];
            }
        );
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ExecuteNonQuery(Operation.Insert, command => AddSetParameters(command, key, value, options));
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return ExecuteNonQueryAsync(Operation.Insert, command => AddSetParameters(command, key, value, options), token);
    }

    public void Refresh(string key)
    {
        UnusedMethodException.Throw();
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        UnusedMethodException.Throw();
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        ExecuteNonQuery(Operation.Remove, command => command.Parameters.AddWithValue("@key", key));
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        return ExecuteNonQueryAsync(Operation.Remove, command => command.Parameters.AddWithValue("@key", key), token);
    }

    private void ExecuteNonQuery(Operation operation, Action<SqliteCommand> configureCommand)
    {
        commandManager.ExecuteCommand(operation, command =>
        {
            configureCommand(command);
            command.ExecuteNonQuery();
        });
    }

    private async Task ExecuteNonQueryAsync(Operation operation, Action<SqliteCommand> configureCommand, CancellationToken token = default)
    {
        await commandManager.ExecuteCommand(operation, async command =>
        {
            configureCommand(command);
            await command.ExecuteNonQueryAsync(token);
        });
    }
    
    private void AddSetParameters(SqliteCommand command, string key, byte[] value, DistributedCacheEntryOptions options)
    {
        command.Parameters.AddWithValue("@key", key);
        command.Parameters.AddWithValue("@value", value);
        AddExpirationParameters(command, options);
    }

    private void AddExpirationParameters(SqliteCommand command, DistributedCacheEntryOptions options)
    {
        var absoluteExpiration = options.AbsoluteExpirationRelativeToNow.HasValue
            ? DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value).Ticks
            : options.AbsoluteExpiration?.Ticks;

        command.Parameters.AddWithValue("@createdAt", DateTimeOffset.UtcNow.Ticks);
        command.Parameters.AddWithValue("@absoluteExpiration", absoluteExpiration ?? (object)DBNull.Value);
    }
}