using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Validators
{
    public interface IClassifierValidator
    {
        /// <summary>
        /// Validate <see cref="Classifier"/> item.
        /// </summary>
        /// <param name="item">Item to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        Task ValidateAsync(Classifier item, CancellationToken cancellationToken = default);
    }

    public sealed class ClassifierValidator : IClassifierValidator
    {
        private readonly IAppDbContext db;

        public ClassifierValidator(IAppDbContext db)
        {
            this.db = db;
        }

        // <inheritdoc/>
        public async Task ValidateAsync(Classifier item, CancellationToken cancellationToken = default)
        {
            if (ClassifierTypes.RequiredStatuses.Contains(item.Type))
                throw new ValidationException(Error.TypeForbidden);

            var classifierType = await db.Classifiers
                .Where(t => t.Type == ClassifierTypes.ClassifierType && t.Code == item.Type)
                .FirstOrDefaultAsync(cancellationToken);

            if (classifierType == null)
                throw new ValidationException(Error.UnknownType);

            if (classifierType.PermissionType != item.PermissionType)
                throw new ValidationException(Error.IncorrectPermissionType);

            if (string.IsNullOrEmpty(item.Value))
                throw new ValidationException(Error.ValueRequired);

            var checkCode = !string.IsNullOrEmpty(item.Code);

            if (await db.Classifiers.AnyAsync(t => t.Id != item.Id && t.Type == item.Type
                && ((checkCode && t.Code == item.Code) || t.Value == item.Value), cancellationToken))
            {
                throw new ValidationException(Error.AlreadyExists);
            }

            switch (item.Type)
            {
                case ClassifierTypes.ResourceSubType:
                    var payload = DeserializePayload<ResourceSubTypePayload>(item.Payload);

                    if (payload.ResourceType == null)
                        throw new ValidationException(Error.PayloadIncomplete);

                    break;

                case ClassifierTypes.Placeholder:
                    if (DeserializePayload<PlaceholderPayload>(item.Payload).Value == null)
                        throw new ValidationException(Error.PayloadIncomplete);

                    break;

                default:
                    break;
            }
        }

        private T DeserializePayload<T>(string payload)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(payload);
            }
            catch
            {
                throw new ValidationException(Error.CannotDeserializePayload);
            }
        }

        public static class Error
        {
            public const string AlreadyExists = "classifier.alreadyExists";
            public const string UnknownType = "classifier.unknownType";
            public const string ValueRequired = "classifier.valueRequired";
            public const string PayloadIncomplete = "classifier.payloadIncomplete";
            public const string CannotDeserializePayload = "classifier.cannotDeserializePayload";
            public const string TypeForbidden = "classifier.typeForbidden";
            public const string IncorrectPermissionType = "classifier.incorrectPermissionType";
        }
    }
}
