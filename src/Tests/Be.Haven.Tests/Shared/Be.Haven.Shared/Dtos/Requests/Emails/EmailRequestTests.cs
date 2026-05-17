namespace Be.Haven.Tests.Shared.Be.Haven.Shared.Dtos.Requests.Emails;

public sealed class EmailRequestTests
{
    [Fact]
    public void Properties_Should_RoundTripValues_When_EmailRequestIsPopulated()
    {
        // Arrange
        var from = new EmailAccountRequest
        {
            Email = "sender@haven.test",
            Password = "secret"
        };
        var credential = new EmailApiCredentialRequest
        {
            UserName = "api-user",
            Password = "api-secret"
        };
        var attachment = new EmailAttachment
        {
            FileName = "contract.pdf",
            Base64 = "Y29udHJhY3Q=",
            ContentType = "application/pdf"
        };
        var requestData = new RequestData
        {
            To = [new EmailAddressRequest { Email = "to@haven.test" }],
            Cc = [new EmailAddressRequest { Email = "cc@haven.test" }],
            Bcc = [new EmailAddressRequest { Email = "bcc@haven.test" }],
            From = from,
            Subject = "Subject",
            Body = "Body",
            Attachments = [attachment]
        };

        // Act
        var result = new EmailRequest
        {
            RequestData = requestData,
            ApiCredential = credential
        };

        // Assert
        result.RequestData.Should().BeSameAs(requestData);
        result.ApiCredential.Should().BeSameAs(credential);
        result.RequestData.From.Should().BeSameAs(from);
        result.RequestData.To.Should().ContainSingle().Which.Email.Should().Be("to@haven.test");
        result.RequestData.Cc.Should().ContainSingle().Which.Email.Should().Be("cc@haven.test");
        result.RequestData.Bcc.Should().ContainSingle().Which.Email.Should().Be("bcc@haven.test");
        result.RequestData.Subject.Should().Be("Subject");
        result.RequestData.Body.Should().Be("Body");
        result.RequestData.Attachments.Should().ContainSingle().Which.Should().BeSameAs(attachment);
    }

    [Fact]
    public void RequestData_Should_DefaultAttachmentsToEmptyCollection_When_NotProvided()
    {
        // Act
        var result = new RequestData();

        // Assert
        result.Attachments.Should().BeEmpty();
    }
}
