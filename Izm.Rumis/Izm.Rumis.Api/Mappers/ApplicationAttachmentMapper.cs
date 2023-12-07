using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public class ApplicationAttachmentMapper
    {
        public static ApplicationAttachmentCreateDto Map(ApplicationAttachmentCreateRequest model, ApplicationAttachmentCreateDto dto)
        {
            Map((ApplicationAttachmentEditRequest)model, dto);

            dto.ApplicationId = model.ApplicationId;

            dto.File = model.File == null ? null : Mapper.MapFile(model.File, new FileDto());
            dto.File.SourceType = FileSourceType.S3;

            return dto;
        }

        public static ApplicationAttachmentUpdateDto Map(ApplicationAttachmentUpdateRequest model, ApplicationAttachmentUpdateDto dto)
        {
            Map((ApplicationAttachmentEditRequest)model, dto);

            dto.File = Mapper.MapFile(model.File, new FileDto());
            dto.File.SourceType = FileSourceType.S3;

            return dto;
        }

        private static ApplicationAttachmentEditDto Map(ApplicationAttachmentEditRequest model, ApplicationAttachmentEditDto dto)
        {
            dto.AttachmentNumber = model.AttachmentNumber;
            dto.AttachmentDate = DateOnly.FromDateTime(model.AttachmentDate);

            return dto;
        }

        public static Expression<Func<ApplicationAttachment, ApplicationAttachmentResponse>> ProjectListItem()
        {
            return t => new ApplicationAttachmentResponse
            {
                Id = t.Id,
                AttachmentNumber = t.AttachmentNumber,
                AttachmentDate = t.AttachmentDate.ToDateTime(new TimeOnly()),
                File = new ApplicationAttachmentResponse.FileData
                {
                    Id = t.FileId,
                    FileName = t.File != null ? t.File.Name : null,
                }
            };
        }
    }
}
