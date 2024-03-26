using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FileService.AWS
{
    public static class AWSFileServiceExtension
    {
        public static IServiceCollection AddAWSFileService(this IServiceCollection serviceCollection, Action<AWSOptions> options)
        {
            serviceCollection.AddScoped<IFileService, AWSFileService>();

            serviceCollection.Configure(options);

            serviceCollection.AddSingleton<IAmazonS3>(sp =>
            {
                var awsOptions = sp.GetRequiredService<IOptions<AWSOptions>>().Value;

                var s3Client = new AmazonS3Client(awsOptions.AccessKey, awsOptions.SecretKey);

                return s3Client;
            });

            return serviceCollection;
        }
    }
}
