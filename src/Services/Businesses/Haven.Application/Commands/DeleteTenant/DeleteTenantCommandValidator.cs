namespace Haven.Application.Commands.DeleteTenant;

/// <summary>
/// Validates tenant move-out route input.
/// </summary>
public class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    /// <summary>
    /// Creates validation rules for tenant move-out commands.
    /// </summary>
    public DeleteTenantCommandValidator()
    {
        // Move-out is scoped by an occupancy public id and never deletes the tenant party/account.
        RuleFor(x => x.Id).RequiredGuid();
    }
}




