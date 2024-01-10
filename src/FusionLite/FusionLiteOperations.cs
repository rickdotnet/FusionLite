namespace FusionLite;

public enum Operation
{
    Initialize,
    Insert,
    Remove,
    Get
}

public class FusionLiteOperations
{
    private readonly Dictionary<Operation, string> commands = new()
    {
        [Operation.Initialize] =
            """
            CREATE TABLE IF NOT EXISTS "cache" (
                "key" TEXT NOT NULL,
                "value" BLOB,
                "createdAt" INTEGER,
                "absoluteExpiration" INTEGER,
                PRIMARY KEY("key")
            ) WITHOUT ROWID;

            CREATE INDEX IF NOT EXISTS "cache_absoluteExpiration" ON "cache" ("absoluteExpiration");
            """,

        [Operation.Insert] =
            "INSERT OR REPLACE INTO cache (key, value, createdAt, absoluteExpiration) VALUES (@key, @value, @createdAt, @absoluteExpiration)",

        [Operation.Get] =
            "SELECT value FROM cache WHERE key = @key AND (absoluteExpiration IS NULL OR absoluteExpiration >= @now);",

        [Operation.Remove] = 
            "DELETE FROM cache WHERE key = @key",
    };

    public string GetCommandText(Operation operation)
    {
        if (!commands.TryGetValue(operation, out var commandText))
        {
            throw new InvalidOperationException($"No command text found for operation {operation}.");
        }

        return commandText;
    }
}