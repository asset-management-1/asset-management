namespace Be.Haven.Core.Files;

public abstract class ExcelReportGeneratorBase<T> : IExcelReportGenerator<T>
{
    private readonly IExcelDocumentFactory _factory;

    /// <summary>
    /// Provides a base implementation for generating Excel reports using a specified data model.
    /// </summary>
    /// <typeparam name="T">The type of the data model used to populate the report.</typeparam>
    protected ExcelReportGeneratorBase(IExcelDocumentFactory factory) => _factory = factory;

    /// <summary>
    /// Gets the unique key used to identify and retrieve an existing Excel report template
    /// associated with the implementation of the generator.
    /// The key serves as a reference to locate and open the appropriate template
    /// for configuration and data population during report generation.
    /// </summary>
    public abstract string ReportKey { get; }

    /// <summary>
    /// Configures the Excel document prior to data population, allowing for any necessary setup
    /// such as selecting sheets, applying styles, or initializing custom configurations.
    /// </summary>
    /// <param name="doc">
    /// The Excel document instance to be configured.
    /// </param>
    /// <param name="model">
    /// The data model providing context or input values for the configuration process.
    /// </param>
    protected virtual void Configure(IExcelDocument doc, T model)
    {
    }

    /// <summary>
    /// Fills the given Excel document with data based on the provided model.
    /// </summary>
    /// <param name="doc">
    /// The Excel document to populate.
    /// </param>
    /// <param name="model">
    /// The data model used to populate the document.
    /// </param>
    protected abstract void Fill(IExcelDocument doc, T model);

    /// <summary>
    /// Generates an Excel report based on the provided model.
    /// </summary>
    /// <param name="model">The model containing the data to populate the report.</param>
    /// <param name="ct">An optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated report as a byte array.</returns>
    public async Task<byte[]> GenerateAsync(T model, CancellationToken ct = default)
    {
        using var doc = await _factory.OpenAsync(ReportKey, ct);

        Configure(doc, model);
        Fill(doc, model);

        return doc.Build();
    }
}