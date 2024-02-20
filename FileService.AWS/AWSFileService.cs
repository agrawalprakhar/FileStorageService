using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.AWS
{
    public class AWSFileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;

        public AWSFileService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        }

        public async Task UploadFileAsync(FileModel file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(file.FilePath, file.BucketName, file.KeyName);
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the Amazon S3 bucket.
        /// </summary>
        /// <param name="file">The file model representing the file to check.</param>
        /// <returns>True if the file exists in the bucket; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> is null.</exception>
        /// <exception cref="AmazonS3Exception">Thrown when an error occurs while communicating with Amazon S3.</exception>
        public async Task<bool> DoesFileExistAsync(FileModel file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = file.BucketName,
                    Key = file.KeyName
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }


        public async Task<string> GetFileAsStringAsync(FileModel file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {


                var request = new GetObjectRequest
                {
                    BucketName = file.BucketName,
                    Key = file.KeyName
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);

                    byte[] bytes = memoryStream.ToArray();

                    string fileContent = Encoding.UTF8.GetString(bytes);

                    Console.WriteLine(fileContent);

                    return fileContent;
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }
        public async Task<string> GetSignedUrlAsync(FileModel file, TimeSpan expiration)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {
                // Check if the file exists in the S3 bucket
                bool fileExists = await DoesFileExistAsync(file);
                if (!fileExists)
                {
                    throw new InvalidOperationException($"File '{file.KeyName}' does not exist in the bucket '{file.BucketName}' or the bucket does not exist.");
                }

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = file.BucketName,
                    Key = file.KeyName,
                    Expires = DateTime.UtcNow.Add(expiration)
                };

                string url = _s3Client.GetPreSignedURL(request);
                Console.WriteLine(url);
                return url;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }


        public async Task DeleteFileAsync(FileModel file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            try
            {
                // Check if the file exists in the S3 bucket
                bool fileExists = await DoesFileExistAsync(file);
                if (!fileExists)
                {
                    throw new InvalidOperationException($"File '{file.KeyName}' does not exist in the bucket '{file.BucketName}' or the bucket does not exist.");
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = file.BucketName,
                    Key = file.KeyName
                };

                await _s3Client.DeleteObjectAsync(request);
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

    }
}
