namespace Haven.Infrastructure.Models.Meters;

/// <summary>
/// Holds the resolved master-data values required by one room meter mutation.
/// </summary>
public sealed class MeterMutationLookupModel
{
    private static readonly IReadOnlyCollection<MasterDataKeyModel> MutationKeys =
    [
        new(MasterDataTypeEnum.InvoiceLineType, MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC),
        new(MasterDataTypeEnum.InvoiceLineType, MASTER_CODE_INVOICE_LINE_TYPE_WATER),
        new(MasterDataTypeEnum.MeterStatus, MASTER_CODE_METER_STATUS_CONFIRMED),
        new(MasterDataTypeEnum.UtilityChargeMode, MASTER_CODE_UTILITY_CHARGE_MODE_METER_READING),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_DRAFT),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_ISSUED),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_OVERDUE),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_PAID),
        new(MasterDataTypeEnum.InvoiceStatus, MASTER_CODE_INVOICE_STATUS_CANCELLED)
    ];

    private static readonly IReadOnlyCollection<MasterDataKeyModel> EvidenceKeys =
    [
        new(MasterDataTypeEnum.DocumentType, MASTER_CODE_DOCUMENT_TYPE_METER_PHOTO),
        new(MasterDataTypeEnum.StorageProvider, MASTER_CODE_STORAGE_PROVIDER_R2),
        new(MasterDataTypeEnum.DocumentStatus, MASTER_CODE_DOCUMENT_STATUS_ACTIVE),
        new(MasterDataTypeEnum.EntityType, MASTER_CODE_ENTITY_TYPE_METER),
        new(MasterDataTypeEnum.DocumentLinkType, MASTER_CODE_DOCUMENT_LINK_TYPE_METER_PHOTO),
        new(MasterDataTypeEnum.DocumentLinkStatus, MASTER_CODE_DOCUMENT_LINK_STATUS_ACTIVE)
    ];

    private static readonly IReadOnlyCollection<MasterDataKeyModel> MutationWithEvidenceKeys =
    [
        .. MutationKeys,
        .. EvidenceKeys
    ];

    /// <summary>
    /// Gets or sets the electricity invoice line type.
    /// </summary>
    public MasterDataValueModel ElectricLineType { get; init; }

    /// <summary>
    /// Gets or sets the water invoice line type.
    /// </summary>
    public MasterDataValueModel WaterLineType { get; init; }

    /// <summary>
    /// Gets or sets the confirmed meter status.
    /// </summary>
    public MasterDataValueModel ConfirmedMeterStatus { get; init; }

    /// <summary>
    /// Gets or sets the meter-reading utility charge mode.
    /// </summary>
    public MasterDataValueModel MeterReadingChargeMode { get; init; }

    /// <summary>
    /// Gets or sets the draft invoice status.
    /// </summary>
    public MasterDataValueModel DraftInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets the issued invoice status.
    /// </summary>
    public MasterDataValueModel IssuedInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets the overdue invoice status.
    /// </summary>
    public MasterDataValueModel OverdueInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets the partially-paid invoice status.
    /// </summary>
    public MasterDataValueModel PartiallyPaidInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets the paid invoice status.
    /// </summary>
    public MasterDataValueModel PaidInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets the cancelled invoice status.
    /// </summary>
    public MasterDataValueModel CancelledInvoiceStatus { get; init; }

    /// <summary>
    /// Gets or sets optional evidence persistence values.
    /// </summary>
    public MeterEvidenceLookupModel Evidence { get; init; }

    /// <summary>
    /// Returns the exact master-data keys required by a meter mutation and its optional evidence phase.
    /// </summary>
    /// <param name="includeEvidence">Whether evidence document and link values are required.</param>
    /// <returns>The deduplicated keys consumed by this typed lookup model.</returns>
    public static IReadOnlyCollection<MasterDataKeyModel> GetRequiredKeys(bool includeEvidence)
    {
        return includeEvidence
            ? MutationWithEvidenceKeys
            : MutationKeys;
    }

    /// <summary>
    /// Converts a resolved master-data dictionary into the named values used by the meter workflow.
    /// </summary>
    /// <param name="values">The exact values loaded for this mutation.</param>
    /// <param name="includeEvidence">Whether evidence values must be included.</param>
    /// <returns>The typed meter mutation lookup model.</returns>
    public static MeterMutationLookupModel Create(
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> values,
        bool includeEvidence)
    {
        // Keep dictionary access at this boundary so the service and downstream phases use named values only.
        return new MeterMutationLookupModel
        {
            ElectricLineType = values.GetValue(
                MasterDataTypeEnum.InvoiceLineType,
                MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC),
            WaterLineType = values.GetValue(
                MasterDataTypeEnum.InvoiceLineType,
                MASTER_CODE_INVOICE_LINE_TYPE_WATER),
            ConfirmedMeterStatus = values.GetValue(
                MasterDataTypeEnum.MeterStatus,
                MASTER_CODE_METER_STATUS_CONFIRMED),
            MeterReadingChargeMode = values.GetValue(
                MasterDataTypeEnum.UtilityChargeMode,
                MASTER_CODE_UTILITY_CHARGE_MODE_METER_READING),
            DraftInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_DRAFT),
            IssuedInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_ISSUED),
            OverdueInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_OVERDUE),
            PartiallyPaidInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID),
            PaidInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_PAID),
            CancelledInvoiceStatus = values.GetValue(
                MasterDataTypeEnum.InvoiceStatus,
                MASTER_CODE_INVOICE_STATUS_CANCELLED),
            Evidence = includeEvidence ? MeterEvidenceLookupModel.Create(values) : null
        };
    }

    /// <summary>
    /// Returns the resolved line type for one fixed Meter V1 utility code.
    /// </summary>
    /// <param name="lineTypeCode">The electricity or water code.</param>
    /// <returns>The matching line type.</returns>
    public MasterDataValueModel GetLineType(string lineTypeCode)
    {
        return lineTypeCode switch
        {
            MASTER_CODE_INVOICE_LINE_TYPE_ELECTRIC => ElectricLineType,
            MASTER_CODE_INVOICE_LINE_TYPE_WATER => WaterLineType,
            _ => throw new ArgumentOutOfRangeException(nameof(lineTypeCode), lineTypeCode, "Unsupported meter line type.")
        };
    }
}

