using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SATS.AI.Documents;

namespace SATS.AI.Extensions;

public static class AIServiceProviderExtensions
{
    public static IServiceProvider ApplyPendingMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<DocumentDbContext>().Database.Migrate();
        return serviceProvider;
    }
}
