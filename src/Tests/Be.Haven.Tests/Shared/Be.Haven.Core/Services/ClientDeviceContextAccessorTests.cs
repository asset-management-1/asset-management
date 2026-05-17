namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class ClientDeviceContextAccessorTests
{
    [Fact]
    public void GetCurrent_Should_ReadAndNormalizeDeviceHeaders_When_RequestContextExists()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[ClientDeviceHeaders.DEVICE_ID] = "  browser-1  ";
        context.Request.Headers[ClientDeviceHeaders.DEVICE_NAME] = "  Codex Smoke  ";
        context.Request.Headers[ClientDeviceHeaders.DEVICE_TYPE] = " WEB ";
        context.Request.Headers[ClientDeviceHeaders.USER_AGENT] = " Test Agent ";
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        var sut = CreateSut(context);

        // Act
        var result = sut.GetCurrent();

        // Assert
        result.DeviceId.Should().Be("browser-1");
        result.DeviceName.Should().Be("Codex Smoke");
        result.DeviceType.Should().Be("WEB");
        result.UserAgent.Should().Be("Test Agent");
        result.IpAddress.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetCurrent_Should_ReturnFallbackValues_When_HttpContextIsMissing()
    {
        // Arrange
        var sut = CreateSut(null);

        // Act
        var result = sut.GetCurrent();

        // Assert
        result.DeviceId.Should().Be(ClientDeviceFallbacks.DEVICE_ID);
        result.DeviceName.Should().Be(ClientDeviceFallbacks.DEVICE_NAME);
        result.DeviceType.Should().Be(ClientDeviceFallbacks.DEVICE_TYPE);
        result.UserAgent.Should().Be(ClientDeviceFallbacks.USER_AGENT);
        result.IpAddress.Should().Be(ClientDeviceFallbacks.IP_ADDRESS);
    }

    [Fact]
    public void GetCurrent_Should_TruncateValues_When_MetadataExceedsLimits()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers[ClientDeviceHeaders.DEVICE_ID] = new string('d', ClientDeviceMetadataLimits.DEVICE_ID_MAX_LENGTH + 5);
        context.Request.Headers[ClientDeviceHeaders.DEVICE_NAME] = new string('n', ClientDeviceMetadataLimits.DEVICE_NAME_MAX_LENGTH + 5);
        context.Request.Headers[ClientDeviceHeaders.DEVICE_TYPE] = new string('t', ClientDeviceMetadataLimits.DEVICE_TYPE_MAX_LENGTH + 5);
        context.Request.Headers[ClientDeviceHeaders.USER_AGENT] = new string('a', ClientDeviceMetadataLimits.USER_AGENT_MAX_LENGTH + 5);
        context.Connection.RemoteIpAddress = IPAddress.Parse("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        var sut = CreateSut(context);

        // Act
        var result = sut.GetCurrent();

        // Assert
        result.DeviceId.Should().HaveLength(ClientDeviceMetadataLimits.DEVICE_ID_MAX_LENGTH);
        result.DeviceName.Should().HaveLength(ClientDeviceMetadataLimits.DEVICE_NAME_MAX_LENGTH);
        result.DeviceType.Should().HaveLength(ClientDeviceMetadataLimits.DEVICE_TYPE_MAX_LENGTH);
        result.UserAgent.Should().HaveLength(ClientDeviceMetadataLimits.USER_AGENT_MAX_LENGTH);
        result.IpAddress.Length.Should().BeLessThanOrEqualTo(ClientDeviceMetadataLimits.IP_ADDRESS_MAX_LENGTH);
    }

    private static ClientDeviceContextAccessor CreateSut(HttpContext context)
    {
        return new ClientDeviceContextAccessor(new HttpContextAccessor
        {
            HttpContext = context
        });
    }
}
