using FileService.AWS;
using FileService.Test.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileService.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileService _fileService;

        public HomeController(ILogger<HomeController> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;

            var fileModel = new FileModel
            {
                BucketName = "Your_Bucket_Name",
                KeyName = "FileName",
                FilePath = "path to file.txt"
            };

            // Call the FileService method to Get  the  content of file
            _fileService.GetFileAsStringAsync(fileModel).GetAwaiter().GetResult();

            // Call the FileService method to upload the file
            _fileService.UploadFileAsync(fileModel).GetAwaiter().GetResult();

            // Call the FileService method to delete the file
            _fileService.DeleteFileAsync(fileModel).GetAwaiter().GetResult();

            // Call the FileService method  with expiration time to Get  the  url of file
            TimeSpan expiration = TimeSpan.FromHours(1);
            _fileService.GetSignedUrlAsync(fileModel,expiration).GetAwaiter().GetResult();

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}