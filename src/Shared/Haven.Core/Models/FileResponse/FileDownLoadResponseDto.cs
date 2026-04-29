namespace Haven.Core.Models.FileResponse;

/// <summary>
/// Represents a Data Transfer Object (DTO) used to encapsulate file download response data.
/// </summary>
public class FileDownLoadResponseDto
{
    /// <summary>
    /// Gets the binary content of the file to be downloaded.
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// Gets the name of the file, including its extension.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets the MIME type of the file (e.g., "application/pdf", "application/vnd.ms-excel").
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownLoadResponseDto"/> class
    /// with the specified file content, file name, and content type.
    /// </summary>
    /// <param name="content">The byte array representing the file content.</param>
    /// <param name="fileName">The name of the file to be downloaded.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    public FileDownLoadResponseDto(byte[] content, string fileName, string contentType)
    {
        Content = content;
        FileName = fileName;
        ContentType = contentType;
    }
}
