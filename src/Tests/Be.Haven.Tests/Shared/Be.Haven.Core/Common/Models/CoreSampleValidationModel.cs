namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Models;

internal sealed class CoreSampleValidationModel
{
    public string RequiredText { get; set; }

    public string Email { get; set; }

    public string Url { get; set; }

    public string RegexValue { get; set; }

    public Guid PublicId { get; set; }

    public int Count { get; set; }

    public int OtherCount { get; set; }

    public int? OptionalCount { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public DateTime? OptionalStartAt { get; set; }

    public DateTime? OptionalEndAt { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public List<string> Items { get; set; }

    public CoreSampleStatusEnum Status { get; set; }

    public CoreSampleStatusEnum? OptionalStatus { get; set; }

    public string StatusCsv { get; set; }
}
