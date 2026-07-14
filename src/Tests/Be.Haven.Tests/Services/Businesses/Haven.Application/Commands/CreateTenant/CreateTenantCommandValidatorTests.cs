using Haven.Application.Commands.CreateTenant;
using Haven.Application.Dtos.Tenants.Common;
using static Haven.Application.Constants.ApplicationConstants;

namespace Be.Haven.Tests.Services.Businesses.Haven.Application.Commands.CreateTenant;

public sealed class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_ProfileTenantPayloadIsValid()
    {
        var command = BuildValidCommand();

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_RoleCodeIsUnsupported()
    {
        var command = BuildValidCommand();
        command.RoleCode = "UNKNOWN";

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.ErrorMessage == global::Haven.Application.Constants.ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID);
    }

    [Fact]
    public void Validate_Should_Fail_When_OptionalTenantIdsAreEmpty()
    {
        var tenantIdCommand = BuildValidCommand();
        tenantIdCommand.TenantProfile = null;
        tenantIdCommand.TenantId = Guid.Empty;

        var accountIdCommand = BuildValidCommand();
        accountIdCommand.TenantProfile = null;
        accountIdCommand.TenantAccountId = Guid.Empty;

        _validator.Validate(tenantIdCommand).IsValid.Should().BeFalse();
        _validator.Validate(accountIdCommand).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_TenantSourceIsMissingOrDuplicated()
    {
        var missing = BuildValidCommand();
        missing.TenantProfile = null;

        var duplicated = BuildValidCommand();
        duplicated.TenantId = Guid.NewGuid();

        _validator.Validate(missing).IsValid.Should().BeFalse();
        _validator.Validate(duplicated).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Fail_When_ProfileEmailIsInvalid()
    {
        var command = BuildValidCommand();
        command.TenantProfile.Email = "not-email";

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName.EndsWith(nameof(TenantProfileRequestDto.Email)));
    }

    private static CreateTenantCommand BuildValidCommand()
    {
        return new CreateTenantCommand
        {
            RoomId = Guid.NewGuid(),
            RoleCode = TENANT_ROLE_PRIMARY,
            TenantProfile = new TenantProfileRequestDto
            {
                FullName = "Nguyễn Văn A",
                Phone = "0901234567",
                Email = "tenant@example.com"
            },
            ContractStartDate = new DateTime(2026, 7, 8),
            ContractEndDate = new DateTime(2027, 7, 8),
            ContractRentAmount = 3_000_000,
            DepositAmount = 3_000_000
        };
    }
}
