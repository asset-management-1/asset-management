using Be.Haven.ApiCommon.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Be.Haven.Tests.Shared.Be.Haven.ApiCommon.Extensions;

public sealed class ServiceRegistrationOptionsTests
{
    [Fact]
    public void AddConfiguredOption_Should_ReturnBuilderForFeatureValidationReuse()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Feature:Name"] = "configured"
            })
            .Build();

        var builder = services.AddConfiguredOption<FeatureOptions>(configuration, "Feature");

        builder.Should().NotBeNull();
        builder.Validate(options => options.Name == "configured");
    }

    [Fact]
    public void AddConfiguredOption_Should_RunChainedFeatureValidationAtStartup()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Feature:Name"] = "configured"
            })
            .Build();
        services.AddConfiguredOption<FeatureOptions>(configuration, "Feature")
            .Validate(_ => false, "feature validation failed");
        using var provider = services.BuildServiceProvider();

        Action act = () => provider.GetRequiredService<IStartupValidator>().Validate();

        act.Should()
            .Throw<OptionsValidationException>()
            .WithMessage("*feature validation failed*");
    }

    private sealed class FeatureOptions
    {
        public string Name { get; set; }
    }
}
