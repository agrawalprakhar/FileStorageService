using FileService.AWS;
using FileService.Azure;
using Microsoft.Extensions.Configuration;

namespace FileService.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var Configuration = builder.Configuration;

            // Retrieve the value of "File Service Type" from configuration
            string emailServiceType = Configuration["FileServiceType"];
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Use the value of emailServiceType to configure services accordingly
            if (emailServiceType == "Azure")
            {
                // Register BlobServiceClient
                builder.Services.AddAzureFileService(options =>
                {
                    options.ConnectionString = Configuration.GetSection("Azure:ConnectionString").Value;
                });
            }
            else if (emailServiceType == "AWS")
            {
                builder.Services.AddAWSFileService(options =>
                {
                    options.AccessKey = Configuration.GetSection("AWS:AccessKeyId").Value;
                    options.SecretKey = Configuration.GetSection("AWS:SecretAccessKey").Value;
                    options.Region = Configuration.GetSection("AWS:Region").Value;
                });
            }
            else
            {
                throw new InvalidOperationException("Invalid file service type specified in configuration.");
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}