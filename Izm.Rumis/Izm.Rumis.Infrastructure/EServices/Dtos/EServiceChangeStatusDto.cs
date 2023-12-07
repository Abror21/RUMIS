using Izm.Rumis.Application.Dto;
using System;
using System.Collections.Generic;

namespace Izm.Rumis.Infrastructure.EServices.Dtos
{
    public class EServiceChangeStatusDto
    {
        public IEnumerable<Guid> FileToDeleteIds { get; set; } = Array.Empty<Guid>();
        public IEnumerable<FileDto> Files { get; set; } = Array.Empty<FileDto>();
        public string Notes { get; set; }
    }
}
