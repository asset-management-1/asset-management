namespace Be.Haven.Core.Interfaces.Excel;

public interface IExcelDocumentFactory
{
    /// <summary>
    /// Opens an editable Excel document from a template source.
    /// </summary>
    /// <param name="sourceKey">
    /// Logical key used to locate the Excel template resource.
    /// </param>
    /// <param name="ct">
    /// Cancellation token used to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains an instance of <see cref="IExcelDocument"/> backed by the resolved template.
    /// </returns>
    Task<IExcelDocument> OpenAsync(string sourceKey, CancellationToken ct = default);
}