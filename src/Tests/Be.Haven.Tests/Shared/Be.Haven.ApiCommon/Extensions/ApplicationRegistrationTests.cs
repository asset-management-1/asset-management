using ApiCommonApplicationRegistration = Be.Haven.ApiCommon.Extensions.ApplicationRegistration;
using ApiCommonServiceRegistration = Be.Haven.ApiCommon.Extensions.ServiceRegistration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Serilog.Extensions.Hosting;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Extensions;

public sealed class ApplicationRegistrationTests
{
#pragma warning disable ASPDEPR004, ASPDEPR008

    [Fact]
    public async Task UseHealthChecks_Should_ReturnHealthyJson_When_HealthEndpointIsRequested()
    {
        // Arrange
        using var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services => services.AddHealthChecks())
            .Configure(app => ApiCommonApplicationRegistration.UseHealthChecks(app)));
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("\"status\": \"Healthy\"");
    }

    [Fact]
    public async Task UseHealthChecks_Should_ReturnServiceUnavailable_When_HealthCheckIsUnhealthy()
    {
        // Arrange
        using var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
                services.AddHealthChecks().AddCheck("broken", () => HealthCheckResult.Unhealthy("down")))
            .Configure(app => ApiCommonApplicationRegistration.UseHealthChecks(app)));
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        content.Should().Contain("\"name\": \"broken\"");
        content.Should().Contain("\"status\": \"Unhealthy\"");
    }

    [Fact]
    public async Task UseApplicationMiddlewares_Should_WriteCorrelationHeader_When_RequestSucceeds()
    {
        // Arrange
        using var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddSingleton(new DiagnosticContext(Serilog.Log.Logger));
            })
            .Configure(app =>
            {
                ApiCommonApplicationRegistration.UseApplicationMiddlewares(app);
                app.Run(context => context.Response.WriteAsync("ok"));
            }));
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(header => header.Key == X_CORRELATION_ID);
    }

    [Fact]
    public async Task UseSwaggerConfiguration_Should_ServeSharedOperationSearchScript_When_SwaggerAssetsAreRequested()
    {
        // Arrange
        using var server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddRouting();
                ApiCommonServiceRegistration.AddApiVersioningInfrastructure(services);
                ApiCommonServiceRegistration.AddConfiguredSwagger(services);
            })
            .Configure(app => ApiCommonApplicationRegistration.UseSwaggerConfiguration(app)));
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync(SwaggerUiConstants.OPERATION_SEARCH_SCRIPT_PATH);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ToString().Should().Be(SwaggerUiConstants.JAVASCRIPT_CONTENT_TYPE);
        content.Should().Contain("querySelector");
    }

    [Fact]
    public void LoadSwaggerOperationSearchScript_Should_ThrowInvalidOperationException_When_ResourceIsMissing()
    {
        // Arrange
        var method = typeof(ApiCommonApplicationRegistration).GetMethod(
            "LoadSwaggerOperationSearchScript",
            BindingFlags.Static | BindingFlags.NonPublic,
            [typeof(string)]);

        // Act
        var action = () => method.Invoke(null, ["Be.Haven.ApiCommon.Missing.js"]);

        // Assert
        action.Should().Throw<TargetInvocationException>()
            .WithInnerException<InvalidOperationException>();
    }

    [Fact]
    public async Task WriteHealthCheckResponseAsync_Should_ReturnWithoutWriting_When_RequestIsCanceled()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var context = new DefaultHttpContext
        {
            RequestAborted = cancellationTokenSource.Token
        };
        context.Response.Body = new MemoryStream();
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        // Act
        await InvokeHealthCheckResponseWriterAsync(context, report);

        // Assert
        context.Response.Body.Length.Should().Be(0);
        context.Response.ContentType.Should().BeNull();
    }

    private static async Task InvokeHealthCheckResponseWriterAsync(
        HttpContext context,
        HealthReport report)
    {
        var method = typeof(ApiCommonApplicationRegistration).GetMethod(
            "WriteHealthCheckResponseAsync",
            BindingFlags.Static | BindingFlags.NonPublic);
        var task = (Task)method.Invoke(null, [context, report]);

        await task;
    }

#pragma warning restore ASPDEPR004, ASPDEPR008
}
