namespace Be.Haven.Core.Helpers;

/// <summary>
/// Provides helper methods for generating Excel files dynamically.
/// </summary>
public static class ExcelGeneratorHelper
{
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
