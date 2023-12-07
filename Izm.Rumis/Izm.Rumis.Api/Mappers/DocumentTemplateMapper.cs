using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public static class DocumentTemplateMapper
    {
        public static Expression<Func<DocumentTemplate, DocumentTemplateIntermediateModel>> IntermediateProject()
        {
            return t => new DocumentTemplateIntermediateModel
            {
                Code = t.Code,
                Id = t.Id,
                Title = t.Title,
                ValidFrom = Mapper.MapDateOnly(t.ValidFrom),
                ValidTo = Mapper.MapDateOnly(t.ValidTo),
                Hyperlink = t.Hyperlink,
                ResourceType = t.ResourceType,
                FileId = t.FileId,
                FileName = t.FileName,
                SupervisorId = t.SupervisorId,
                PermissionType = t.PermissionType
            };
        }

        public static Expression<Func<DocumentTemplateIntermediateModel, DocumentTemplateModel>> Project()
        {
            return t => new DocumentTemplateModel
            {
                Id = t.Id,
                Title = t.Title,
                ValidFrom = t.ValidFrom,
                ValidTo = t.ValidTo,
                Hyperlink = t.Hyperlink,
                ResourceType = t.ResourceType == null ? null : new DocumentTemplateModel.ClassifierData
                {
                    Id = t.ResourceType.Id,
                    Code = t.ResourceType.Code,
                    Value = t.ResourceType.Value
                },
                File = new DocumentTemplateModel.FileData
                {
                    Id = t.FileId,
                    FileName = t.FileName
                },
                SupervisorId = t.SupervisorId,
                PermissionType = t.PermissionType
            };
        }

        public static readonly Func<DocumentTemplateIntermediateModel, DocumentTemplateModel> ProjectCompiled =
         ((Project())).Compile();

    }
}
