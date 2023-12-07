using Izm.Rumis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Application.Common
{
    public interface IAppDbContext
    {
        Guid UnitOfWorkId { get; }
        DbSet<Classifier> Classifiers { get; }
        DbSet<DocumentTemplate> DocumentTemplates { get; }
        DbSet<File> Files { get; }
        DbSet<Parameter> Parameters { get; }
        DbSet<TextTemplate> TextTemplates { get; }
        DbSet<User> Users { get; }
        DbSet<UserProfile> UserProfiles { get; }
        DbSet<PersonDataReport> PersonDataReports { get; }
        DbSet<Person> Persons { get; }
        DbSet<PersonTechnical> PersonTechnicals { get; }
        DbSet<PersonContact> PersonContacts { get; }
        DbSet<EducationalInstitution> EducationalInstitutions { get; }
        DbSet<Supervisor> Supervisors { get; }
        DbSet<Role> Roles { get; }
        DbSet<RolePermission> RolePermissions { get; }
        DbSet<Domain.Entities.Application> Applications { get; }
        DbSet<ApplicationAttachment> ApplicationAttachments { get; set; }
        DbSet<Resource> Resources { get; }
        DbSet<ResourceParameter> ResourceParameters { get; }
        DbSet<ApplicationResourceAttachment> ApplicationResourceAttachments { get; }
        DbSet<ApplicationResource> ApplicationResources { get; }
        DbSet<ApplicationSocialStatus> ApplicationSocialStatuses { get; }
        DbSet<ApplicationResourceContactPerson> ApplicationResourceContactPersons { get; }
        DbSet<EducationalInstitutionContactPerson> EducationalInstitutionContactPersons { get; }
        DbSet<ContactPersonResourceSubType> ContactPersonResourceSubTypes { get; }
        DbSet<GdprAudit> GdprAudits { get; }
        DbSet<GdprAuditData> GdprAuditData { get; }
        DbSet<EducationalInstitutionResourceSubType> EducationalInstitutionResourceSubTypes { get; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task InTransactionAsync(Func<Task> operations);
        DbSet<T> Set<T>() where T : class;
        EntityEntry<T> Entry<T>(T entity) where T : class;
        Task InsertManyAsync<T>(IList<T> entities) where T : class;
        Task DeleteManyAsync<T>(IList<T> entities) where T : class;
    }
}
