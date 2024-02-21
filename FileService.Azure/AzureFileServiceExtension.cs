using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Azure
{
    public static class AzureFileServiceExtension
    {
        public static IServiceCollection AddAzureFileService(this IServiceCollection serviceCollection, Action<AzureOptions> options)
        {
            serviceCollection.AddSingleton(provider =>
            {
                var azureOptions = new AzureOptions();
                options?.Invoke(azureOptions);

                var connectionString = azureOptions.ConnectionString;
                var blobServiceClient = new BlobServiceClient(connectionString);

                return blobServiceClient;
            });

            serviceCollection.AddScoped<IFileService, AzureFileService>();

            return serviceCollection;
        }
    }

}
