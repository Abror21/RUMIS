using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Dto;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IPersonValidator
    {
        /// <summary>
        /// Validate <see cref="PersonEditDto"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ValidateAsync(PersonCreateDto item, CancellationToken cancellationToken = default);
    }

    public sealed class PersonValidator : IPersonValidator
    {
        private readonly IAppDbContext db;

        public PersonValidator(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public async Task ValidateAsync(PersonCreateDto item, CancellationToken cancellationToken = default)
        {
            if (!Utility.IsPrivatePersonalIdentifierChecksumValid(item.PrivatePersonalIdentifier))
                throw new ValidationException(Error.InvalidPrivatePersonalIdentifier);

            if (await db.Persons.AnyAsync(t => t.PrivatePersonalIdentifier == item.PrivatePersonalIdentifier, cancellationToken))
                throw new ValidationException(Error.AlreadyExists);
        }

        public static class Error
        {
            public const string AlreadyExists = "person.alreadyExists";
            public const string InvalidPrivatePersonalIdentifier = "person.invlalidPrivatePersonalIdentifier";
        }
    }
}
