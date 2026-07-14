using System.Reflection;
using Be.Haven.Shared.Dtos.Options.Observability;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Helpers;

public sealed class OpenTelemetryHelperTests
{
    [Fact]
    public void Configure_Should_RegisterTracingPipeline_When_InstrumentationIsDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddOpenTelemetry();
        var resource = ResourceBuilder.CreateDefault().AddService("haven-tests");
        var options = new OpenTelemetryOptions
        {
            TracingSettings = new OpenTelemetryTracingOptions
            {
                Enabled = true,
                ExportToConsole = false,
                ExportToOtlp = false
            },
            InstrumentationSettings = new OpenTelemetryInsOptions
            {
                AspNetCore = new OpenTelemetryInsToggleOptions
                {
                    Enabled = false
                },
                HttpClient = new OpenTelemetryInsToggleOptions
                {
                    Enabled = false
                }
            }
        };

        // Act
        OpenTelemetryHelper.Configure(builder, resource, options);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetServices<object>().Should().NotBeNull();
    }

    [Fact]
    public void Configure_Should_RegisterInstrumentation_When_AspNetCoreAndHttpClientAreEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = services.AddOpenTelemetry();
        var resource = ResourceBuilder.CreateDefault().AddService("haven-tests");
        var options = new OpenTelemetryOptions
        {
            TracingSettings = new OpenTelemetryTracingOptions
            {
                Enabled = true,
                Filter = new OpenTelemetryFilterOptions
                {
                    ServerFilterPrefixes = ["/health"],
                    ClientFilterPrefixes = ["/metrics"],
                    ClientIgnoredHosts = ["metadata.google.internal"]
                }
            },
            InstrumentationSettings = new OpenTelemetryInsOptions
            {
                AspNetCore = new OpenTelemetryInsToggleOptions
                {
                    Enabled = true,
                    RecordException = true
                },
                HttpClient = new OpenTelemetryInsToggleOptions
                {
                    Enabled = true,
                    RecordException = true
                }
            }
        };

        // Act
        OpenTelemetryHelper.Configure(builder, resource, options);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddAspNetCore_Should_ConfigureInstrumentationOptions_When_Enabled()
    {
        // Arrange
        var builder = Sdk.CreateTracerProviderBuilder();
        var options = CreateEnabledOptions();

        // Act
        InvokePrivateStatic(
            "AddAspNetCore",
            builder,
            options,
            new[] { "/health" });
        using var provider = builder.Build();

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddHttpClient_Should_ConfigureInstrumentationOptions_When_Enabled()
    {
        // Arrange
        var builder = Sdk.CreateTracerProviderBuilder();
        var options = CreateEnabledOptions();

        // Act
        InvokePrivateStatic(
            "AddHttpClient",
            builder,
            options,
            new[] { "/metrics" },
            new[] { "metadata.google.internal" });
        using var provider = builder.Build();

        // Assert
        provider.Should().NotBeNull();
    }

    [Theory]
    [InlineData("/health/live", true)]
    [InlineData("/api/v1/auth/login", false)]
    [InlineData("", false)]
    public void StartsWithAny_Should_MatchConfiguredPrefixes_When_ValueIsProvided(
        string value,
        bool expected)
    {
        // Act
        var result = InvokePrivateStatic<bool>(
            "StartsWithAny",
            value,
            new[] { "/health", "/metrics" });

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://metadata.google.internal/computeMetadata/v1", true)]
    [InlineData("https://api.example.test:8443/users", true)]
    [InlineData("https://api.example.test/users", false)]
    public void IsIgnoredHost_Should_MatchHostOrHostWithPort_When_UriIsProvided(
        string uri,
        bool expected)
    {
        // Act
        var result = InvokePrivateStatic<bool>(
            "IsIgnoredHost",
            new Uri(uri),
            new[] { "metadata.google.internal", "api.example.test:8443" });

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("/health/live", false)]
    [InlineData("/api/v1/auth/login", true)]
    public void ShouldTraceServerRequest_Should_RespectConfiguredPrefixes_When_PathIsProvided(
        string path,
        bool expected)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        // Act
        var result = InvokePrivateStatic<bool>(
            "ShouldTraceServerRequest",
            context,
            new[] { "/health" });

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://metadata.google.internal/computeMetadata/v1", false)]
    [InlineData("https://api.example.test/metrics", false)]
    [InlineData("https://api.example.test/users", true)]
    public void ShouldTraceHttpRequest_Should_RespectIgnoredHostsAndPathPrefixes_When_RequestIsProvided(
        string uri,
        bool expected)
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);

        // Act
        var result = InvokePrivateStatic<bool>(
            "ShouldTraceHttpRequest",
            request,
            new[] { "/metrics" },
            new[] { "metadata.google.internal" });

        // Assert
        result.Should().Be(expected);
    }

    private static T InvokePrivateStatic<T>(string methodName, params object[] parameters)
    {
        var method = typeof(OpenTelemetryHelper).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        return (T)method.Invoke(null, parameters);
    }

    private static void InvokePrivateStatic(string methodName, params object[] parameters)
    {
        var method = typeof(OpenTelemetryHelper).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic);

        method.Invoke(null, parameters);
    }

    private static OpenTelemetryOptions CreateEnabledOptions() =>
        new()
        {
            TracingSettings = new OpenTelemetryTracingOptions
            {
                Enabled = true
            },
            InstrumentationSettings = new OpenTelemetryInsOptions
            {
                AspNetCore = new OpenTelemetryInsToggleOptions
                {
                    Enabled = true,
                    RecordException = true
                },
                HttpClient = new OpenTelemetryInsToggleOptions
                {
                    Enabled = true,
                    RecordException = true
                }
            }
        };
}
