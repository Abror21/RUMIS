using Izm.Rumis.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Services
{
    public interface IFileManager
    {
        Task<IActionResult> Download(Guid fileId);
    }

    public class FileManager : IFileManager
    {
        private readonly IFileService fileService;

        public FileManager(IFileService fileService)
        {
            this.fileService = fileService;
        }

        public async Task<IActionResult> Download(Guid fileId)
        {
            var file = await fileService.GetAsync(fileId);

            return file == null
                ? new NotFoundResult()
                : new FileContentResult(file.Content, "application/octet-stream")
                {
                    FileDownloadName = file.Name
                };
        }
    }
}
