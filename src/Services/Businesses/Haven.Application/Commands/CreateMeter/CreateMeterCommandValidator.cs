namespace Haven.Application.Commands.CreateMeter;

/// <summary>
/// Validates values used to create one room meter period.
/// </summary>
public class CreateMeterCommandValidator : AbstractValidator<CreateMeterCommand>
{
    /// <summary>
    /// Creates route, date, meter-value, price, and evidence validation rules.
    /// </summary>
    /// <param name="imageValidationService">The shared image decoder used to verify evidence bytes.</param>
    public CreateMeterCommandValidator(IImageOptimizationService imageValidationService)
    {
        // The route and date identify the room and calendar period being created.
        RuleFor(x => x.RoomId).RequiredGuid();

        RuleFor(x => x.BillingDate)
            .NotDefault()
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_DATE_INVALID);

        // At least one meter section must be present before the service checks the effective room policies.
        RuleFor(x => x)
            .Must(command => command.ElectricCurrent.HasValue || command.WaterCurrent.HasValue)
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_SECTION_REQUIRED);

        // Meter values and period prices cannot be negative when supplied.
        RuleFor(x => x.ElectricPrevious).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.ElectricCurrent).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.ElectricPrice).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterPrevious).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterCurrent).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterPrice).GreaterOrEqualZeroWhenPresent();

        // A manually supplied first-period baseline cannot exceed the current meter value.
        RuleFor(x => x)
            .Must(command => !command.ElectricPrevious.HasValue
                             || !command.ElectricCurrent.HasValue
                             || command.ElectricCurrent >= command.ElectricPrevious)
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_RANGE);

        RuleFor(x => x)
            .Must(command => !command.WaterPrevious.HasValue
                             || !command.WaterCurrent.HasValue
                             || command.WaterCurrent >= command.WaterPrevious)
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_RANGE);

        // Each utility accepts a bounded image collection; Core decodes bytes instead of trusting mobile MIME metadata.
        RuleFor(x => x.ElectricImages)
            .MaxCount(ObjectStorageConstants.MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.ElectricImages)
            .OptionalImageFile()
            .OptionalImageContent(imageValidationService);

        RuleFor(x => x.WaterImages)
            .MaxCount(ObjectStorageConstants.MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.WaterImages)
            .OptionalImageFile()
            .OptionalImageContent(imageValidationService);

    }
}
