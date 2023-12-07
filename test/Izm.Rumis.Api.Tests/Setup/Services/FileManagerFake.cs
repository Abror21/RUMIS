using Izm.Rumis.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class FileManagerFake : IFileManager
    {
        public Task<IActionResult> Download(Guid fileId)
        {
            var result = new FileContentResult(Encoding.UTF8.GetBytes("content"), "application/octet-stream")
            {
                FileDownloadName = "download.txt"
            } as IActionResult;

            return Task.FromResult(result);
        }
    }
}
