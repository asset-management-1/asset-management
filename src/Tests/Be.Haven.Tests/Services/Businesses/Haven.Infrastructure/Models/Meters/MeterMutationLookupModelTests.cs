using Haven.Application.Models.MasterData;
using Haven.Infrastructure.Models.Meters;

namespace Be.Haven.Tests.Services.Businesses.Haven.Infrastructure.Models.Meters;

public sealed class MeterMutationLookupModelTests
{
    [Fact]
    public void GetRequiredKeys_Should_IncludeEveryInvoiceStatusUsedByTransactionalMutationGuard()
    {
        // Act
        var keys = MeterMutationLookupModel.GetRequiredKeys(false);

        // Assert
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_DRAFT));
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_ISSUED));
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_OVERDUE));
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID));
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_PAID));
        keys.Should().Contain(new MasterDataKeyModel(
            MasterDataTypeEnum.InvoiceStatus,
            MASTER_CODE_INVOICE_STATUS_CANCELLED));
    }

    [Fact]
    public void Create_Should_ExposeNamedInvoiceStatusesForLiveMutationDecisions()
    {
        // Arrange
        var keys = MeterMutationLookupModel.GetRequiredKeys(false);
        var nextId = 0L;
        var values = keys.ToDictionary(
            key => key,
            key => new MasterDataValueModel
            {
                Id = ++nextId,
                Code = key.Code
            });

        // Act
        var result = MeterMutationLookupModel.Create(values, false);

        // Assert
        result.DraftInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_DRAFT);
        result.IssuedInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_ISSUED);
        result.OverdueInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_OVERDUE);
        result.PartiallyPaidInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_PARTIALLY_PAID);
        result.PaidInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_PAID);
        result.CancelledInvoiceStatus.Code.Should().Be(MASTER_CODE_INVOICE_STATUS_CANCELLED);
    }
}
