namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides helper methods to generate PDF reports with tabular data.
/// Includes functionality for drawing table headers, rows, borders, 
/// calculating proportional column widths, and exporting to a PDF file.
/// </summary>
public static class PdfGeneratorHelper
{
    /// <summary>
    /// Draws the table header row in a PDF document.
    /// </summary>
    /// <param name="gfx">Graphics context used for drawing.</param>
    /// <param name="headers">Array of column header texts.</param>
    /// <param name="colWidths">Array of column widths.</param>
    /// <param name="y">Reference to the current Y-coordinate on the page (updated after drawing).</param>
    /// <param name="margin">Left margin of the table.</param>
    /// <param name="font">Font used for the header text.</param>
    public static void DrawPdfTableHeader(XGraphics gfx, string[] headers, double[] colWidths,
        ref double y, double margin, XFont font)
    {
        double headerY = y;

        // Background
        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 220, 220)),
            margin, headerY, colWidths.Sum(), 20);

        // Text
        double x = margin;
        for (int i = 0; i < headers.Length; i++)
        {
            gfx.DrawString(headers[i], font, XBrushes.Black,
                new XRect(x + 2, y + 2, colWidths[i] - 4, 15), XStringFormats.TopCenter);
            x += colWidths[i];
        }

        // Border
        DrawTableBorders(gfx, margin, headerY, colWidths, 35);
        y += 20;
    }

    /// <summary>
    /// Draws a single row of table data in the PDF document.
    /// </summary>
    /// <param name="gfx">Graphics context used for drawing.</param>
    /// <param name="rowData">Array of text values representing a row of data.</param>
    /// <param name="colWidths">Array of column widths.</param>
    /// <param name="y">Reference to the current Y-coordinate on the page (updated after drawing).</param>
    /// <param name="margin">Left margin of the table.</param>
    /// <param name="font">Font used for row text.</param>
    /// <param name="isEvenRow">Indicates whether this row is even (used for zebra striping).</param>
    public static void DrawPdfTableRow(XGraphics gfx, string[] rowData, double[] colWidths,
        ref double y, double margin, XFont font, bool isEvenRow)
    {
        double rowY = y;

        // Zebra striping
        if (isEvenRow)
        {
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(245, 245, 245)),
                margin, rowY, colWidths.Sum(), 18);
        }

        double x = margin;
        for (int i = 0; i < rowData.Length; i++)
        {
            var format = XStringFormats.TopCenter;


            gfx.DrawString(rowData[i], font, XBrushes.Black,
                new XRect(x + 2, y + 2, colWidths[i] - 4, 15), format);

            x += colWidths[i];
        }

        DrawTableBorders(gfx, margin, rowY, colWidths, 20);
        y += 20;
    }

    /// <summary>
    /// Calculates proportional column widths based on predefined weights.
    /// </summary>
    /// <param name="availableWidth">Total available width for the table.</param>
    /// <param name="columnCount">Number of columns in the table.</param>
    /// <returns>An array of calculated column widths proportional to the available space.</returns>
    public static double[] CalculateProportionalColumnWidths(double availableWidth, int columnCount)
    {
        double[] weights = { 0.5, 1.4, 1.0, 0.8, 0.7, 1.4, 1.2, 1.5, 1.1, 1.0 };
        double totalWeight = weights.Sum();
        double[] colWidths = new double[columnCount];

        for (int i = 0; i < columnCount; i++)
        {
            colWidths[i] = (weights[i] / totalWeight) * availableWidth;
        }

        return colWidths;
    }

    /// <summary>
    /// Draws horizontal and vertical borders for a table row or header.
    /// </summary>
    /// <param name="gfx">Graphics context used for drawing.</param>
    /// <param name="startX">Starting X-coordinate for the table.</param>
    /// <param name="startY">Starting Y-coordinate for the row or header.</param>
    /// <param name="colWidths">Array of column widths.</param>
    /// <param name="rowHeight">Height of the row.</param>
    private static void DrawTableBorders(XGraphics gfx, double startX, double startY,
        double[] colWidths, double rowHeight)
    {
        var pen = new XPen(XColors.Black, 0.5);

        // Horizontal lines
        gfx.DrawLine(pen, startX, startY, startX + colWidths.Sum(), startY);
        gfx.DrawLine(pen, startX, startY + rowHeight, startX + colWidths.Sum(), startY + rowHeight);

        // Vertical lines
        double x = startX;
        for (int i = 0; i <= colWidths.Length; i++)
        {
            gfx.DrawLine(pen, x, startY, x, startY + rowHeight);
            if (i < colWidths.Length)
                x += colWidths[i];
        }
    }

    /// <summary>
    /// Generates a complete PDF file from the provided table data.
    /// </summary>
    /// <param name="tableData">Structured table data containing title, headers, and rows.</param>
    /// <param name="fileName">Name of the resulting PDF file.</param>
    /// <param name="contentType">MIME type of the file (usually "application/pdf").</param>
    /// <returns>
    /// A <see cref="FileDownLoadResponseDto"/> containing the generated PDF file data.
    /// </returns>
    public static FileDownLoadResponseDto GeneratePdf(TableStructureDto tableData, string fileName, string contentType)
    {
        var document = new PdfDocument();
        document.Info.Title = tableData.Title;

        var page = document.AddPage();
        page.Orientation = PageOrientation.Landscape;
        var gfx = XGraphics.FromPdfPage(page);

        var fontHeader = new XFont("Arial", 14, XFontStyleEx.Bold);
        var fontCell = new XFont("Arial", 9, XFontStyleEx.Regular);
        var fontCellBold = new XFont("Arial", 9, XFontStyleEx.Bold);

        double margin = 40;
        double y = margin;

        // Title
        gfx.DrawString(tableData.Title, fontHeader, XBrushes.Black,
            new XRect(0, y, page.Width.Point, 20), XStringFormats.TopCenter);
        y += 40;

        // Calculate column widths
        double availableWidth = page.Width.Point - (margin * 2);
        double[] colWidths = CalculateProportionalColumnWidths(availableWidth, tableData.Headers.Length);

        // Draw headers
        DrawPdfTableHeader(gfx, tableData.Headers, colWidths, ref y, margin, fontCellBold);

        if (tableData.Rows.Count > 0)
        {
            // Draw rows
            int rowIndex = 0;
            foreach (var rowData in tableData.Rows)
            {
                // Check if need new page
                if (y > page.Height.Point - 80)
                {
                    page = document.AddPage();
                    page.Orientation = PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                    DrawPdfTableHeader(gfx, tableData.Headers, colWidths, ref y, margin, fontCellBold);
                }

                DrawPdfTableRow(gfx, rowData, colWidths, ref y, margin, fontCell, rowIndex % 2 == 0);
                rowIndex++;
            }
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);

        return new FileDownLoadResponseDto(
            stream.ToArray(),
            fileName,
            contentType
        );
    }
}
