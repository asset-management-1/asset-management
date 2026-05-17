namespace Be.Haven.Core.Models.ObjectStorage;

/// <summary>
/// Represents uploaded object metadata keyed by logical batch slot.
/// </summary>
public class ObjectStorageUploadBatchResultModel
{
    /// <summary>
    /// Gets or sets the uploaded objects keyed by slot name.
    /// </summary>
    public Dictionary<string, ObjectUploadResponseModel> UploadsBySlot { get; set; } = [];

    /// <summary>
    /// Gets the uploaded object for one slot.
    /// </summary>
    /// <param name="slotName">The slot name to read.</param>
    /// <returns>The uploaded object metadata, or <c>null</c> when the slot was not uploaded.</returns>
    public ObjectUploadResponseModel GetUpload(string slotName)
    {
        // Return null for absent optional slots so callers can keep simple fallback logic.
        return UploadsBySlot.TryGetValue(slotName, out var upload) ? upload : null;
    }
}
