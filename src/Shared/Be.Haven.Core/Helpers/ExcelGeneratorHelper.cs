namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides helper methods for generating Excel files dynamically.
/// </summary>
public static class ExcelGeneratorHelper
{
    /// <summary>
    /// Generates an Excel file based on the provided table structure.
    /// </summary>
    /// <param name="tableData">The structured table data containing title, headers, and rows to be exported.</param>
    /// <param name="fileName">The name of the file to be generated.</param>
    /// <param name="contentType">The MIME content type of the file (e.g., "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet").</param>
    /// <returns>
    /// A <see cref="FileDownLoadResponseDto"/> object containing the generated Excel file bytes,
    /// filename, and content type for file download.
    /// </returns>
    public static FileDownLoadResponseDto GenerateExcel(TableStructureDto tableData, string fileName, string contentType)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(tableData.Title);

        // Headers
        for (int i = 0; i < tableData.Headers.Length; i++)
        {
            var cell = sheet.Cell(1, i + 1);
            cell.Value = tableData.Headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        if (tableData.Rows.Count > 0) {
            // Data rows
            for (int rowIndex = 0; rowIndex < tableData.Rows.Count; rowIndex++)
            {
                var rowData = tableData.Rows[rowIndex];
                for (int colIndex = 0; colIndex < rowData.Length; colIndex++)
                {
                    var cell = sheet.Cell(rowIndex + 2, colIndex + 1);
                    cell.Value = rowData[colIndex];
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    // Format numbers
                    if (colIndex == 3) // Price
                    {
                        cell.Style.NumberFormat.Format = "#,##0.00";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    // Zebra striping
                    if (rowIndex % 2 == 0)
                    {
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 245);
                    }

                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
            }

            // Auto-fit columns
            sheet.Columns().AdjustToContents();
        }        

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new FileDownLoadResponseDto(
            stream.ToArray(),
            fileName,
            contentType
        );
    }

    /// <summary>
    /// Converts a byte array into an <see cref="IFormFile"/> object for use in file uploads or HTTP requests.
    /// </summary>
    /// <param name="content">The byte array representing the content of the file.</param>
    /// <param name="fileName">The name of the file, including the extension.</param>
    /// <param name="contentType">The MIME content type of the file (default is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet").</param>
    /// <returns>
    /// An <see cref="IFormFile"/> object created from the provided byte array, with the specified file name and content type.
    /// </returns>
    public static IFormFile ToFormFile(
        byte[] content,
        string fileName,
        string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
    {    
        var stream = new MemoryStream(content);
        stream.Position = 0;

        // name: field name (form field), fileName: tên file thật
        var file = new FormFile(stream, 0, content.Length, name: "file", fileName: fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        return file;
    }
}
