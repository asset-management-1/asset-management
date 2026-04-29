namespace Haven.Core.Interfaces.Excel;

public interface IExcelRow<out T>
{
    /// <summary>
    /// Represents an individual item associated with a row in an Excel worksheet.
    /// This property is typically used to store or retrieve the data model object
    /// corresponding to a specific row.
    /// </summary>
    T Item { get; }

    /// <summary>
    /// Sets a cell value relative to the specified anchor column with optional styling.
    /// </summary>
    /// <param name="colOffset">The zero-based column offset relative to the starting column.</param>
    /// <param name="value">The value to assign to the cell. Can be null.</param>
    /// <param name="style">An optional action to apply custom styling to the cell. Can be null.</param>
    public void Set(int colOffset, object value, Action<IXLCell> style = null);
}