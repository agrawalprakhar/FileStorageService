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

        public async Task UploadFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));



            if (!(file is S3FileModel s3File))
                throw new ArgumentException("File model is not of type S3FileModel");

            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(s3File.FilePath, s3File.BucketName, s3File.KeyName);
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
        public async Task<bool> DoesFileExistAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is S3FileModel s3File))
                throw new ArgumentException("File model is not of type S3FileModel");

            try
            {

                var request = new GetObjectMetadataRequest
                {
                    BucketName = s3File.BucketName,
                    Key = s3File.KeyName
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

        public async Task<string> GetFileAsStringAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is S3FileModel s3File))
                throw new ArgumentException("File model is not of type S3FileModel");

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = s3File.BucketName,
                    Key = s3File.KeyName
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);

                    byte[] bytes = memoryStream.ToArray();

                    string fileContent = Encoding.UTF8.GetString(bytes);

                    return fileContent;
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }
        public async Task<string> GetSignedUrlAsync(FileModelBase file, TimeSpan expiration)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is S3FileModel s3File))
                throw new ArgumentException("File model is not of type S3FileModel");

            try
            {
                // Check if the file exists in the S3 bucket
                bool fileExists = await DoesFileExistAsync(s3File);
                if (!fileExists)
                {
                    throw new InvalidOperationException($"File '{s3File.KeyName}' does not exist in the bucket '{s3File.BucketName}' Else the bucket does not exist.");
                }

                var request = new GetPreSignedUrlRequest
                {
                    BucketName = s3File.BucketName,
                    Key = s3File.KeyName,
                    Expires = DateTime.UtcNow.Add(expiration)
                };

                string url = _s3Client.GetPreSignedURL(request);

                return url;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is S3FileModel s3File))
                throw new ArgumentException("File model is not of type S3FileModel");

            try
            {
                // Check if the file exists in the S3 bucket
                bool fileExists = await DoesFileExistAsync(s3File);
                if (!fileExists)
                {
                    throw new InvalidOperationException($"File '{s3File.KeyName}' does not exist in the bucket '{s3File.BucketName}' or the bucket does not exist.");
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = s3File.BucketName,
                    Key = s3File.KeyName
                };

                await _s3Client.DeleteObjectAsync(request);
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        public async Task<List<string>> GetKeysAsync(GetAllKeysRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.BucketOrContainer == null)
                throw new ArgumentNullException(nameof(request.BucketOrContainer));


            if (request.PageNumber <= 0 || request.PageSize <= 0)
                throw new ArgumentException("Page number and page size must be greater than zero");

            var keys = new List<string>();

            try
            {
                string continuationToken = null;
                int itemsProcessed = 0;
                int itemsToSkip = (request.PageNumber - 1) * request.PageSize;

                do
                {

                    var listObjectsRequest = new ListObjectsV2Request
                    {
                        BucketName = request.BucketOrContainer,
                        MaxKeys = request.PageSize,
                        ContinuationToken = continuationToken
                    };

                    var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);

                    foreach (var obj in response.S3Objects)
                    {

                        if (itemsProcessed >= itemsToSkip)
                        {
                            keys.Add(obj.Key);

                            if (keys.Count >= request.PageSize)
                                break;
                        }

                        itemsProcessed++;
                    }
                    continuationToken = response.NextContinuationToken;

                } while (!string.IsNullOrEmpty(continuationToken) && keys.Count < request.PageSize);

                return keys;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving keys from the S3 bucket.", ex);
            }
        }

    }
}
