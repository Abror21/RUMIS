using Izm.Rumis.Api.Models;
using Izm.Rumis.Application;
using Izm.Rumis.Application.Dto;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq.Expressions;

namespace Izm.Rumis.Api.Mappers
{
    public class Mapper
    {
        private static readonly Func<Classifier, ClassifierValue> classifierProjection =
            CompileProjection<Classifier, ClassifierValue>(x => new ClassifierValue
            {
                Id = x.Id,
                Value = x.Value,
                Code = x.Code,
                Payload = x.Payload,
                Type = x.Type
            });

        private static readonly Func<UserProfile, AuditUserModel> auditUserProjection =
           CompileProjection<UserProfile, AuditUserModel>(x => new AuditUserModel
           {
               //FirstName = x.FirstName,
               //FullName = x.FullName,
               //Id = x.Id,
               //LastName = x.LastName,
               //UserName = x.Name
           });

        /// <summary>
        /// Map classifier entity to view model. 
        /// Be aware, that the LINQ to SQL projection this mapping uses will still query all the classifier fields.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ClassifierValue MapClassifier(Classifier entity)
        {
            return entity == null ? null : classifierProjection.Invoke(entity);
        }

        /// <summary>
        /// Map audit user entity to view model. 
        /// Be aware, that the LINQ to SQL projection this mapping uses will still query all the user profile fields.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static AuditUserModel MapAuditUser(UserProfile entity)
        {
            return entity == null ? null : auditUserProjection.Invoke(entity);
        }

        public static FileDto MapFile(IFormFile model, FileDto dto)
        {
            if (model == null)
                return dto;

            dto.Content = Utility.StreamToArray(model.OpenReadStream());
            dto.ContentType = model.ContentType;
            dto.FileName = model.FileName;

            return dto;
        }

        public static DateOnly? MapDateOnly(DateTime? date)
        {
            if (!date.HasValue)
                return null;

            return DateOnly.FromDateTime(date.Value);
        }

        public static DateTime? MapDateOnly(DateOnly? date)
        {
            if (!date.HasValue)
                return null;

            return date.Value.ToDateTime(new TimeOnly());
        }

        public static Func<TEntity, TModel> CompileProjection<TEntity, TModel>(Expression<Func<TEntity, TModel>> projection)
        {
            return projection.Compile();
        }
    }
}
