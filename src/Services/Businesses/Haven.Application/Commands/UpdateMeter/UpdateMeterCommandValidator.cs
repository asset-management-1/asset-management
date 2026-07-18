namespace Haven.Application.Commands.UpdateMeter;

/// <summary>
/// Validates values used to update one room meter period.
/// </summary>
public class UpdateMeterCommandValidator : AbstractValidator<UpdateMeterCommand>
{
    /// <summary>
    /// Creates route, date, meter-value, price, and evidence validation rules.
    /// </summary>
    /// <param name="imageValidationService">The shared image decoder used to verify evidence bytes.</param>
    public UpdateMeterCommandValidator(IImageOptimizationService imageValidationService)
    {
        // The route and date identify the existing room period being edited.
        RuleFor(x => x.RoomId).RequiredGuid();

        RuleFor(x => x.BillingDate)
            .NotDefault()
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_DATE_INVALID);

        // At least one meter section must be submitted before the service checks all required policies.
        RuleFor(x => x)
            .Must(command => command.ElectricCurrent.HasValue || command.WaterCurrent.HasValue)
            .WithMessage(ApplicationErrorConstants.MeterErrors.ERROR_METER_SECTION_REQUIRED);

        // Optional partial-update values remain non-negative when supplied.
        RuleFor(x => x.ElectricPrevious).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.ElectricCurrent).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.ElectricPrice).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterPrevious).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterCurrent).GreaterOrEqualZeroWhenPresent();

        RuleFor(x => x.WaterPrice).GreaterOrEqualZeroWhenPresent();

        // A submitted baseline cannot exceed the matching current value.
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
            .MaxCount(MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.ElectricImages)
            .OptionalImageFile()
            .OptionalImageContent(imageValidationService);

        RuleFor(x => x.WaterImages)
            .MaxCount(MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.WaterImages)
            .OptionalImageFile()
            .OptionalImageContent(imageValidationService);

        // Delete requests carry only selected evidence identities; the service verifies room and utility ownership in its transaction.
        RuleFor(x => x.DeletedElectricImageIds)
            .MaxCount(MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.DeletedElectricImageIds).RequiredGuid();

        RuleFor(x => x.DeletedWaterImageIds)
            .MaxCount(MAX_METER_EVIDENCE_IMAGE_COUNT, ApplicationErrorConstants.MeterErrors.ERROR_METER_EVIDENCE_LIMIT);

        RuleForEach(x => x.DeletedWaterImageIds).RequiredGuid();
    }
}