/// <summary>
/// Holds master-data values required only when meter evidence is uploaded.
/// </summary>
public sealed class MeterEvidenceLookupModel
{
    /// <summary>
    /// Gets or sets the meter-photo document type.
    /// </summary>
    public MasterDataValueModel DocumentType { get; init; }

    /// <summary>
    /// Gets or sets the R2 storage provider.
    /// </summary>
    public MasterDataValueModel StorageProvider { get; init; }

    /// <summary>
    /// Gets or sets the active document status.
    /// </summary>
    public MasterDataValueModel ActiveDocumentStatus { get; init; }

    /// <summary>
    /// Gets or sets the meter entity type.
    /// </summary>
    public MasterDataValueModel MeterEntityType { get; init; }

    /// <summary>
    /// Gets or sets the meter-photo document link type.
    /// </summary>
    public MasterDataValueModel MeterPhotoLinkType { get; init; }

    /// <summary>
    /// Gets or sets the active document link status.
    /// </summary>
    public MasterDataValueModel ActiveLinkStatus { get; init; }

    /// <summary>
    /// Converts resolved evidence values into the named document persistence contract.
    /// </summary>
    /// <param name="values">The values loaded for the evidence phase.</param>
    /// <returns>The typed evidence lookup model.</returns>
    public static MeterEvidenceLookupModel Create(
        IReadOnlyDictionary<MasterDataKeyModel, MasterDataValueModel> values)
    {
        return new MeterEvidenceLookupModel
        {
            DocumentType = values.GetValue(
                MasterDataTypeEnum.DocumentType,
                MASTER_CODE_DOCUMENT_TYPE_METER_PHOTO),
            StorageProvider = values.GetValue(
                MasterDataTypeEnum.StorageProvider,
                MASTER_CODE_STORAGE_PROVIDER_R2),
            ActiveDocumentStatus = values.GetValue(
                MasterDataTypeEnum.DocumentStatus,
                MASTER_CODE_DOCUMENT_STATUS_ACTIVE),
            MeterEntityType = values.GetValue(
                MasterDataTypeEnum.EntityType,
                MASTER_CODE_ENTITY_TYPE_METER),
            MeterPhotoLinkType = values.GetValue(
                MasterDataTypeEnum.DocumentLinkType,
                MASTER_CODE_DOCUMENT_LINK_TYPE_METER_PHOTO),
            ActiveLinkStatus = values.GetValue(
                MasterDataTypeEnum.DocumentLinkStatus,
                MASTER_CODE_DOCUMENT_LINK_STATUS_ACTIVE)
        };
    }
}
