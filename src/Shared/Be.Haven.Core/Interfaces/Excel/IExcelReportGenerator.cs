namespace Be.Haven.Core.Interfaces.Excel;

public interface IExcelReportGenerator<in T>
{
    /// <summary>
    /// Gets the key used to identify the specific Excel report template.
    /// </summary>
    string ReportKey { get; }

    /// <summary>
    /// Generates an Excel report asynchronously based on the provided model.
    /// </summary>
    /// <param name="model">The data model used to generate the report.</param>
    /// <param name="ct">A cancellation token to observe during the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated report as a byte array.</returns>
    Task<byte[]> GenerateAsync(T model, CancellationToken ct = default);
}