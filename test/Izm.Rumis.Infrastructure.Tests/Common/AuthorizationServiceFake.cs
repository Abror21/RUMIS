using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure.Tests.Common
{
    internal sealed class AuthorizationServiceFake : IAuthorizationService
    {
        public IAuthorizedResource AuthorizeResourceCalledWith { get; set; } = null;
        public IAuthorizedResourceCreateDto AuthorizeResourceCreateDtoCalledWith { get; set; } = null;
        public IAuthorizedResourceEditDto AuthorizeResourceEditDtoCalledWith { get; set; } = null;
        public IAuthorizedDocumentTemplateEditDto AuthorizedDocumentTemplateEditDtoCalledWith { get; set; } = null;
        public int? AuthorizeEducationalInstitutionDtoCalledWith { get; set; } = null;
        public string AuthorizeAsyncCalledWith { get; set; } = null;

        public void Authorize(IAuthorizedResource item)
        {
            AuthorizeResourceCalledWith = item;

            return;
        }

        public void Authorize(IAuthorizedResourceCreateDto item)
        {
            AuthorizeResourceCreateDtoCalledWith = item;

            return;
        }

        public void Authorize(IAuthorizedResourceEditDto item)
        {
            AuthorizeResourceEditDtoCalledWith = item;

            return;
        }

        public void Authorize(IAuthorizedDocumentTemplateEditDto item)
        {
            AuthorizedDocumentTemplateEditDtoCalledWith = item;

            return;
        }

        public void Authorize(int educationalInstitutionId)
        {
            AuthorizeEducationalInstitutionDtoCalledWith = educationalInstitutionId;

            return;
        }

        public Task AuthorizeAsync(string parentOrGuardianPersonalIdentifier, string studentPersonalIdentifier, CancellationToken cancellationToken = default)
        {
            AuthorizeAsyncCalledWith = parentOrGuardianPersonalIdentifier + studentPersonalIdentifier;

            return Task.CompletedTask;
        }

        public Task AuthorizeAsync(string type, string privatePersonalIdentifier, string educationalIsntitutionCode, CancellationToken cancellationToken = default)
        {
            AuthorizeAsyncCalledWith = privatePersonalIdentifier + educationalIsntitutionCode;

            return Task.CompletedTask;
        }
    }
}
