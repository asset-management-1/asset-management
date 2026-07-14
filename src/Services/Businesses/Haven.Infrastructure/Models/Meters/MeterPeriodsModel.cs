namespace Haven.Infrastructure.Models.Meters;

/// <summary>
/// Contains the selected and preceding calendar boundaries used by one room meter workflow.
/// </summary>
/// <param name="CurrentFrom">The first date of the selected period.</param>
/// <param name="CurrentTo">The final date of the selected period.</param>
/// <param name="PreviousFrom">The first date of the preceding period.</param>
/// <param name="PreviousTo">The final date of the preceding period.</param>
public sealed record MeterPeriodsModel(
    DateOnly CurrentFrom,
    DateOnly CurrentTo,
    DateOnly PreviousFrom,
    DateOnly PreviousTo)
{
    /// <summary>
    /// Creates selected and preceding calendar boundaries from a validated date.
    /// </summary>
    /// <param name="date">The date identifying the selected billing month.</param>
    /// <returns>The selected and preceding calendar periods.</returns>
    public static MeterPeriodsModel From(DateTime date)
    {
        return From(date.Year, date.Month);
    }

    /// <summary>
    /// Creates selected and preceding calendar boundaries from a validated year and month.
    /// </summary>
    /// <param name="year">The selected calendar year.</param>
    /// <param name="month">The selected calendar month.</param>
    /// <returns>The selected and preceding calendar periods.</returns>
    public static MeterPeriodsModel From(int year, int month)
    {
        var currentFrom = new DateOnly(year, month, 1);
        var previousFrom = currentFrom.AddMonths(-1);

        return new MeterPeriodsModel(
            currentFrom,
            currentFrom.AddMonths(1).AddDays(-1),
            previousFrom,
            previousFrom.AddMonths(1).AddDays(-1));
    }
}
