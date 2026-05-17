namespace Be.Haven.Tests.Shared.Be.Haven.Core.Common.Fakes;

internal sealed class FakeObjectStorageService : IObjectStorageService
{
    public List<ObjectUploadRequestModel> UploadRequests { get; } = [];

    public List<string> DeleteRequests { get; } = [];

    public Func<ObjectUploadRequestModel, CancellationToken, Task<ObjectUploadResponseModel>> UploadHandler { get; set; }

    public Func<string, CancellationToken, Task<bool>> DeleteHandler { get; set; }

    public Task<ObjectUploadResponseModel> UploadAsync(
        ObjectUploadRequestModel request,
        CancellationToken cancellationToken = default)
    {
        UploadRequests.Add(request);

        if (UploadHandler is not null)
        {
            return UploadHandler(request, cancellationToken);
        }

        return Task.FromResult(new ObjectUploadResponseModel
        {
            BucketName = "fake-bucket",
            ObjectKey = request.ObjectKey,
            ContentType = request.ContentType,
            FileSize = request.Content.Length,
            Checksum = "fake-checksum"
        });
    }

    public Task<bool> DeleteAsync(
        string objectKey,
        CancellationToken cancellationToken = default)
    {
        DeleteRequests.Add(objectKey);

        return DeleteHandler is not null
            ? DeleteHandler(objectKey, cancellationToken)
            : Task.FromResult(true);
    }
}
