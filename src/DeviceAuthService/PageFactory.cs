using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Azure.WebJobs;


namespace Ltwlf.Azure.B2C
{

    public class PageFactory
    {
        public enum PageType
        {
            UserCode,
            Success,
            Error
        }
        
        private readonly string _userCodePagePath;
        private readonly string _successPagePath;
        private readonly string _errorPagePath;
        

        public PageFactory(IOptions<ConfigOptions> options, IHostingEnvironment  host)
        {
            //For Visual Studio
            //_userCodePagePath = options.Value.UserCodePage ?? Path.Combine(host.ContentRootPath, "./www/userCode.html");
            //_successPagePath = options.Value.SuccessPage ?? Path.Combine(host.ContentRootPath, "./www/success.html");
            //_errorPagePath = options.Value.ErrorPage ?? Path.Combine(host.ContentRootPath, "./www/error.html");

            //For Azure Function
            //_userCodePagePath = options.Value.UserCodePage ?? "C:/home/site/wwwroot/www/userCode.html";
            //_successPagePath = options.Value.SuccessPage ?? "C:/home/site/wwwroot/www/success.html";
            //_errorPagePath = options.Value.ErrorPage ?? "C:/home/site/wwwroot/www/error.html";

            //Test
            var local_root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azure_root = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

            var actual_root = local_root ?? azure_root;

            _userCodePagePath = options.Value.UserCodePage ?? Path.Combine(actual_root, "./www/userCode.html");
            _successPagePath = options.Value.SuccessPage ?? Path.Combine(actual_root, "./www/success.html");
            _errorPagePath = options.Value.ErrorPage ?? Path.Combine(actual_root, "./www/error.html");

        }

        public IActionResult GetPageResult(PageType pageType)
        {
            var path = pageType switch
            
            {
                PageType.UserCode => _userCodePagePath,
                PageType.Success => _successPagePath,
                PageType.Error => _errorPagePath,
                _ => throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null)
            };
            
            if (path.StartsWith("http"))
            {
                var client = new WebClient();
                {
                    return new  FileContentResult(client.DownloadData(new Uri(path)),
                        "text/html; charset=UTF-8");
                }
            }

            return new FileStreamResult(File.OpenRead(path), "text/html; charset=UTF-8");
        }
    }
}