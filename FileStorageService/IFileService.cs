using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads a file asynchronously.
        /// </summary>
        /// <param name="file">The file model representing the file to upload.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UploadFileAsync(FileModelBase file);

        /// <summary>
        /// Retrieves the content of a file as a string asynchronously.
        /// </summary>
        /// <param name="file">The file model representing the file to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the content of the file as a string.</returns>
        Task<string> GetFileAsStringAsync(FileModelBase file);

        /// <summary>
        /// Deletes a file asynchronously.
        /// </summary>
        /// <param name="file">The file model representing the file to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteFileAsync(FileModelBase file);

        /// <summary>
        /// Generates a pre-signed URL for downloading the specified file with the provided expiration time.
        /// </summary>
        /// <param name="file">The file model representing the file for which to generate the pre-signed URL.</param>
        /// <param name="expiration">The expiration time for the pre-signed URL.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the pre-signed URL.</returns>
        Task<string> GetSignedUrlAsync(FileModelBase file, TimeSpan expiration);

        /// <summary>
        /// Retrieves a list of keys asynchronously based on the provided request.
        /// </summary>
        /// <param name="request">The request object containing parameters for retrieving keys.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of keys.</returns>
        Task<List<string>> GetKeysAsync(GetAllKeysRequest request);
    }
}
