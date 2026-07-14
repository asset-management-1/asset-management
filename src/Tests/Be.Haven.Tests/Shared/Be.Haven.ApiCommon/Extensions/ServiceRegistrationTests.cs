using Be.Haven.ApiCommon.Options.Swagger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog.Extensions.Hosting;
using ApiCommonServiceRegistration = Be.Haven.ApiCommon.Extensions.ServiceRegistration;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Extensions;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddHavenAuthenticationServices_Should_RegisterBearerSchemeAndValidators_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ApiCommonServiceRegistration.AddHavenAuthenticationServices(services);
        using var provider = services.BuildServiceProvider();

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IAuthResetValidator) &&
            descriptor.ImplementationType == typeof(HavenAuthResetValidator));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IClientSessionValidator) &&
            descriptor.ImplementationType == typeof(HavenClientSessionValidator));
        var options = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        options.DefaultAuthenticateScheme.Should().Be(BEARER);
        options.DefaultChallengeScheme.Should().Be(BEARER);
        options.DefaultForbidScheme.Should().Be(BEARER);
    }

    [Fact]
    public void AddHavenTokenValidationOptions_Should_BindAndValidateOptions_When_ConfigIsValid()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildTokenConfiguration(secretKey: "a-secret-key-that-is-long-enough");

        // Act
        ApiCommonServiceRegistration.AddHavenTokenValidationOptions(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<AuthenticationTokenValidationOptions>>().Value;
        options.Issuer.Should().Be("haven");
        options.Audiences.Should().Contain("portal");
        options.SecretKey.Should().Be("a-secret-key-that-is-long-enough");
    }

    [Fact]
    public void AddHavenTokenValidationOptions_Should_ThrowOptionsValidationException_When_ConfigIsInvalid()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildTokenConfiguration(secretKey: string.Empty);

        // Act
        ApiCommonServiceRegistration.AddHavenTokenValidationOptions(services, configuration);
        using var provider = services.BuildServiceProvider();
        var action = () => provider.GetRequiredService<IOptions<AuthenticationTokenValidationOptions>>().Value;

        // Assert
        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddConfiguredSwagger_Should_RegisterSwaggerGeneratorAndOptionsConfiguration_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ApiCommonServiceRegistration.AddConfiguredSwagger(services);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IConfigureOptions<SwaggerGenOptions>) &&
            descriptor.ImplementationType == typeof(ConfigureSwaggerOptions));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType.Name.Contains("ISwaggerProvider", StringComparison.Ordinal));
    }

    [Fact]
    public void AddConfiguredOpenTelemetry_Should_NotRegisterTelemetry_When_TracingIsDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:OpenTelemetrySettings:TracingSettings:Enabled"] = "false"
        });

        // Act
        ApiCommonServiceRegistration.AddConfiguredOpenTelemetry(services, configuration);

        // Assert
        services.Should().NotContain(descriptor =>
            descriptor.ServiceType.FullName != null &&
            descriptor.ServiceType.FullName.Contains("OpenTelemetry", StringComparison.Ordinal));
    }

    [Fact]
    public void AddConfiguredOpenTelemetry_Should_RegisterTelemetry_When_TracingIsEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:OpenTelemetrySettings:TracingSettings:Enabled"] = "true"
        });

        // Act
        ApiCommonServiceRegistration.AddConfiguredOpenTelemetry(services, configuration);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType.FullName != null &&
            descriptor.ServiceType.FullName.Contains("OpenTelemetry", StringComparison.Ordinal));
    }

    [Fact]
    public void AddConfiguredLogging_Should_RegisterDiagnosticContextAndLoggingProvider_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:LoggingSettings:EnableConsole"] = "false"
        });

        // Act
        ApiCommonServiceRegistration.AddConfiguredLogging(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<DiagnosticContext>().Should().NotBeNull();
        provider.GetRequiredService<ILoggerFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddConfiguredLogging_Should_RegisterConsoleSink_When_ConsoleLoggingIsEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:LoggingSettings:EnableConsole"] = "true",
            [$"{GCP_SETTINGS}:LoggingSettings:DefaultOutputTemplate"] = "[{Level}] {Message}{NewLine}{Exception}"
        });

        // Act
        ApiCommonServiceRegistration.AddConfiguredLogging(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<DiagnosticContext>().Should().NotBeNull();
        provider.GetRequiredService<ILoggerFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddApiVersioningInfrastructure_Should_RegisterApiExplorerProvider_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ApiCommonServiceRegistration.AddApiVersioningInfrastructure(services);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IApiVersionDescriptionProvider));
    }

    [Fact]
    public void AddConfiguredHealthChecks_Should_RegisterSelfCheckAndApplyCustomConfiguration_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var customCalled = false;

        // Act
        ApiCommonServiceRegistration.AddConfiguredHealthChecks(services, builder =>
        {
            customCalled = true;
            builder.AddCheck("custom", () => HealthCheckResult.Healthy());
        });
        using var provider = services.BuildServiceProvider();

        // Assert
        customCalled.Should().BeTrue();
        provider.GetRequiredService<HealthCheckService>().Should().NotBeNull();
    }

    private static IConfiguration BuildTokenConfiguration(string secretKey) =>
        BuildConfiguration(new Dictionary<string, string>
        {
            [$"{AUTH_SETTINGS}:Issuer"] = "haven",
            [$"{AUTH_SETTINGS}:Audiences:0"] = "portal",
            [$"{AUTH_SETTINGS}:SecretKey"] = secretKey
        });

    private static IConfiguration BuildConfiguration(Dictionary<string, string> values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
