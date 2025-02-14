using Microsoft.EntityFrameworkCore;

namespace Xerris.DotNet.Data.MySql;

public abstract class MySqlDbContextFactory<T> : DbContextFactory<T> where T : DbContext
{
    public MySqlDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer)
    {
    }

    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        return new DbContextOptionsBuilder<DbContextBase>().UseMySQL(ConnectionBuilder.AdminConnectionString,
                mySqlOptions => { mySqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}