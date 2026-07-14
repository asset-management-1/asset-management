namespace Haven.Infrastructure.Mappings.MasterData;

/// <summary>
/// Maps resolved master-data values into small lookup models used by mutation mappers.
/// </summary>
public static class MasterDataLookupMapper
{
    /// <summary>
    /// Builds the resolved lookup ids needed to apply one room field payload.
    /// </summary>
    /// <param name="fields">The submitted room fields.</param>
    /// <param name="masterData">The resolved master-data values for the current mutation.</param>
    /// <param name="includeAvailableStatus">Whether to include the default available status for a new room.</param>
    /// <returns>The room field lookup model.</returns>
    public static RoomFieldLookupModel BuildRoomFieldLookups(
        RoomFieldUpdateModel fields,
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> masterData,
        bool includeAvailableStatus)
    {
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(masterData);

        // Only submitted code fields need lookup IDs; omitted fields stay untouched during partial update.
        return new RoomFieldLookupModel
        {
            UnitTypeId = fields.TypeCode is null
                ? null
                : masterData.GetValue(MasterDataTypeEnum.UnitType, fields.TypeCode).Id,
            RentalModeId = fields.RentalModeCode is null
                ? null
                : masterData.GetValue(MasterDataTypeEnum.UnitRentalMode, fields.RentalModeCode).Id,
            AvailableStatusId = includeAvailableStatus
                ? masterData.GetValue(MasterDataTypeEnum.UnitStatus, MASTER_CODE_UNIT_STATUS_AVAILABLE).Id
                : null
        };
    }

    /// <summary>
    /// Builds the resolved lookup ids needed to apply one rental charge policy payload.
    /// </summary>
    /// <param name="input">The submitted charge policy input.</param>
    /// <param name="masterData">The resolved master-data values for the current mutation.</param>
    /// <param name="includeActiveStatus">Whether the mutation creates a new policy that needs the active status.</param>
    /// <returns>The charge policy lookup model.</returns>
    public static RentalChargePolicyLookupModel BuildChargePolicyLookups(
        IChargePolicyInput input,
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> masterData,
        bool includeActiveStatus)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(masterData);

        // Partial rows resolve only submitted combobox codes; omitted codes leave existing FKs unchanged.
        return new RentalChargePolicyLookupModel
        {
            LineTypeId = input.Code is null
                ? null
                : masterData.GetValue(MasterDataTypeEnum.InvoiceLineType, input.Code).Id,
            VehicleTypeId = input.VehicleTypeCode is null
                ? null
                : masterData.GetValue(MasterDataTypeEnum.VehicleType, input.VehicleTypeCode).Id,
            StatusId = includeActiveStatus
                ? masterData.GetValue(MasterDataTypeEnum.CommonStatus, MASTER_CODE_ACTIVE).Id
                : null
        };
    }

    /// <summary>
    /// Builds the package lookup values shared by property and room package mutations.
    /// </summary>
    /// <param name="masterData">The resolved master-data values for the current mutation.</param>
    /// <param name="includeCustomType">Whether the current mutation creates a custom package.</param>
    /// <returns>The package lookup model used by persistence synchronization.</returns>
    public static UnitPackageMutationLookupModel BuildUnitPackageLookups(
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> masterData,
        bool includeCustomType)
    {
        ArgumentNullException.ThrowIfNull(masterData);

        // Package mutation methods receive only their required typed values, not the broad lookup dictionary.
        return new UnitPackageMutationLookupModel
        {
            NoFurniturePackageType = masterData.GetValue(
                MasterDataTypeEnum.UnitPackageType,
                MASTER_CODE_UNIT_PACKAGE_TYPE_NO_FURNITURE),
            CustomPackageTypeId = includeCustomType
                ? masterData.GetValue(
                    MasterDataTypeEnum.UnitPackageType,
                    MASTER_CODE_UNIT_PACKAGE_TYPE_CUSTOM).Id
                : default,
            ActiveStatusId = masterData.GetValue(
                MasterDataTypeEnum.UnitPackageStatus,
                MASTER_CODE_UNIT_PACKAGE_STATUS_ACTIVE).Id
        };
    }
}
