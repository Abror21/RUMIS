using Izm.Rumis.Infrastructure.Enums;
using Izm.Rumis.Infrastructure.Viis;
using Izm.Rumis.Infrastructure.Viis.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Tests.Setup.Services
{
    internal class ViisServiceFake : IViisService
    {
        public List<RelatedPersonData.Student> Students { get; set; } = new List<RelatedPersonData.Student>();
        public List<SocialStatusData.Student> StudentSocialStatus { get; set; } = new List<SocialStatusData.Student>();
        public List<EmployeeData.Employee> Employee { get; set; } = new List<EmployeeData.Employee>();

        public Task CheckPersonApplicationsAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<RelatedPersonData.Student>> GetStudentsAsync(RequestParamType type, string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Students);
        }

        public Task<List<EmployeeData.Employee>> GetEmployeesAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Employee);
        }

        public Task SyncEducationalInstitutionsAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public Task<List<SocialStatusData.Student>> CheckSocialStatusAsync(string privatePersonalIdentifier, string type, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StudentSocialStatus);
        }
    }
}
