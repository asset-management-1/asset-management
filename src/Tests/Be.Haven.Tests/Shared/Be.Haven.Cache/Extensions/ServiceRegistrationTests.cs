using System.Reflection;
using static Be.Haven.Shared.Constants.RedisConstants;
using CacheServiceRegistration = Be.Haven.Cache.Extensions.ServiceRegistration;

namespace Be.Haven.Tests.Shared.Be.Haven.Cache.Extensions;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddDistributedCache_Should_RegisterInMemoryCacheServices_When_MemoryModeIsEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{CACHE_SETTING}:IsMemory"] = "true",
            [$"{CACHE_SETTING}:AbsoluteExpiration"] = "10",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:MaxFailedAttempts"] = "5"
        });

        // Act
        CacheServiceRegistration.AddDistributedCache(services, configuration);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICachingService) &&
            descriptor.ImplementationType == typeof(CachingService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICacheBypassService) &&
            descriptor.ImplementationType == typeof(InMemoryCacheBypassService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ILoginAttemptService) &&
            descriptor.ImplementationType == typeof(LoginMemoryAttemptService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICacheVersionService) &&
            descriptor.ImplementationType == typeof(InMemoryCacheVersionService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IDistributedCache));
    }

    [Fact]
    public void AddDistributedCache_Should_BindCacheAndLoginAttemptOptions_When_ConfigIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{CACHE_SETTING}:IsMemory"] = "true",
            [$"{CACHE_SETTING}:AbsoluteExpiration"] = "30",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:MaxFailedAttempts"] = "9",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:FailedWindow"] = "00:07:00",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:LockDuration"] = "00:11:00"
        });

        // Act
        CacheServiceRegistration.AddDistributedCache(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        var cacheOptions = provider.GetRequiredService<IOptions<CacheOptions>>().Value;
        var loginOptions = provider.GetRequiredService<IOptions<LoginAttemptOptions>>().Value;
        cacheOptions.IsMemory.Should().BeTrue();
        cacheOptions.AbsoluteExpiration.Should().Be(30);
        loginOptions.MaxFailedAttempts.Should().Be(9);
        loginOptions.FailedWindow.Should().Be(TimeSpan.FromMinutes(7));
        loginOptions.LockDuration.Should().Be(TimeSpan.FromMinutes(11));
    }

    [Fact]
    public void AddDistributedCache_Should_ThrowInvalidOperationException_When_CacheSectionIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>());

        // Act
        var act = () => CacheServiceRegistration.AddDistributedCache(services, configuration);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddDistributedCache_Should_ThrowArgumentException_When_RedisConnectionCannotBeConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{CACHE_SETTING}:IsMemory"] = "false",
            [$"{CACHE_SETTING}:AbsoluteExpiration"] = "10",
            [$"{CACHE_SETTING}:RedisSettings:Host"] = "",
            [$"{CACHE_SETTING}:RedisSettings:Port"] = "6379",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:MaxFailedAttempts"] = "5"
        });

        // Act
        var act = () => CacheServiceRegistration.AddDistributedCache(services, configuration);

        // Assert
        act.Should().Throw<ArgumentException>();
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICachingService) &&
            descriptor.ImplementationType == typeof(CachingService));
    }

    [Fact]
    public void AddDistributedCache_Should_RegisterRedisServices_When_RedisConnectionIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildRedisConfiguration();
        var redis = new Mock<IConnectionMultiplexer>();
        redis.SetupGet(x => x.IsConnected).Returns(true);

        // Act
        InvokeAddDistributedCache(
            services,
            configuration,
            _ => redis.Object);
        using var provider = services.BuildServiceProvider();

        // Assert
        var redisOptions = provider
            .GetRequiredService<IOptions<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions>>()
            .Value;
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IConnectionMultiplexer) &&
            descriptor.ImplementationInstance == redis.Object);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICacheVersionService) &&
            descriptor.ImplementationType == typeof(DistributedCacheVersionService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICacheBypassService) &&
            descriptor.ImplementationType == typeof(DistributedCacheBypassService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ILoginAttemptService) &&
            descriptor.ImplementationType == typeof(LoginAttemptService));
        redisOptions.ConfigurationOptions.Should().NotBeNull();
        redisOptions.ConfigurationOptions.EndPoints.Should().ContainSingle();
    }

    [Fact]
    public void AddDistributedCache_Should_RegisterRedisServices_When_RedisConnectionIsNotConnected()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildRedisConfiguration();
        var redis = new Mock<IConnectionMultiplexer>();
        redis.SetupGet(x => x.IsConnected).Returns(false);

        // Act
        InvokeAddDistributedCache(
            services,
            configuration,
            _ => redis.Object);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IConnectionMultiplexer) &&
            descriptor.ImplementationInstance == redis.Object);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static IConfiguration BuildRedisConfiguration() =>
        BuildConfiguration(new Dictionary<string, string>
        {
            [$"{CACHE_SETTING}:IsMemory"] = "false",
            [$"{CACHE_SETTING}:AbsoluteExpiration"] = "10",
            [$"{CACHE_SETTING}:RedisSettings:Host"] = "localhost",
            [$"{CACHE_SETTING}:RedisSettings:Port"] = "6379",
            [$"{CACHE_SETTING}:RedisSettings:DbNumber"] = "0",
            [$"{CACHE_SETTING}:RedisSettings:ConnectRetry"] = "1",
            [$"{CACHE_SETTING}:RedisSettings:ConnectTimeout"] = "100",
            [$"{CACHE_SETTING}:RedisSettings:SyncTimeout"] = "100",
            [$"{LOGIN_AT_TEMPT_SETTINGS}:MaxFailedAttempts"] = "5"
        });

    private static void InvokeAddDistributedCache(
        IServiceCollection services,
        IConfiguration configuration,
        Func<ConfigurationOptions, IConnectionMultiplexer> connectRedis)
    {
        var method = typeof(CacheServiceRegistration)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(x => x.Name == nameof(CacheServiceRegistration.AddDistributedCache)
                         && x.GetParameters().Length == 3);

        method.Invoke(null, [services, configuration, connectRedis]);
    }
}
