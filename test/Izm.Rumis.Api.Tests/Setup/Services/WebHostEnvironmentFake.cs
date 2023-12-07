using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal sealed class WebHostEnvironmentFake : IWebHostEnvironment
    {
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
    }
}
