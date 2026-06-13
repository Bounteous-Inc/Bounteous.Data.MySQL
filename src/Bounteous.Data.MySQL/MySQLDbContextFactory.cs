using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.MySQL;

public abstract class MySqlDbContextFactory<T, TUserId>(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<TUserId> identityProvider)
    : DbContextFactory<T, TUserId>(connectionBuilder, observer, identityProvider)
    where T : IDbContext<TUserId>
    where TUserId : struct
{
    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => new DbContextOptionsBuilder<DbContextBase<TUserId>>()
            .UseMySQL(ConnectionBuilder.AdminConnectionString,
                mySqlOptions => { mySqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
}
