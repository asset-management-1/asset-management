namespace Haven.Application.Commands.CreateTenant;

/// <summary>
/// Validates tenant creation requests.
/// </summary>
public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    /// <summary>
    /// Creates validation rules for tenant creation.
    /// </summary>
    public CreateTenantCommandValidator()
    {
        // Tenant creation is always attached to one room selected by the landlord.
        RuleFor(x => x.RoomId).RequiredGuid();

        // Optional existing tenant identifiers must not carry Guid.Empty.
        RuleFor(x => x.TenantId).NotDefaultWhenPresent();
        RuleFor(x => x.TenantAccountId).NotDefaultWhenPresent();

        // Role drives whether the flow creates a contract or only an occupant membership.
        RuleFor(x => x.RoleCode)
            .Required()
            .MaxLen(100)
            .Must(roleCode => string.Equals(roleCode, TENANT_ROLE_PRIMARY, StringComparison.OrdinalIgnoreCase)
                              || string.Equals(roleCode, TENANT_ROLE_OCCUPANT, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_ROLE_INVALID);

        // Optional money values must not create negative contract or deposit data.
        RuleFor(x => x.ContractRentAmount).GreaterOrEqualZeroWhenPresent();
        RuleFor(x => x.DepositAmount).GreaterOrEqualZeroWhenPresent();

        // When both dates are supplied, the tenant stay window must be chronological.
        RuleFor(x => x.ContractEndDate)
            .GreaterThanOrEqualTo(x => x.ContractStartDate)
            .When(x => x.ContractStartDate.HasValue && x.ContractEndDate.HasValue);

        // FE must choose exactly one tenant source: party id, account id, or inline profile.
        RuleFor(x => x)
            .Must(HasExactlyOneTenantSource)
            .WithMessage(ApplicationErrorConstants.TenantErrors.ERROR_TENANT_SOURCE_REQUIRED);

        // Manual tenant profile is validated only for no-account tenants.
        RuleFor(x => x.TenantProfile).SetValidator(new TenantProfileRequestDtoValidator())
            .When(x => x.TenantProfile is not null);
    }

    /// <summary>
    /// Checks that the request identifies exactly one tenant source.
    /// </summary>
    /// <param name="command">The tenant creation command.</param>
    /// <returns><c>true</c> when one tenant source is supplied.</returns>
    private static bool HasExactlyOneTenantSource(CreateTenantCommand command)
    {
        // The landlord can choose an existing tenant party, an account, or manual profile, never a mix.
        var count = 0;

        if (command.TenantId.HasValue)
        {
            count++;
        }

        if (command.TenantAccountId.HasValue)
        {
            count++;
        }

        if (command.TenantProfile is not null)
        {
            count++;
        }

        return count == 1;
    }
}

/// <summary>
/// Validates inline tenant profile data.
/// </summary>
public class TenantProfileRequestDtoValidator : AbstractValidator<TenantProfileRequestDto>
{
    /// <summary>
    /// Creates validation rules for an inline tenant profile.
    /// </summary>
    public TenantProfileRequestDtoValidator()
    {
        // A no-account tenant still needs a display name for room cards and contract screens.
        RuleFor(x => x.FullName).Required().MaxLen(255);

        // Contact fields are optional but bounded for storage and search display.
        RuleFor(x => x.Phone).MaxLen(50);

        RuleFor(x => x.Email)
            .MaxLen(255)
            .EmailFormat()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
