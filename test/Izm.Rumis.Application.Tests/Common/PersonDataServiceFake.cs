using Izm.Rumis.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Tests.Common
{
    public class PersonDataServiceFake : IPersonDataService
    {
        public GetBirthDataParameters GetBirthDataCalledWith { get; set; } = null;
        public IEnumerable<string> ParentOrGuardianPersonalIdentifiers { get; set; } = new List<string>();

        public Task<DateTime> GetBirthDateAsync(string submitterPersonalIdentifier, string targetPrivatePersonalIdentifier, string type, CancellationToken cancellation = default)
        {
            GetBirthDataCalledWith = new GetBirthDataParameters(submitterPersonalIdentifier, targetPrivatePersonalIdentifier, type);

            return Task.FromResult(DateTime.Now);
        }

        public Task<IEnumerable<string>> GetStudentsByParentOrGuardianAsync(string privatePersonalIdentifier, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ParentOrGuardianPersonalIdentifiers);
        }

        public Task<bool> CheckEducationalInstitutionAsEmployee(string privatePersonalIdentifier, string educationalInstitutionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CheckEducationalInstitutionAsStudent(string privatePersonalIdentifier, string educationalInstitutionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public record GetBirthDataParameters(string SubmitterPersonalIdentifier, string TargetPrivatePersonalIdentifier, string Type);
    }
}
