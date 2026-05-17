namespace Be.Haven.Core.Interfaces.Excel;

/// <summary>
/// Abstraction for building an Excel document from a template.
/// </summary>
public interface IExcelDocument : IDisposable
{
    /// <summary>
    /// Selects the active worksheet by name.
    /// </summary>
    /// <param name="name">Worksheet name.</param>
    void UseSheet(string name);

    /// <summary>
    /// Selects the active worksheet by 1-based index.
    /// </summary>
    /// <param name="index">Worksheet index (>= 1).</param>
    void UseSheet(int index);

    /// <summary>
    /// Sets value for a defined name with optional cell styling.
    /// </summary>
    /// <param name="name">Defined name (named cell/range).</param>
    /// <param name="value">Value to write.</param>
    /// <param name="style">Optional cell style configuration.</param>
    void Set(string name, object value, Action<IXLCell> style = null);

    /// <summary>
    /// Fills a table starting from an anchor defined name.
    /// </summary>
    /// <typeparam name="T">Row data type.</typeparam>
    /// <param name="anchorName">Anchor defined name.</param>
    /// <param name="items">Items to write.</param>
    /// <param name="mapRow">Maps item values to row cells.</param>
    /// <param name="insertRows">Insert rows if needed.</param>
    /// <param name="templateRowOffset">Template row offset from anchor.</param>
    /// <param name="clearEndCol">Last column to clear.</param>
    /// <param name="rowStyleOverride">Optional row style override.</param>
    void FillTable<T>(
        string anchorName,
        IReadOnlyList<T> items,
        Action<IExcelRow<T>> mapRow,
        bool insertRows = true,
        int templateRowOffset = 0,
        int? clearEndCol = null,
        Action<T, IXLRangeRow> rowStyleOverride = null);

    /// <summary>
    /// Builds and returns the Excel file bytes.
    /// </summary>
    /// <returns>Excel file content.</returns>
    byte[] Build();

    /// <summary>
    /// Adjusts the width of all used columns in the active worksheet to fit their content.
    /// Optionally adjusts the heights of all used rows to fit their content as well.
    /// </summary>
    /// <param name="includeRows">
    /// Indicates whether to adjust the row heights in addition to column widths.
    /// </param>
    void AutoFitUsed(bool includeRows = false);
    
    /// <summary>
    /// Adjusts the width of columns in the specified range to fit their content.
    /// Optionally adjusts the height of rows containing used cells within the range to fit their content as well.
    /// </summary>
    /// <param name="fromCol">
    /// The starting column index of the range to be adjusted.
    /// </param>
    /// <param name="toCol">
    /// The ending column index of the range to be adjusted.
    /// </param>
    /// <param name="includeRows">
    /// A flag indicating whether to adjust the height of rows containing used cells within the specified column range.
    /// </param>
    void AutoFitColumns(int fromCol, int toCol, bool includeRows = false);
}
