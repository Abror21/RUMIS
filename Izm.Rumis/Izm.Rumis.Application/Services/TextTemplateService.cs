using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Services
{
    public class TextTemplateService : ITextTemplateService
    {
        private readonly IAppDbContext db;

        public TextTemplateService(IAppDbContext db)
        {
            this.db = db;
        }

        /// <inheritdoc/>
        public SetQuery<TextTemplate> Get()
        {
            return new SetQuery<TextTemplate>(db.TextTemplates.AsNoTracking());
        }

        /// <inheritdoc/>
        /// <exception cref="ValidationException"></exception>
        public async Task<int> CreateAsync(TextTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            var entity = new TextTemplate
            {
                Code = Utility.SanitizeCode(item.Code),
                Content = item.Content,
                Title = item.Title
            };

            await Validate(entity);

            await db.TextTemplates.AddAsync(entity, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task UpdateAsync(int id, TextTemplateEditDto item, CancellationToken cancellationToken = default)
        {
            var entity = await db.TextTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            entity.Code = Utility.SanitizeCode(item.Code);
            entity.Title = item.Title;
            entity.Content = item.Content;

            await Validate(entity);

            await db.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        /// <exception cref="EntityNotFoundException"></exception>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await db.TextTemplates.FindAsync(new object[] { id }, cancellationToken);

            if (entity == null)
                throw new EntityNotFoundException();

            db.TextTemplates.Remove(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task Validate(TextTemplate item, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(item.Title))
                throw new ValidationException("textTemplate.titleRequired");

            if (string.IsNullOrEmpty(item.Code))
                throw new ValidationException("textTemplate.codeRequired");

            if (await db.TextTemplates.AnyAsync(t => t.Id != item.Id && t.Code == item.Code, cancellationToken))
                throw new ValidationException("textTemplate.alreadyExists");
        }
    }
}
