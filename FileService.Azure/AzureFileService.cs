using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Azure
{
    public class AzureFileService : IFileService
    {

        private readonly BlobServiceClient _blobServiceClient;

        public AzureFileService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        public async Task<bool> DoesBlobExistAsync(string containerName, string blobName)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task UploadFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                await blobClient.UploadAsync(azureBlobFile.FilePath, true);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error uploading file to Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<string> GetFileAsStringAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                var response = await blobClient.DownloadAsync();

                using (var reader = new StreamReader(response.Value.Content))
                {
                    string fileContent = await reader.ReadToEndAsync();

                    return fileContent;
                }
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error getting file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task DeleteFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                // Check if the container and file exist
                bool blobExists = await DoesBlobExistAsync(azureBlobFile.ContainerName, azureBlobFile.KeyName);
                if (!blobExists)
                {
                    throw new InvalidOperationException($" The specified blob does not exist Else Blob '{azureBlobFile.KeyName}' does not exist in container '{azureBlobFile.ContainerName}'");
                }

                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error deleting file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<string> GetSignedUrlAsync(FileModelBase file, TimeSpan expiration)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                // Check if the container and file exist
                bool blobExists = await DoesBlobExistAsync(azureBlobFile.ContainerName, azureBlobFile.KeyName);
                if (!blobExists)
                {
                    throw new InvalidOperationException($"The specified blob does not exist Else Blob '{azureBlobFile.KeyName}' does not exist in container '{azureBlobFile.ContainerName}'");
                }

                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName);
                var blobClient = blobContainerClient.GetBlobClient(azureBlobFile.KeyName);

                // Generate the signed URL with the specified expiration time
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = azureBlobFile.ContainerName,
                    BlobName = azureBlobFile.KeyName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration), // Use TimeSpan here
                };

                // Set other permissions as needed (e.g., Read, Write, Delete)
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Get the SAS token
                var sasToken = blobClient.GenerateSasUri(sasBuilder);

                return sasToken.ToString();
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error getting signed URL for Azure Blob Storage: {ex.Message}");
            }
        }



    }
}
