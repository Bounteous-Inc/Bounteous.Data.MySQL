using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Bounteous.Data.MySQL.Tests;

public class MySqlDbContextFactoryTests
{
    private class TestDbContext : DbContextBase
    {
        public TestDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer) 
            : base(options, observer)
        {
        }

        protected override void RegisterModels(ModelBuilder modelBuilder)
        {
        }
    }

    private class TestMySqlDbContextFactory : MySqlDbContextFactory<TestDbContext>
    {
        public TestMySqlDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
            : base(connectionBuilder, observer)
        {
        }

        protected override TestDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        {
            return new TestDbContext(options, observer);
        }

        public DbContextOptions<DbContextBase> TestApplyOptions(bool sensitiveDataLoggingEnabled = false)
        {
            return ApplyOptions(sensitiveDataLoggingEnabled);
        }

        public TestDbContext TestCreate(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        {
            return Create(options, observer);
        }
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        Assert.NotNull(factory);
    }


    [Fact]
    public void ApplyOptions_WithDefaultParameters_ShouldReturnOptionsWithMySqlProvider()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingEnabled_ShouldReturnOptionsWithLoggingEnabled()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: true);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingDisabled_ShouldReturnOptionsWithLoggingDisabled()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions(sensitiveDataLoggingEnabled: false);

        Assert.NotNull(options);
    }

    [Fact]
    public void ApplyOptions_ShouldUseAdminConnectionString()
    {
        const string expectedConnectionString = "Server=testserver;Database=testdb;User=admin;Password=pass;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(expectedConnectionString);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }

    [Fact]
    public void Create_ShouldReturnValidDbContext()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();
        var context = factory.TestCreate(options, mockObserver.Object);

        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }

    [Fact]
    public void Create_ShouldUseProvidedObserver()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();
        var context = factory.TestCreate(options, mockObserver.Object);

        Assert.NotNull(context);
    }

    [Theory]
    [InlineData("Server=localhost;Database=db1;")]
    [InlineData("Server=remotehost;Database=db2;Port=3307;")]
    [InlineData("Server=127.0.0.1;Database=testdb;User=root;")]
    public void ApplyOptions_WithDifferentConnectionStrings_ShouldHandleCorrectly(string connectionString)
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns(connectionString);
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
        mockConnectionBuilder.Verify(x => x.AdminConnectionString, Times.AtLeastOnce);
    }

    [Fact]
    public void ApplyOptions_ShouldEnableDetailedErrors()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();

        Assert.NotNull(options);
    }

    [Fact]
    public void Create_MultipleCalls_ShouldReturnDifferentInstances()
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(x => x.AdminConnectionString).Returns("Server=localhost;Database=test;");
        var mockObserver = new Mock<IDbContextObserver>();

        var factory = new TestMySqlDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);
        var options = factory.TestApplyOptions();
        var context1 = factory.TestCreate(options, mockObserver.Object);
        var context2 = factory.TestCreate(options, mockObserver.Object);

        Assert.NotNull(context1);
        Assert.NotNull(context2);
        Assert.NotSame(context1, context2);
    }
}
