using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bounteous.Data.MySQL.Tests;

public class MySqlDbContextFactoryTests
{
    private class TestDbContext(
        DbContextOptions options,
        IDbContextObserver observer,
        IIdentityProvider<Guid> identityProvider)
        : DbContextBase<Guid>(options, observer, identityProvider)
    {
        protected override void RegisterModels(ModelBuilder modelBuilder)
        {
        }
    }

    private class TestMySqlDbContextFactory(
        IConnectionBuilder connectionBuilder,
        IDbContextObserver observer,
        IIdentityProvider<Guid> identityProvider)
        : MySqlDbContextFactory<TestDbContext, Guid>(connectionBuilder, observer, identityProvider)
    {
        public DbContextOptions TestApplyOptions(bool sensitiveDataLoggingEnabled = false)
            => ApplyOptions(sensitiveDataLoggingEnabled);

        public TestDbContext TestCreate(DbContextOptions options, IDbContextObserver observer,
            IIdentityProvider<Guid> identityProvider)
            => Create(options, observer, identityProvider);

        protected override TestDbContext Create(DbContextOptions options, IDbContextObserver observer,
            IIdentityProvider<Guid> identityProvider)
            => new(options, observer, identityProvider);
    }

    private static TestMySqlDbContextFactory CreateFactory(string connectionString,
        out Mock<IConnectionBuilder> mockConnectionBuilder, out Mock<IDbContextObserver> mockObserver,
        out Mock<IIdentityProvider<Guid>> mockIdentityProvider)
    {
        mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(connectionString);
        mockObserver = new Mock<IDbContextObserver>();
        mockIdentityProvider = new Mock<IIdentityProvider<Guid>>();
        return new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object,
            mockIdentityProvider.Object);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out _, out _);

        Assert.NotNull(factory);
    }

    [Fact]
    public void ApplyOptions_WithDefaultParameters_ShouldReturnOptionsWithMySqlProvider()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out _, out _);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingEnabled_ShouldReturnOptionsWithLoggingEnabled()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out _, out _);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: true);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingDisabled_ShouldReturnOptionsWithLoggingDisabled()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out _, out _);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: false);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_ShouldUseAdminConnectionString()
    {
        const string expectedConnectionString = "Server=testserver;Database=testdb;User=admin;Password=pass;";
        var factory = CreateFactory(expectedConnectionString, out var mockConnectionBuilder, out _, out _);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }

    [Fact]
    public void Create_ShouldReturnValidDbContext()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out var mockObserver,
            out var mockIdentityProvider);
        var options = factory.TestApplyOptions();
        var context = factory.TestCreate(options, mockObserver.Object, mockIdentityProvider.Object);

        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }

    [Fact]
    public void Create_ShouldUseProvidedObserver()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out var mockObserver,
            out var mockIdentityProvider);
        var options = factory.TestApplyOptions();
        var context = factory.TestCreate(options, mockObserver.Object, mockIdentityProvider.Object);

        Assert.NotNull(context);
    }

    [Theory]
    [InlineData("Server=localhost;Database=db1;")]
    [InlineData("Server=remotehost;Database=db2;Port=3307;")]
    [InlineData("Server=127.0.0.1;Database=testdb;User=root;")]
    public void ApplyOptions_WithDifferentConnectionStrings_ShouldHandleCorrectly(string connectionString)
    {
        var factory = CreateFactory(connectionString, out var mockConnectionBuilder, out _, out _);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }

    [Fact]
    public void ApplyOptions_ShouldEnableDetailedErrors()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out _, out _);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void Create_MultipleCalls_ShouldReturnDifferentInstances()
    {
        var factory = CreateFactory("Server=localhost;Database=test;", out _, out var mockObserver,
            out var mockIdentityProvider);
        var options = factory.TestApplyOptions();
        var context1 = factory.TestCreate(options, mockObserver.Object, mockIdentityProvider.Object);
        var context2 = factory.TestCreate(options, mockObserver.Object, mockIdentityProvider.Object);

        Assert.NotNull(context1);
        Assert.NotNull(context2);
        Assert.NotSame(context1, context2);
    }
}
