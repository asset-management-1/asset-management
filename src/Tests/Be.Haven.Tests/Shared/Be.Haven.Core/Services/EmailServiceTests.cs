namespace Be.Haven.Tests.Shared.Be.Haven.Core.Services;

public sealed class EmailServiceTests
{
    [Fact]
    public void Constructor_Should_CreateService_When_UsingConfiguredSendGridClient()
    {
        // Arrange
        var options = CreateOptions();

        // Act
        var sut = new EmailService(Mock.Of<ILogger<EmailService>>(), Options.Create(options));

        // Assert
        sut.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_Should_ReturnFalse_When_EmailConfigurationIsMissing()
    {
        // Arrange
        var sendGrid = new Mock<ISendGridClient>();
        var sut = CreateSut(new EmailOptions(), sendGrid);

        // Act
        var result = await sut.SendEmailAsync(CreateEmailRequest());

        // Assert
        result.Should().BeFalse();
        sendGrid.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_Should_ReturnFalse_When_RequestHasNoRecipient()
    {
        // Arrange
        var request = CreateEmailRequest();
        request.RequestData.To = [];
        var sendGrid = new Mock<ISendGridClient>();
        var sut = CreateSut(CreateOptions(), sendGrid);

        // Act
        var result = await sut.SendEmailAsync(request);

        // Assert
        result.Should().BeFalse();
        sendGrid.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendEmailAsync_Should_SendMessageWithRecipientsAndAttachments_When_RequestIsValid()
    {
        // Arrange
        SendGridMessage captured = null;
        var sendGrid = new Mock<ISendGridClient>();
        sendGrid
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((message, _) => captured = message)
            .ReturnsAsync(CreateResponse(HttpStatusCode.Accepted, string.Empty));
        var sut = CreateSut(CreateOptions(), sendGrid);

        // Act
        var result = await sut.SendEmailAsync(CreateEmailRequest());

        // Assert
        result.Should().BeTrue();
        captured.From.Email.Should().Be("noreply@haven.test");
        captured.Subject.Should().Be("Welcome");
        captured.Personalizations.Should().ContainSingle();
        captured.Personalizations[0].Tos.Should().ContainSingle(x => x.Email == "user@haven.test");
        captured.Personalizations[0].Ccs.Should().ContainSingle(x => x.Email == "cc@haven.test");
        captured.Personalizations[0].Bccs.Should().ContainSingle(x => x.Email == "bcc@haven.test");
        captured.Attachments.Should().ContainSingle(x => x.Filename == "hello.txt");
    }

    [Fact]
    public async Task SendEmailAsync_Should_ReturnFalse_When_SendGridRejectsMessage()
    {
        // Arrange
        var sendGrid = new Mock<ISendGridClient>();
        sendGrid
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResponse(HttpStatusCode.BadRequest, "invalid recipient"));
        var sut = CreateSut(CreateOptions(), sendGrid);

        // Act
        var result = await sut.SendEmailAsync(CreateEmailRequest());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_Should_ReturnFalse_When_SendGridThrowsNonCancellationException()
    {
        // Arrange
        var sendGrid = new Mock<ISendGridClient>();
        sendGrid
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("send failed"));
        var sut = CreateSut(CreateOptions(), sendGrid);

        // Act
        var result = await sut.SendEmailAsync(CreateEmailRequest());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmailAsync_Should_PropagateCancellation_When_SendGridIsCanceled()
    {
        // Arrange
        var sendGrid = new Mock<ISendGridClient>();
        sendGrid
            .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        var sut = CreateSut(CreateOptions(), sendGrid);

        // Act
        var action = () => sut.SendEmailAsync(CreateEmailRequest(), new CancellationToken(true));

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    private static EmailService CreateSut(
        EmailOptions options,
        Mock<ISendGridClient> sendGrid) =>
        new(
            Mock.Of<ILogger<EmailService>>(),
            Options.Create(options),
            sendGrid.Object);

    private static EmailOptions CreateOptions() =>
        new()
        {
            ApiKey = "SG.test",
            FromEmail = "noreply@haven.test",
            FromName = "Haven"
        };

    private static EmailRequest CreateEmailRequest() =>
        new()
        {
            RequestData = new RequestData
            {
                To =
                [
                    new EmailAddressRequest
                    {
                        Email = "user@haven.test"
                    }
                ],
                Cc =
                [
                    new EmailAddressRequest
                    {
                        Email = "cc@haven.test"
                    }
                ],
                Bcc =
                [
                    new EmailAddressRequest
                    {
                        Email = "bcc@haven.test"
                    }
                ],
                Subject = "Welcome",
                Body = "<p>Hello</p>",
                Attachments =
                [
                    new EmailAttachment
                    {
                        FileName = "hello.txt",
                        Base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")),
                        ContentType = "text/plain"
                    }
                ]
            }
        };

    private static Response CreateResponse(
        HttpStatusCode statusCode,
        string content) =>
        new(
            statusCode,
            new StringContent(content),
            null);
}
