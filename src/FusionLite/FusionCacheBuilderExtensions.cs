using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace FusionLite;

public static class FusionCacheBuilderExtensions
{
    public static IFusionCacheBuilder WithFusionLite(this IFusionCacheBuilder builder, IServiceCollection services, Action<FusionLiteOptions>? setupAction = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();

        if (setupAction == null)
            setupAction = FusionLiteOptions.DefaultAction;

        services.Configure(setupAction);
        services.AddTransient(x => x.GetRequiredService<IOptions<FusionLiteOptions>>().Value);
        services.AddSingleton<IDistributedCache, FusionLite>();

        var sqliteOptions = new FusionLiteOptions();
        setupAction(sqliteOptions);

        builder.WithRegisteredDistributedCache();
        builder.WithSystemTextJsonSerializer();
        return builder;
    }
}