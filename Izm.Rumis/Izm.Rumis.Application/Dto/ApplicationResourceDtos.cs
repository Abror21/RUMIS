using System;
using System.Collections.Generic;

namespace Izm.Rumis.Application.Dto
{
    public class ApplicationResourceEditDto
    {
        public Guid? AssignedResourceId { get; set; }
        public DateTime? AssignedResourceReturnDate { get; set; }
        public IEnumerable<Guid> EducationalInstitutionContactPersonIds { get; set; } = Array.Empty<Guid>();
        public string Notes { get; set; }
    }

    public class ApplicationResourceCreateDto : ApplicationResourceEditDto
    {
        public Guid ApplicationId { get; set; }
    }

    public class ApplicationResourceUpdateDto : ApplicationResourceEditDto {}

    public class ApplicationResourceChangeStatusDto
    {
        public IEnumerable<Guid> FileToDeleteIds { get; set; } = Array.Empty<Guid>();
        public IEnumerable<FileDto> Files { get; set; } = Array.Empty<FileDto>();
        public string Notes { get; set; }
    }

    public class ApplicationResourceReturnEditDto
    {
        public Guid ResourceStatusId { get; set; }
        public Guid ReturnResourceStateId { get; set; }
        public DateTime ReturnResourceDate { get; set; }
        public string Notes { get; set; }
    }

    public class ApplicationResourceCancelDto
    {
        public Guid ReasonId { get; set; }
        public string Description { get; set; }
        public bool ChangeApplicationStatusToWithdrawn { get; set; }
    }

    public class ApplicationResourceReturnDeadlineDto
    {
        public IEnumerable<Guid> ApplicationResourceIds { get; set; }
        public DateTime AssignedResourceReturnDate { get; set; }
    }
}
