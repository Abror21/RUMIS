using EFCore.BulkExtensions;
using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Models;
using Izm.Rumis.Domain.Attributes;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure.Common;
using Izm.Rumis.Infrastructure.Exceptions;
using Izm.Rumis.Infrastructure.Identity;
using Izm.Rumis.Infrastructure.Logging;
using Izm.Rumis.Infrastructure.Modif;
using Izm.Rumis.Infrastructure.Sessions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Infrastructure
{
    public class AppDbContext : DbContext, IAppDbContext, IIdentityDbContext, ILogDbContext, IModifDbContext, ISessionDbContext
    {
        private readonly ICurrentUserService currentUser;
        private readonly SoftDeleteHelper softDeleteHelper = new SoftDeleteHelper();
        private readonly MethodInfo softDeleteAddFilterMethod;
        private readonly MethodInfo softDeleteCheckDeletedMethod;
        private readonly IMediator _mediator;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ICurrentUserService currentUser,
            IMediator mediator) : base(options)
        {
            this.currentUser = currentUser;

            softDeleteAddFilterMethod = softDeleteHelper.GetType().GetMethod(nameof(SoftDeleteHelper.AddEntityFilter));
            softDeleteCheckDeletedMethod = softDeleteHelper.GetType().GetMethod(nameof(SoftDeleteHelper.CheckDeleted));
            _mediator = mediator;
        }

        public Guid UnitOfWorkId { get; } = Guid.NewGuid();

        // administration
        public DbSet<Classifier> Classifiers { get; set; }
        public DbSet<DocumentTemplate> DocumentTemplates { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<GdprAudit> GdprAudits { get; set; }
        public DbSet<GdprAuditData> GdprAuditData { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<PersonDataReport> PersonDataReports { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<PersonTechnical> PersonTechnicals { get; set; }
        public DbSet<PersonContact> PersonContacts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<TextTemplate> TextTemplates { get; set; }

        // identity
        public DbSet<IdentityUser> IdentityUsers { get; set; }
        public DbSet<IdentityUserLogin> IdentityUserLogins { get; set; }

        // log
        public DbSet<Log> Log { get; set; }

        // modif
        //public DbSet<SampleEntityModif> SampleEntityModifs { get; set; }

        // business
        public DbSet<Domain.Entities.Application> Applications { get; set; }
        public DbSet<ApplicationAttachment> ApplicationAttachments { get; set; }
        public DbSet<ApplicationResource> ApplicationResources { get; set; }
        public DbSet<ApplicationResourceContactPerson> ApplicationResourceContactPersons { get; set; }
        public DbSet<ApplicationResourceAttachment> ApplicationResourceAttachments { get; set; }
        public DbSet<ApplicationSocialStatus> ApplicationSocialStatuses { get; set; }
        public DbSet<EducationalInstitution> EducationalInstitutions { get; set; }
        public DbSet<ContactPersonResourceSubType> ContactPersonResourceSubTypes { get; set; }
        public DbSet<EducationalInstitutionContactPerson> EducationalInstitutionContactPersons { get; set; }
        public DbSet<EducationalInstitutionResourceSubType> EducationalInstitutionResourceSubTypes { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceParameter> ResourceParameters { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Session> Sessions { get; set; }

        private async Task _dispatchDomainEvents()
        {
            var domainEventEntities = ChangeTracker.Entries<Entity<Guid>>()
                .Select(po => po.Entity)
                .Where(po => po.Events.Any())
                .ToArray();

            foreach (var entity in domainEventEntities)
            {
                var events = entity.Events.ToArray();
                entity.Events.Clear();
                foreach (var entityDomainEvent in events)
                    await _mediator.Publish(entityDomainEvent);
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ownedEntityType = typeof(IOwnedEntity);

            foreach (var type in modelBuilder.Model.GetEntityTypes())
            {
                if (type.ClrType.BaseType == typeof(ModifEntity))
                    modelBuilder.Entity(type.ClrType).Metadata.SetIsTableExcludedFromMigrations(true);

                foreach (var rel in type.GetForeignKeys())
                {
                    //if (!(rel.PrincipalEntityType.ClrType.GetInterfaces().Contains(entityStructureType)))
                    rel.DeleteBehavior = DeleteBehavior.NoAction;
                }
            }

            // temporarily exclude entities from the database (for development)
            //modelBuilder.Entity<EntityClassName>().Metadata.SetIsTableExcludedFromMigrations(true);

            AddSoftDeleteFilters(modelBuilder);
            ConvertEnumToString(modelBuilder);

            modelBuilder.Entity<GdprAudit>(entity =>
            {
                entity.HasIndex(c => c.Created);
                entity.HasIndex(c => c.DataHandlerPrivatePersonalIdentifier);
                entity.HasIndex(c => c.DataOwnerPrivatePersonalIdentifier);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasOne(c => c.CreatedBy)
                    .WithMany();

                entity.HasOne(c => c.ModifiedBy)
                    .WithMany();

                entity.HasOne(c => c.PersonTechnical)
                    .WithMany(c => c.Persons)
                    .HasForeignKey(c => c.PersonTechnicalId);
            });

            modelBuilder.Entity<PersonTechnical>(entity =>
            {
                entity.HasOne(c => c.CreatedBy)
                    .WithMany();

                entity.HasOne(c => c.ModifiedBy)
                    .WithMany();

                entity.HasOne(c => c.User)
                    .WithOne(c => c.PersonTechnical);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(c => c.Id).ValueGeneratedNever();

                entity.HasMany(c => c.Profiles)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(c => c.Profiles)
                    .HasForeignKey(c => c.UserId);

                entity.HasMany(c => c.Roles)
                    .WithMany(c => c.UserProfiles);
            });

            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.Property(c => c.Id).ValueGeneratedNever();

                entity.HasOne(c => c.User)
                    .WithOne()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<IdentityUserLogin>(entity =>
            {
                entity.Property(c => c.Id).ValueGeneratedNever();

                entity.HasOne(c => c.User)
                    .WithMany(c => c.Logins)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Classifier>(entity =>
            {
                entity.HasOne(c => c.EducationalInstitution)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<EducationalInstitution>(entity =>
            {
                entity.HasOne(c => c.Status)
                    .WithMany()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasIndex(c => c.Date);

                entity.HasIndex(c => c.Level);

                entity.HasIndex(c => c.RequestMethod);
            });
            // address structure binding example
            //modelBuilder.Entity<EntityAddress>(entity =>
            //{
            //    entity.OwnsOne(t => t.Address);
            //});

            base.OnModelCreating(modelBuilder);
        }

        public int SaveChanges()
        {
            PrepareEntries();

            int result;

            try
            {
                result = base.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("error.dbUpdate", ex);
            }

            return result;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dispatchDomainEvents();

            PrepareEntries();

            int result;

            try
            {
                result = await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("error.dbUpdate", ex);
            }

            return result;
        }

        public virtual async Task InTransactionAsync(Func<Task> operations)
        {
            using (var trn = await Database.BeginTransactionAsync())
            {
                await operations();
                await trn.CommitAsync();
            }
        }

        public virtual async Task InsertManyAsync<T>(IList<T> entities) where T : class
        {
            await DbContextBulkExtensions.BulkInsertAsync(this, entities);
        }

        public virtual async Task DeleteManyAsync<T>(IList<T> entities) where T : class
        {
            await DbContextBulkExtensions.BulkDeleteAsync(this, entities);
        }

        //public IEnumerable<TModel> GetAuditRecords<TEntity, TModel>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TModel>> selector)
        //    where TEntity : class
        //{
        //    var entityType = Model.FindEntityType(typeof(TEntity));
        //    var schema = entityType.GetSchema();
        //    var table = entityType.GetTableName();

        //    FormattableString query;

        //    query = $@"select * from {schema}.{table}_Modif";

        //    Database.ExecuteSqlRaw()

        //    return Set<TEntity>().FromSqlInterpolated(query).AsNoTracking().Where(where).Select(selector).ToList();
        //}

        //// This is an example of how to query domain entities with auditable user data in case when user profiles are not used.
        //public IQueryable<AuditableEntity<T>> QueryAuditable<T>() where T : class, IAuditable
        //{
        //    var query = Set<T>()
        //        .Join(IdentityUsers, t => t.CreatedById, u => u.Id, (t, u) => new
        //        {
        //            Entity = t,
        //            CreatedBy = new AuditableEntityUser
        //            {
        //                Id = u.Id,
        //                Name = u.UserName
        //            }
        //        })
        //        .Join(IdentityUsers, t => t.Entity.ModifiedById, u => u.Id, (t, u) => new AuditableEntity<T>
        //        {
        //            Entity = t.Entity,
        //            CreatedBy = t.CreatedBy,
        //            ModifiedBy = new AuditableEntityUser
        //            {
        //                Id = u.Id,
        //                Name = u.UserName
        //            }
        //        });

        //    return query;
        //}

        private void AddSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            var thisType = GetType();

            foreach (var prop in thisType.GetProperties())
            {
                var setType = prop.PropertyType;

                if (setType.IsGenericType && setType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var args = setType.GetGenericArguments();

                    foreach (var arg in args)
                    {
                        if (arg.GetInterfaces().Contains(typeof(ISoftDeletable)))
                        {
                            var genericMethod = softDeleteAddFilterMethod.MakeGenericMethod(arg);
                            genericMethod.Invoke(softDeleteHelper, new object[] { modelBuilder });
                        }
                    }
                }
            }
        }

        private void ConvertEnumToString(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var nullableType = Nullable.GetUnderlyingType(property.ClrType);

                    if (property.ClrType.IsEnum || nullableType?.IsEnum == true)
                    {
                        var type = typeof(EnumToStringConverter<>).MakeGenericType(nullableType ?? property.ClrType);
                        var converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

                        property.SetValueConverter(converter);
                    }
                }
            }
        }

        private void PrepareEntries()
        {
            var now = DateTime.Now;
            var userId = currentUser.Id;

            if (userId == UserIds.Anonymous)
                userId = UserIds.Application;

            var classifierValidation = new List<(Guid?, ClassifierTypeAttribute)>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Entity<Guid>)
                {
                    var guidEntry = (entry.Entity as Entity<Guid>);

                    if (guidEntry.Id == Guid.Empty)
                        throw new Exception("Empty GUIDs are not allowed.");
                }

                if (entry.Entity is IAuditable)
                {
                    var auditableEntry = entry.Entity as IAuditable;

                    if (entry.State == EntityState.Added)
                    {
                        auditableEntry.Created = now;
                        auditableEntry.CreatedById = userId;
                        auditableEntry.Modified = now;
                        auditableEntry.ModifiedById = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditableEntry.Modified = now;
                        auditableEntry.ModifiedById = userId;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        auditableEntry.Modified = now;
                        auditableEntry.ModifiedById = userId;
                    }
                }

                if (entry.Entity is ISoftDeletable)
                {
                    var softDeleteEntry = entry.Entity as ISoftDeletable;

                    if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        softDeleteEntry.IsDeleted = true;
                    }
                }

                // validate changed foreign key properties
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    foreach (var prop in entry.OriginalValues.Properties)
                    {
                        var originalValue = entry.OriginalValues[prop];
                        var currentValue = entry.CurrentValues[prop];

                        if (prop.PropertyInfo != null && (entry.State == EntityState.Added || originalValue?.ToString() != currentValue?.ToString()))
                        {
                            var classifierTypeAttr = prop.PropertyInfo.GetCustomAttributes(typeof(ClassifierTypeAttribute), false);

                            if (classifierTypeAttr.Any())
                            {
                                var classifierType = classifierTypeAttr.First() as ClassifierTypeAttribute;
                                classifierValidation.Add((currentValue as Guid?, classifierType));
                            }
                        }
                    }
                }
            }

            if (classifierValidation.Any())
                ValidateClassifiersAsync(classifierValidation.ToArray()).Wait();
        }

        private async Task ValidateClassifiersAsync(params (Guid?, ClassifierTypeAttribute)[] classifiers)
        {
            var nonEmpty = classifiers.Where(t => t.Item1 != null).ToList();
            var ids = nonEmpty.Select(t => t.Item1).ToArray();

            var data = await Classifiers
                .Where(t => t.Type == ClassifierTypes.ClassifierType || (ids.Contains(t.Id) && !t.IsDisabled))
                .Select(t => new
                {
                    t.Id,
                    t.Type,
                    t.Code,
                    t.Payload
                }).ToListAsync();

            var types = data.Where(t => t.Type == ClassifierTypes.ClassifierType).Select(t => new
            {
                t.Id,
                t.Type,
                t.Code,
                Props = ClassifierPayload.Parse<ClassifierTypePayload>(t.Payload)
            }).ToArray();

            foreach (var c in nonEmpty)
            {
                var ex = false;
                var id = c.Item1;
                var typeAttr = c.Item2;
                var type = c.Item2.Value;

                ex = typeAttr.IsGroup
                    ? data.Any(t => t.Id == id && t.Type == ClassifierTypes.ClassifierType && types.Any(n => n.Props.Group == type))
                    : data.Any(t => t.Id == id && t.Type == type);

                if (!ex)
                    throw new ValidationException("error.classifierValueNotFound",
                        new Exception($"Classifier {type}{(typeAttr.IsGroup ? ".*" : "")}/{id} not found or disabled."));
            }
        }

        private void ValidateSoftDeletable(Type type, Type keyType, object id)
        {
            var genericMethod = softDeleteCheckDeletedMethod.MakeGenericMethod(type, keyType);
            genericMethod.Invoke(softDeleteHelper, new object[] { this, id });
        }

        private class SoftDeleteHelper
        {
            public void AddEntityFilter<TEntity>(ModelBuilder modelBuilder)
                where TEntity : class, ISoftDeletable
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(t => !t.IsDeleted);
            }

            public void CheckDeleted<TEntity, TKey>(DbContext context, TKey id)
                where TEntity : class, IEntity<TKey>, ISoftDeletable
                where TKey : IComparable
            {
                var exists = context.Set<TEntity>().Any(t => (object)t.Id == (object)id);

                if (!exists)
                    throw new EntityNotFoundException();
            }
        }
    }
}
