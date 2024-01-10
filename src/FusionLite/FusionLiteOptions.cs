using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace FusionLite;

public record FusionLiteOptions : IOptions<FusionLiteOptions>
{
    public static FusionLiteOptions Default { get; } = new();
    public static Action<FusionLiteOptions> DefaultAction { get; } = options =>
    {
        options.DefaultTimeout = Default.DefaultTimeout;
        options.MemoryOnly = Default.MemoryOnly;
        options.UsePooling = Default.UsePooling;
        options.CachePath = Default.CachePath;
    };
    
    FusionLiteOptions IOptions<FusionLiteOptions>.Value => this;

    public bool MemoryOnly { get; set; }
    public bool UsePooling { get; set; } = true;
    public int DefaultTimeout { get; set; }

    private string cachePath = "FusionLiteCache.db";

    public string CachePath
    {
        get => cachePath;
        set
        {
            if (value.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Replace("Data Source=", "");
            }

            if (value.Contains('=') || value.Contains('"'))
            {
                throw new ArgumentException("CachePath must be a path and not a connection string!");
            }

            cachePath = value.Trim();
        }
    }

    internal string ConnectionString
    {
        get
        {
            var builder = new SqliteConnectionStringBuilder
            {
                BrowsableConnectionString = false,
                ConnectionString = null,
                DataSource = MemoryOnly
                    ? ":memory:"
                    : CachePath,
                Mode = MemoryOnly
                    ? SqliteOpenMode.Memory
                    : SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                Pooling = UsePooling,
            };

            if (DefaultTimeout > 0)
            {
                builder.DefaultTimeout = DefaultTimeout;
            }

            return builder.ConnectionString;
        }
    }
}