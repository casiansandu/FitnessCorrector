namespace FitnessCorrector.Application.Abstractions;

public interface ICloudStorageService
{
    /// <summary>
    /// Uploads a file to Cloud Storage
    /// </summary>
    /// <param name="stream">File stream to upload</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public URL of the uploaded file</returns>
    Task<string> UploadFileAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from Cloud Storage
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream of the downloaded file</returns>
    Task<Stream> DownloadFileAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from Cloud Storage
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a signed URL for temporary access to a file
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    /// <returns>Signed URL</returns>
    Task<string> GetSignedUrlAsync(string fileName, int expirationMinutes = 60);
}
