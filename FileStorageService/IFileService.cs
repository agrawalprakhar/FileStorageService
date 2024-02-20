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
        Task UploadFileAsync(FileModel file);

        /// <summary>
        /// Retrieves the content of a file as a string asynchronously.
        /// </summary>
        /// <param name="file">The file model representing the file to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the content of the file as a string.</returns>
        Task<string> GetFileAsStringAsync(FileModel file);

        /// <summary>
        /// Deletes a file asynchronously.
        /// </summary>
        /// <param name="file">The file model representing the file to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteFileAsync(FileModel file);

        /// <summary>
        /// Generates a pre-signed URL for downloading the specified file from the Amazon S3 bucket with the provided expiration time.
        /// </summary>
        /// <param name="file">The file model representing the file for which to generate the pre-signed URL.</param>
        /// <param name="expiration">The expiration time for the pre-signed URL.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the pre-signed URL.</returns>
        Task<string> GetSignedUrlAsync(FileModel file, TimeSpan expiration);
    }
}
