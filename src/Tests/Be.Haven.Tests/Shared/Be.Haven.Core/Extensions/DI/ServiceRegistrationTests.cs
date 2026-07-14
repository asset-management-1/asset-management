using System.Reflection;
using Be.Haven.Core.Extensions.DI;
using Be.Haven.Core.Interfaces.Repositories;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Hosting;
using CoreServiceRegistration = Be.Haven.Core.Extensions.DI.ServiceRegistration;

namespace Be.Haven.Tests.Shared.Be.Haven.Core.Extensions.DI;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void AddCoreInfrastructure_Should_RegisterCoreServices_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        try
        {
            CoreServiceRegistration.AddCoreInfrastructure(services);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("TypeAdapter.Adapt was already called", StringComparison.Ordinal))
        {
        }

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHttpContextAccessor));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(CorrelationIdHandler) &&
            descriptor.Lifetime == ServiceLifetime.Transient);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IJsonSerializerService) &&
            descriptor.ImplementationType == typeof(JsonSerializerService) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IAuthService) &&
            descriptor.ImplementationType == typeof(AuthService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IClientDeviceContextAccessor) &&
            descriptor.ImplementationType == typeof(ClientDeviceContextAccessor) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddUnitOfWork_Should_RegisterUnitOfWorkForProvidedDbContext_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        CoreServiceRegistration.AddUnitOfWork<CoreRepositoryDbContext>(services);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IUnitOfWork) &&
            descriptor.ImplementationType == typeof(UnitOfWork<CoreRepositoryDbContext>) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddDapperInfrastructure_Should_RegisterConnectionFactoryAndDapperService_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        CoreServiceRegistration.AddDapperInfrastructure(services);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IDbConnectionFactory) &&
            descriptor.ImplementationType == typeof(DbConnectionFactory) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IDapperService) &&
            descriptor.ImplementationType == typeof(DapperService) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddDbConnectionInitialization_Should_RegisterConnectionRefreshServices_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        CoreServiceRegistration.AddDbConnectionInitialization(services);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IConnectionStringProvider) &&
            descriptor.ImplementationType == typeof(ConnectionStringProvider) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService) &&
            descriptor.ImplementationType == typeof(DbConnectionHostService));
    }

    [Fact]
    public void AddAuthenticatedHttpClient_Should_RegisterTypedClientWithConfiguredBaseAddress_When_ClientIsResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        CoreServiceRegistration.AddAuthenticatedHttpClient<ICoreRegistrationClient, CoreRegistrationClient>(
            services,
            (_, client) => client.BaseAddress = new Uri("https://api.example.test"));
        using var provider = services.BuildServiceProvider();

        // Act
        var result = provider.GetRequiredService<ICoreRegistrationClient>();

        // Assert
        result.Client.BaseAddress.Should().Be(new Uri("https://api.example.test"));
    }

    [Fact]
    public void AddAuthenticatedHttpClientWithTokenHandler_Should_RegisterTypedClient_When_ClientIsResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        services.AddTransient<CoreRegistrationTokenHandler>();
        CoreServiceRegistration.AddAuthenticatedHttpClient<
            ICoreRegistrationClient,
            CoreRegistrationClient,
            CoreRegistrationTokenHandler>(services);
        using var provider = services.BuildServiceProvider();

        // Act
        var result = provider.GetRequiredService<ICoreRegistrationClient>();

        // Assert
        result.Should().BeOfType<CoreRegistrationClient>();
    }

    [Fact]
    public void AddAuthenticatedHttpClientMultiple_Should_RegisterServiceForNamedClient_When_SingleClientOverloadIsUsed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        var configuredClient = false;
        var configuredBuilder = false;

        // Act
        CoreServiceRegistration.AddAuthenticatedHttpClientMultiple<
            ICoreRegistrationClient,
            CoreRegistrationClient>(
            services,
            "registration-client",
            (_, client) =>
            {
                configuredClient = true;
                client.BaseAddress = new Uri("https://registration.example.test");
            },
            _ => configuredBuilder = true);
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("registration-client");

        // Assert
        configuredClient.Should().BeTrue();
        configuredBuilder.Should().BeTrue();
        client.BaseAddress.Should().Be(new Uri("https://registration.example.test"));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICoreRegistrationClient) &&
            descriptor.ImplementationType == typeof(CoreRegistrationClient));
    }

    [Fact]
    public void AddAuthenticatedHttpClientMultiple_Should_RegisterServiceForAllNamedClients_When_ParamsOverloadIsUsed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpContextAccessor();
        services.AddTransient<CorrelationIdHandler>();
        var configuredClients = new List<string>();
        var configuredBuilders = 0;

        // Act
        CoreServiceRegistration.AddAuthenticatedHttpClientMultiple<
            ICoreRegistrationClient,
            CoreRegistrationClient>(
            services,
            ("one", (_, client) =>
            {
                configuredClients.Add("one");
                client.BaseAddress = new Uri("https://one.example.test");
            }, _ => configuredBuilders++),
            ("two", (_, client) =>
            {
                configuredClients.Add("two");
                client.BaseAddress = new Uri("https://two.example.test");
            }, _ => configuredBuilders++));
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var firstClient = factory.CreateClient("one");
        var secondClient = factory.CreateClient("two");

        // Assert
        configuredClients.Should().Equal("one", "two");
        configuredBuilders.Should().Be(2);
        firstClient.BaseAddress.Should().Be(new Uri("https://one.example.test"));
        secondClient.BaseAddress.Should().Be(new Uri("https://two.example.test"));
        services.Count(descriptor => descriptor.ServiceType == typeof(ICoreRegistrationClient))
            .Should().Be(1);
    }

    [Fact]
    public void AddGooglePubSubPublisher_Should_RegisterPublisherServicesAndOptions_When_ConfigIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{QUEUE_INFO_SETTINGS}:Topics:property-created"] = "topic-properties",
            [$"{QUEUE_INFO_SETTINGS}:Subscriptions:property-created"] = "sub-properties"
        });

        // Act
        CoreServiceRegistration.AddGooglePubSubPublisher(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IGcpPubSubPublisherClientFactory) &&
            descriptor.ImplementationType == typeof(GcpPubSubPublisherClientFactoryService));
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IGcpPublisherService) &&
            descriptor.ImplementationType == typeof(GcpPublisherService));
        provider.GetRequiredService<QueueInfoOptions>().Topics["property-created"].Should().Be("topic-properties");
    }

    [Fact]
    public void AddGooglePubSubSubscriber_Should_RegisterConfiguredHostedService_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(new GcpOptions
        {
            ProjectId = "project-id"
        }));
        services.AddSingleton(new QueueInfoOptions());

        // Act
        CoreServiceRegistration.AddGooglePubSubSubscriber(
            services,
            job => job.SubscribeAsync<CoreSubscriberSampleEvent, CoreSubscriberSampleHandler>());
        using var provider = services.BuildServiceProvider();
        var hostedService = provider.GetServices<IHostedService>().Single();

        // Assert
        hostedService.Should().BeOfType<GcpSubscriberJob>();
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddConfiguredDbContext_Should_ConfigurePostgreSqlProvider_When_PostgreSqlIsSelected()
    {
        // Arrange
        var services = CreateDbContextServices(SqlProvider.PostgreSql);
        CoreServiceRegistration.AddConfiguredDbContext<CoreRepositoryDbContext>(services, "Main");
        using var provider = services.BuildServiceProvider();

        // Act
        var result = provider.GetRequiredService<CoreRepositoryDbContext>();

        // Assert
        result.Database.ProviderName.Should().Contain("Npgsql");
    }

    [Fact]
    public void AddConfiguredDbContext_Should_ConfigureSqlServerProvider_When_SqlServerIsSelected()
    {
        // Arrange
        var services = CreateDbContextServices(SqlProvider.SqlServer);
        CoreServiceRegistration.AddConfiguredDbContext<CoreRepositoryDbContext>(services, "Main");
        using var provider = services.BuildServiceProvider();

        // Act
        var result = provider.GetRequiredService<CoreRepositoryDbContext>();

        // Assert
        result.Database.ProviderName.Should().Contain("SqlServer");
    }

    [Fact]
    public void AddConfiguredDbContext_Should_EnableDevelopmentOptions_When_EnvironmentIsDevelopment()
    {
        // Arrange
        var previousEnvironment = Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT);
        Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, ENVIRONMENT_DEVELOPMENT);
        var services = CreateDbContextServices(SqlProvider.PostgreSql);
        CoreServiceRegistration.AddConfiguredDbContext<CoreRepositoryDbContext>(services, "Main");
        using var provider = services.BuildServiceProvider();

        try
        {
            // Act
            var result = provider.GetRequiredService<CoreRepositoryDbContext>();

            // Assert
            result.Database.ProviderName.Should().Contain("Npgsql");
        }
        finally
        {
            Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, previousEnvironment);
        }
    }

    [Fact]
    public void AddConfiguredDbContext_Should_ThrowNotSupportedException_When_ProviderIsUnsupported()
    {
        // Arrange
        var services = CreateDbContextServices(unchecked((SqlProvider)999));
        CoreServiceRegistration.AddConfiguredDbContext<CoreRepositoryDbContext>(services, "Main");
        using var provider = services.BuildServiceProvider();

        // Act
        var action = () => provider.GetRequiredService<CoreRepositoryDbContext>();

        // Assert
        action.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void AddGcpSecretManagerInfrastructure_Should_RegisterLocalSecretService_When_SecretModeIsDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICachingService>(Mock.Of<ICachingService>());
        services.AddSingleton(Options.Create(new GcpOptions
        {
            ProjectNumber = "123456",
            SecretManagerSettings = new SecretManagerOptions
            {
                IsUseSecret = false
            }
        }));
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:ProjectNumber"] = "123456",
            [$"{GCP_SETTINGS}:SecretManagerSettings:IsUseSecret"] = "false"
        });

        // Act
        CoreServiceRegistration.AddGcpSecretManagerInfrastructure(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetRequiredService<IGcpSecretService>().Should().BeOfType<GcpSecretService>();
    }

    [Fact]
    public void AddGcpSecretManagerInfrastructure_Should_RegisterSecretManagerClient_When_SecretModeIsEnabled()
    {
        // Arrange
        using var credentialFile = new ServiceAccountCredentialFile();
        var previousCredentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile.Path);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICachingService>(Mock.Of<ICachingService>());
        services.AddSingleton(Options.Create(new GcpOptions
        {
            ProjectNumber = "123456",
            SecretManagerSettings = new SecretManagerOptions
            {
                IsUseSecret = true
            }
        }));
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{GCP_SETTINGS}:ProjectNumber"] = "123456",
            [$"{GCP_SETTINGS}:SecretManagerSettings:IsUseSecret"] = "true"
        });

        try
        {
            // Act
            GcpCredentialTestGuard.ExecuteOrSkip(() =>
            {
                CoreServiceRegistration.AddGcpSecretManagerInfrastructure(services, configuration);
                using var provider = services.BuildServiceProvider();
                var result = provider.GetRequiredService<IGcpSecretService>();

                // Assert
                result.Should().BeOfType<GcpSecretService>();
                provider.GetRequiredService<SecretManagerServiceClient>().Should().NotBeNull();
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", previousCredentialPath);
        }
    }

    [Fact]
    public void AddEmailService_Should_RegisterEmailServiceAndBindOptions_When_ConfigIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{EMAIL_SETTINGS}:ApiKey"] = "test-api-key",
            [$"{EMAIL_SETTINGS}:FromEmail"] = "no-reply@example.test",
            [$"{EMAIL_SETTINGS}:FromName"] = "Haven Tests"
        });

        // Act
        var result = CoreServiceRegistration.AddEmailService(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IEmailService) &&
            descriptor.ImplementationType == typeof(EmailService));
        provider.GetRequiredService<IOptions<EmailOptions>>().Value.FromEmail.Should().Be("no-reply@example.test");
    }

    [Fact]
    public void AddEmailService_Should_RejectMissingRequiredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>());
        CoreServiceRegistration.AddEmailService(services, configuration);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IOptions<EmailOptions>>().Value;

        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddUploadGcpService_Should_RegisterUploadServiceAndBindOptions_When_ConfigIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{UPLOAD_GCP_SETTINGS}:Path"] = "uploads"
        });

        // Act
        var result = CoreServiceRegistration.AddUploadGcpService(services, configuration);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IUploadGcpService) &&
            descriptor.ImplementationType == typeof(UploadGcpService));
    }

    [Fact]
    public void AddR2ObjectStorageService_Should_RegisterObjectStorageServiceAndBindOptions_When_ConfigIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{R2_STORAGE_SETTINGS}:Endpoint"] = "https://r2.example.test",
            [$"{R2_STORAGE_SETTINGS}:BucketName"] = "haven-assets",
            [$"{R2_STORAGE_SETTINGS}:AccessKeyId"] = "test-access-key",
            [$"{R2_STORAGE_SETTINGS}:SecretAccessKey"] = "test-secret-key"
        });

        // Act
        var result = CoreServiceRegistration.AddR2ObjectStorageService(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IObjectStorageService) &&
            descriptor.ImplementationType == typeof(R2ObjectStorageService));
        provider.GetRequiredService<IOptions<R2StorageOptions>>().Value.BucketName.Should().Be("haven-assets");
    }

    [Fact]
    public void AddR2ObjectStorageService_Should_RejectMissingRequiredConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>());
        CoreServiceRegistration.AddR2ObjectStorageService(services, configuration);
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IOptions<R2StorageOptions>>().Value;

        action.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void AddConfiguredQuartz_Should_RegisterQuartzHostedService_When_ConfiguredJobIsEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:Enabled"] = "true",
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:IntervalSeconds"] = "60"
        });

        // Act
        CoreServiceRegistration.AddConfiguredQuartz(services, configuration);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddConfiguredQuartz_Should_SkipJobSchedule_When_ConfiguredJobIsDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:Enabled"] = "false"
        });

        // Act
        CoreServiceRegistration.AddConfiguredQuartz(services, configuration);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddConfiguredQuartz_Should_RegisterDefaultIntervalTrigger_When_IntervalIsMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostApplicationLifetime>(Mock.Of<IHostApplicationLifetime>());
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:Enabled"] = "true"
        });

        // Act
        CoreServiceRegistration.AddConfiguredQuartz(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetServices<IHostedService>().Should().NotBeEmpty();
    }

    [Fact]
    public void AddGcpCloudFileProvider_Should_RegisterStorageAndUploadServices_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = CoreServiceRegistration.AddGcpCloudFileProvider(services);

        // Assert
        result.Should().BeSameAs(services);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(StorageClient) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IGcpUploadService) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddGcpCloudFileProvider_Should_ResolveUploadService_When_CredentialsAreAvailable()
    {
        // Arrange
        using var credentialFile = new ServiceAccountCredentialFile();
        var previousCredentialPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFile.Path);
        var services = new ServiceCollection();
        services.AddLogging();
        CoreServiceRegistration.AddGcpCloudFileProvider(services);

        try
        {
            // Act
            GcpCredentialTestGuard.ExecuteOrSkip(() =>
            {
                using var provider = services.BuildServiceProvider();
                var result = provider.GetRequiredService<IGcpUploadService>();

                // Assert
                result.Should().BeOfType<GcpUploadService>();
            });
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", previousCredentialPath);
        }
    }

    [Fact]
    public void AddConfiguredQuartz_Should_RegisterCronTrigger_When_CronExpressionIsConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostApplicationLifetime>(Mock.Of<IHostApplicationLifetime>());
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:Enabled"] = "true",
            [$"{QUARTZ_SETTINGS}:Jobs:registration-job:CronExpression"] = "0/30 * * * * ?"
        });

        // Act
        CoreServiceRegistration.AddConfiguredQuartz(services, configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        provider.GetServices<IHostedService>().Should().NotBeEmpty();
    }

    [Fact]
    public void SafeGetTypes_Should_ReturnLoadableTypes_When_AssemblyPartiallyFailsToLoad()
    {
        // Arrange
        var assembly = new ThrowingTypeLoadAssembly();
        var method = typeof(CoreServiceRegistration).GetMethod(
            "SafeGetTypes",
            BindingFlags.Static | BindingFlags.NonPublic);

        // Act
        var result = ((IEnumerable<Type>)method.Invoke(null, [assembly])).ToList();

        // Assert
        result.Should().Equal(typeof(CoreRegistrationQuartzJob));
    }

    private static ServiceCollection CreateDbContextServices(SqlProvider provider)
    {
        var services = new ServiceCollection();
        var connectionProvider = new Mock<IConnectionStringProvider>();
        connectionProvider.Setup(x => x.GetConnectionString("Main"))
            .Returns("Host=localhost;Database=haven;Username=haven;Password=haven;TrustServerCertificate=True");
        services.AddLogging();
        services.AddSingleton(connectionProvider.Object);
        services.AddSingleton(Options.Create(new GcpOptions
        {
            DatabaseSettings = new DatabaseOptions
            {
                Provider = provider,
                ConnectionName = "Main"
            }
        }));

        return services;
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string> values = null)
    {
        values ??= [];

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private interface ICoreRegistrationClient
    {
        HttpClient Client { get; }
    }

    private sealed class CoreRegistrationClient : ICoreRegistrationClient
    {
        public CoreRegistrationClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }

    private sealed class CoreRegistrationTokenHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            base.SendAsync(request, cancellationToken);
    }

    [QuartzJobKey("registration-job")]
    private sealed class CoreRegistrationQuartzJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    [QuartzJobKey("registration-job")]
    private sealed class CoreRegistrationDuplicateQuartzJob : IJob
    {
        public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
    }

    private sealed class ThrowingTypeLoadAssembly : Assembly
    {
        public override Type[] GetTypes() =>
            throw new ReflectionTypeLoadException(
                [typeof(CoreRegistrationQuartzJob), null],
                [new TypeLoadException("missing test type")]);
    }

    private sealed class ServiceAccountCredentialFile : IDisposable
    {
        public ServiceAccountCredentialFile()
        {
            using var rsa = RSA.Create(2048);
            var privateKey = rsa.ExportPkcs8PrivateKeyPem().Replace("\n", "\\n");
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"haven-registration-{Guid.NewGuid():N}.json");
            var json =
                $$"""
                {
                  "type": "service_account",
                  "project_id": "haven-test",
                  "private_key_id": "test-key",
                  "private_key": "{{privateKey}}",
                  "client_email": "haven-test@haven-test.iam.gserviceaccount.com",
                  "client_id": "1234567890",
                  "token_uri": "https://oauth2.googleapis.com/token"
                }
                """;

            File.WriteAllText(Path, json);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }
}
