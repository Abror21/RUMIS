using Izm.Rumis.Application.Dto;
using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Application.Validators;
using Izm.Rumis.Domain.Enums;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Constants.Classifiers;
using Izm.Rumis.Domain.Models.ClassifierPayloads;
using System.Text.Json;

namespace Izm.Rumis.Application.Tests
{
    public class ClassifierServiceTests
    {
        [Fact]
        public void Get_ReturnsData()
        {
            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.AddRange(
                new Classifier { Type = "x", Code = "", Value = "" },
                new Classifier { Type = "x", Code = "", Value = "" });

            db.SaveChanges();

            var data = CreateService(db).Get().List();

            Assert.Equal(2, data.Count());
        }

        [Theory]
        [InlineData(UserProfileType.Country, 6)]
        [InlineData(UserProfileType.Supervisor, 5)]
        [InlineData(UserProfileType.EducationalInstitution, 4)]
        public void Get_Authorized_ReturnsData(UserProfileType currentUserProfileType, int resultCount)
        {
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int educationalInstitution2Id = 2;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            var currentUserProfile = new CurrentUserProfileServiceFake();
            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Supervisors.AddRange(
                new Supervisor { Id = supervisorId, Code = "c", Name = "n" },
                new Supervisor { Id = supervisorId + 1, Code = "c", Name = "n" });

            db.EducationalInstitutions.AddRange(
                new EducationalInstitution 
                { 
                    Id = educationalInstitutionId, 
                    Code = "c", 
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId 
                },
                new EducationalInstitution 
                { 
                    Id = educationalInstitution2Id, 
                    Code = "c", 
                    Name = "n",
                    StatusId = statusClassifierId,
                    SupervisorId = supervisorId 
                });

            db.Classifiers.AddRange(
                new Classifier 
                { 
                    Type = "x",
                    Code = "", 
                    Value = "", 
                    PermissionType = UserProfileType.Country
                },
                new Classifier 
                { 
                    Type = "x", 
                    Code = "", 
                    Value = "", 
                    PermissionType = UserProfileType.Supervisor,
                    SupervisorId = supervisorId 
                },
                new Classifier 
                { 
                    Type = "x", 
                    Code = "", 
                    Value = "", 
                    PermissionType = UserProfileType.Supervisor,
                    SupervisorId = supervisorId + 1
                },
                new Classifier 
                { 
                    Type = "x", 
                    Code = "", 
                    Value = "", 
                    PermissionType = UserProfileType.EducationalInstitution,
                    EducationalInstitutionId = educationalInstitutionId
                },
                new Classifier 
                { 
                    Type = "x", 
                    Code = "", 
                    Value = "", 
                    PermissionType = UserProfileType.EducationalInstitution,
                    EducationalInstitutionId = educationalInstitution2Id
                });

            db.SaveChanges();

            var data = CreateService(db: db, currentUserProfileService: currentUserProfile).Get().List();

            Assert.Equal(resultCount, data.Count());
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            await CreateService(db).CreateAsync(new ClassifierCreateDto
            {
                Type = ClassifierTypes.ResourceSubType,
                Code = ResourceSubType.WindowsLaptop,
                Value = "lv",
                Payload = JsonSerializer.Serialize(new ResourceSubTypePayload())
            });

            Assert.True(db.Classifiers.Any(t => t.Code == ResourceSubType.WindowsLaptop));
        }

        //[Fact]
        //public async Task Create_AllowsMultipleEmptyCodes()
        //{
        //    const string type = "t1";

        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        db.Classifiers.Add(new Classifier { Type = ClassifierTypes.ClassifierType, Code = type, Value = "" });
        //        db.Classifiers.Add(new Classifier { Type = type, Code = "", Value = "" });

        //        db.SaveChanges();

        //        await CreateService(db).CreateAsync(new ClassifierCreateDto
        //        {
        //            Type = type,
        //            Value = "lv"
        //        });

        //        var count = db.Classifiers.Where(t => t.Type == type).Count();

        //        Assert.Equal(2, count);
        //    }
        //}

        //[Fact]
        //public async Task Create_CodeReserved_ThrowsValidation()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        //        {
        //            return CreateService(db).CreateAsync(new ClassifierCreateDto
        //            {
        //                Type = ClassifierTypes.ClassifierType,
        //                Code = ClassifierTypes.ClassifierType,
        //                Value = string.Empty
        //            });
        //        });

        //        Assert.Equal("classifier.codeReserved", ex.Message);
        //    }
        //}

        [Fact]
        public async Task Update_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();

            db.Classifiers.Add(new Classifier 
            { 
                Id = id, 
                Type = ClassifierTypes.ResourceSubType, 
                Code = ResourceSubType.WindowsLaptop,
                Value = string.Empty
            });

            db.SaveChanges();

            await CreateService(db).UpdateAsync(id, new ClassifierUpdateDto
            {
                Code = ResourceSubType.ChromebookLaptop,
                Value = "x",
                Payload = JsonSerializer.Serialize(new ResourceSubTypePayload())
            });

            var item = db.Classifiers.FirstOrDefault(t => t.Id == id);

            Assert.Equal(ResourceSubType.ChromebookLaptop, item.Code);
        }

