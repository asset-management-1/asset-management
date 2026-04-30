namespace Be.Haven.Core.Models.FileResponse;

/// <summary>
/// Represents a table structure used for exporting or displaying tabular data.
/// </summary>
public class TableStructureDto
{
    /// <summary>
    /// Gets or sets the collection of column headers for the table.
    /// </summary>
    public string[] Headers { get; set; }

    /// <summary>
    /// Gets or sets the list of data rows, 
    /// where each row is represented as an array of string values.
    /// </summary>
    public List<string[]> Rows { get; set; }

    /// <summary>
    /// Gets or sets the title of the table, 
    /// typically used as a heading or caption.
    /// </summary>
    public string Title { get; set; }
}