        [Fact]
        public async Task Update_ThrowsNotFound()
        {
            using var db = ServiceFactory.ConnectDb();

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            {
                return CreateService(db).UpdateAsync(Guid.NewGuid(), new ClassifierUpdateDto());
            });
        }

        //[Fact]
        //public async Task Update_AllowsMultipleEmptyCodes()
        //{
        //    const string type = "t1";

        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        var id = Guid.NewGuid();

        //        db.Classifiers.AddRange(
        //            new Classifier { Type = ClassifierTypes.ClassifierType, Code = type, Value = "" },
        //            new Classifier { Type = type, Value = "" },
        //            new Classifier { Type = type, Id = id, Code = "x", Value = "" });
        //        db.SaveChanges();

        //        await CreateService(db).UpdateAsync(id, new ClassifierUpdateDto
        //        {
        //            Code = null,
        //            Value = "x"
        //        });

        //        var count = db.Classifiers.Select(t => t.Code).ToList().Count(t => string.IsNullOrEmpty(t));

        //        Assert.Equal(2, count);
        //    }
        //}

        [Fact]
        public async Task Update_Required_IgnoresEssentialProperties()
        {
            const string code = "t1";
            const string value = "v";
            const bool isDisabled = false;
            DateTime activeFrom = DateTime.Now;
            DateTime activeTo = activeFrom.AddDays(1);

            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = id,
                Type = ClassifierTypes.ClassifierType,
                Code = code,
                Value = "",
                ActiveFrom = activeFrom,
                ActiveTo = activeTo,
                IsDisabled = false,
                IsRequired = true
            });
            db.SaveChanges();

            await CreateService(db).UpdateAsync(id, new ClassifierUpdateDto
            {
                Value = value,
                Code = code + "_update",
                ActiveFrom = activeFrom.AddDays(1),
                ActiveTo = activeTo.AddDays(1),
                IsDisabled = !isDisabled
            });

            var entity = db.Classifiers.FirstOrDefault(t => t.Id == id);

            Assert.Equal(code, entity.Code);
            Assert.Equal(isDisabled, entity.IsDisabled);
            Assert.Equal(value, entity.Value);
            Assert.Equal(activeFrom, entity.ActiveFrom);
            Assert.Equal(activeTo, entity.ActiveTo);
        }

        //[Fact]
        //public async Task Code_IsSanitizedOnCreate()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        const string inputCode = "*)c1&-a .8 +~";
        //        const string finalCode = "c1-a.8";

        //        await CreateService(db).CreateAsync(new ClassifierCreateDto
        //        {
        //            Code = inputCode,
        //            Type = ClassifierTypes.ClassifierType,
        //            Value = "x"
        //        });

        //        var exists = db.Classifiers.Any(t => t.Code == finalCode);

        //        Assert.True(exists);
        //    }
        //}

        //[Fact]
        //public async Task Code_IsSanitizedOnUpdate()
        //{
        //    using (var db = ServiceFactory.ConnectDb())
        //    {
        //        const string inputCode = "*)c1&-a .8 +~";
        //        const string finalCode = "c1-a.8";

        //        var id = Guid.NewGuid();

        //        db.Classifiers.AddRange(new Classifier 
        //        { 
        //            Id = id,
        //            Type = ClassifierTypes.ClassifierType,
        //            Value = string.Empty 
        //        });
        //        db.SaveChanges();

        //        await CreateService(db).UpdateAsync(id, new ClassifierUpdateDto
        //        {
        //            Code = inputCode,
        //            Value = "x"
        //        });

        //        var exists = db.Classifiers.Any(t => t.Code == finalCode);

        //        Assert.True(exists);
        //    }
        //}

        [Fact]
        public async Task Delete_Succeeds()
        {
            using var db = ServiceFactory.ConnectDb();

            var id = Guid.NewGuid();

            db.Classifiers.Add(new Classifier { Id = id, Type = ClassifierTypes.ClassifierType, Code = "", Value = "" });
            db.SaveChanges();

            await CreateService(db).DeleteAsync(id);

            var exists = db.Classifiers.Any(t => t.Id == id);

            Assert.False(exists);
        }

        [Fact]
        public async Task Delete_Required_ThrowsInvalidOperation()
        {
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier { Id = id, Type = ClassifierTypes.ClassifierType, Code = "", Value = "", IsRequired = true });
            db.SaveChanges();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return CreateService(db).DeleteAsync(id);
            });

            Assert.Equal(ClassifierService.Error.CannotDeleteRequired, ex.Message);
        }

        [Fact]
        public async Task Delete_TypeNotEmpty_ThrowsInvalidOperation()
        {
            const string type = "t1";
            var id = Guid.NewGuid();

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier { Id = id, Type = ClassifierTypes.ClassifierType, Code = type, Value = "" });
            db.Classifiers.Add(new Classifier { Type = type, Code = "", Value = "" });
            db.SaveChanges();

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return CreateService(db).DeleteAsync(id);
            });

            Assert.Equal(ClassifierService.Error.CannotDeleteTypeNotEmpty, ex.Message);
        }

        [Fact]
        public async Task Delete_ThrowsNotFound()
        {
            using var db = ServiceFactory.ConnectDb();

            await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            {
                return CreateService(db).DeleteAsync(Guid.NewGuid());
            });
        }

        private ClassifierService CreateService(
            AppDbContext db,
            IClassifierValidator validator = null,
            IAuthorizationService authorizationService = null,
            ICurrentUserProfileService currentUserProfileService = null)
        {
            return new ClassifierService(
                db, 
                validator: validator ?? ServiceFactory.CreateClassifierValidator(),
                authorizationService: authorizationService ?? ServiceFactory.CreateAuthorizationService(),
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService());
        }
    }
}
