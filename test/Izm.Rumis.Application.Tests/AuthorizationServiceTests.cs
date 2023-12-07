using Izm.Rumis.Application.Common;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Tests.Common;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Izm.Rumis.Domain.Enums;
using System;
using Xunit;

namespace Izm.Rumis.Application.Tests
{
    public class AuthorizationServiceTests
    {
        private CurrentUserProfileServiceFake currentUserProfile;

        public AuthorizationServiceTests()
        {
            currentUserProfile = new CurrentUserProfileServiceFake();
        }

        [Theory]
        [InlineData(UserProfileType.Country, UserProfileType.Country)]
        [InlineData(UserProfileType.Country, UserProfileType.Supervisor)]
        [InlineData(UserProfileType.Country, UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor, UserProfileType.Supervisor)]
        [InlineData(UserProfileType.Supervisor, UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.EducationalInstitution)]
        public void Authorize_Succeeds(UserProfileType currentUserProfileType, UserProfileType permissionType)
        {
            // Assign
            const int supervisorId = 1;
            const int educationalInstitutionId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.SaveChanges();

            var item = new AuthorizedResource
            {
                PermissionType = permissionType,
                SupervisorId = supervisorId,
                EducationalInstitutionId = educationalInstitutionId
            };

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(item);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(UserProfileType.Country, UserProfileType.Country)]
        [InlineData(UserProfileType.Country, UserProfileType.Supervisor)]
        [InlineData(UserProfileType.Country, UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor, UserProfileType.Supervisor)]
        [InlineData(UserProfileType.Supervisor, UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.EducationalInstitution)]
        public void Authorize_CreateDto_Succeeds(UserProfileType currentUserProfileType, UserProfileType permissionType)
        {
            // Assign
            const int supervisorId = 1;
            const int educationalInstitutionId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.SaveChanges();

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = permissionType,
                SupervisorId = supervisorId,
                EducationalInstitutionId = educationalInstitutionId
            };

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(item);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(UserProfileType.Country)]
        [InlineData(UserProfileType.Supervisor)]
        [InlineData(UserProfileType.EducationalInstitution)]
        public void Authorize_EditDto_Succeeds(UserProfileType currentUserProfileType)
        {
            // Assign
            const int supervisorId = 1;
            const int educationalInstitutionId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.SaveChanges();

            var item = new AuthorizedResourceEditDto
            {
                SupervisorId = supervisorId,
                EducationalInstitutionId = educationalInstitutionId
            };

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(item);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(UserProfileType.Country)]
        [InlineData(UserProfileType.Supervisor)]
        [InlineData(UserProfileType.EducationalInstitution)]
        public void Authorize_EducationalInstitutionId_Succeeds(UserProfileType currentUserProfileType)
        {
            // Assign
            const int supervisorId = 1;
            const int educationalInstitutionId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            using var db = ServiceFactory.ConnectDb();

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.SaveChanges();

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(educationalInstitutionId);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Authorize_Throws_SupervisorIdRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = UserProfileType.Supervisor
            };

            var service = CreateService(db);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorIdRequired, result.Message);
        }

        [Fact]
        public void Authorize_DocumentTemplate_Throws_SupervisorIdRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = new AuthorizedDocumentTemplateEditDto
            {
                PermissionType = UserProfileType.Supervisor
            };

            var service = CreateService(db);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorIdRequired, result.Message);
        }

        [Fact]
        public void Authorize_DocumentTemplate_Throws_SupervisorForbidden_PermissionType()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = 1;

            var item = new AuthorizedDocumentTemplateEditDto
            {
                PermissionType = UserProfileType.Country
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorForbidden, result.Message);
        }

        [Fact]
        public void Authorize_DocumentTemplate_Throws_SupervisorForbidden_SupervisorId()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = 1;

            var item = new AuthorizedDocumentTemplateEditDto
            {
                PermissionType = UserProfileType.Supervisor,
                SupervisorId = 2
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorForbidden, result.Message);
        }

        [Fact]
        public void Authorize_DocumentTemplate_Throws_PermissionTypeForbidden()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            currentUserProfile.Type = UserProfileType.EducationalInstitution;

            var item = new AuthorizedDocumentTemplateEditDto
            {
                PermissionType = UserProfileType.EducationalInstitution
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.PermissionTypeForbidden, result.Message);
        }


        [Theory]
        [InlineData(UserProfileType.Country)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_DocumentTemplate_Succeeds(UserProfileType currentUserProfileType)
        {
            // Assign
            const int supervisorId = 1;

            currentUserProfile.Type = currentUserProfileType;

            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;


            using var db = ServiceFactory.ConnectDb();

            var item = new AuthorizedDocumentTemplateEditDto
            {
                PermissionType = currentUserProfileType,
                SupervisorId = supervisorId

            };

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(item);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void Authorize_Throws_EducationalInstitutionIdRequired()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = UserProfileType.EducationalInstitution
            };

            var service = CreateService(db);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.EducationalInstitutionIdRequired, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.Supervisor, UserProfileType.Country)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.Country)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.Supervisor)]
        public void Authorize_Throws_PermissionTypeForbidden(UserProfileType currentUserProfileType, UserProfileType permissionType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int supervisorId = 1;

            currentUserProfile.Type = currentUserProfileType;

            var item = new AuthorizedResource
            {
                PermissionType = permissionType
            };

            if (permissionType == UserProfileType.Supervisor)
            {
                currentUserProfile.SupervisorId = supervisorId;
                item.SupervisorId = supervisorId;
            }

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.PermissionTypeForbidden, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.Supervisor, UserProfileType.Country)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.Country)]
        [InlineData(UserProfileType.EducationalInstitution, UserProfileType.Supervisor)]
        public void Authorize_CreateDto_Throws_PermissionTypeForbidden(UserProfileType currentUserProfileType, UserProfileType permissionType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int supervisorId = 1;

            currentUserProfile.Type = currentUserProfileType;

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = permissionType
            };

            if (permissionType == UserProfileType.Supervisor)
            {
                currentUserProfile.SupervisorId = supervisorId;
                item.SupervisorId = supervisorId;
            }

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.PermissionTypeForbidden, result.Message);
        }

        [Fact]
        public void Authorize_Throws_SupervisorForbidden()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int supervisorId = 1;

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = supervisorId;

            var item = new AuthorizedResource
            {
                PermissionType = UserProfileType.Supervisor,
                SupervisorId = supervisorId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorForbidden, result.Message);
        }

        [Fact]
        public void Authorize_CreateDto_Throws_SupervisorForbidden()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int supervisorId = 1;

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = supervisorId;

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = UserProfileType.Supervisor,
                SupervisorId = supervisorId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorForbidden, result.Message);
        }

        [Fact]
        public void Authorize_EditDto_Throws_SupervisorForbidden()
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int supervisorId = 1;

            currentUserProfile.Type = UserProfileType.Supervisor;
            currentUserProfile.SupervisorId = supervisorId;

            var item = new AuthorizedResourceEditDto
            {
                SupervisorId = supervisorId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.SupervisorForbidden, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_Throws_EducationalInstitutionForbidden(UserProfileType currentUserProfileType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;
            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId + 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId + 1
            });

            db.SaveChanges();

            var item = new AuthorizedResource
            {
                PermissionType = UserProfileType.EducationalInstitution,
                EducationalInstitutionId = educationalInstitutionId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.EducationalInstitutionForbidden, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_CreateDto_Throws_EducationalInstitutionForbidden(UserProfileType currentUserProfileType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;
            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId + 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId + 1
            });

            db.SaveChanges();

            var item = new AuthorizedResourceCreateDto
            {
                PermissionType = UserProfileType.EducationalInstitution,
                EducationalInstitutionId = educationalInstitutionId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.EducationalInstitutionForbidden, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_EditDto_Throws_EducationalInstitutionForbidden(UserProfileType currentUserProfileType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;
            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId + 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId + 1
            });

            db.SaveChanges();

            var item = new AuthorizedResourceEditDto
            {
                EducationalInstitutionId = educationalInstitutionId + 1
            };

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(item));

            // Assert
            Assert.Equal(AuthorizationService.Error.EducationalInstitutionForbidden, result.Message);
        }

        [Theory]
        [InlineData(UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_EducationalInstitutionId_UserProfileNotInitialized(UserProfileType currentUserProfileType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;
            currentUserProfile.IsInitialized = false;
            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId + 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId + 1
            });

            db.SaveChanges();

            var service = CreateService(db, currentUserProfile);

            // Act
            service.Authorize(educationalInstitutionId + 1);

            // Assert
            Assert.True(true);
        }

        [Theory]
        [InlineData(UserProfileType.EducationalInstitution)]
        [InlineData(UserProfileType.Supervisor)]
        public void Authorize_EducationalInstitutionId_Throws_EducationalInstitutionForbidden(UserProfileType currentUserProfileType)
        {
            // Assign
            using var db = ServiceFactory.ConnectDb();

            const int educationalInstitutionId = 1;
            const int supervisorId = 1;
            var statusClassifierId = Guid.NewGuid();

            currentUserProfile.Type = currentUserProfileType;
            currentUserProfile.IsInitialized = true;
            if (currentUserProfileType == UserProfileType.Supervisor)
                currentUserProfile.SupervisorId = supervisorId;

            if (currentUserProfileType == UserProfileType.EducationalInstitution)
                currentUserProfile.EducationalInstitutionId = educationalInstitutionId;

            db.Classifiers.Add(new Classifier
            {
                Id = statusClassifierId,
                Code = "c",
                Value = "v",
                Type = ClassifierTypes.EducationalInstitutionStatus
            });

            db.SaveChanges();

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId
            });

            db.Add(new EducationalInstitution
            {
                Id = educationalInstitutionId + 1,
                Code = "c",
                Name = "n",
                StatusId = statusClassifierId,
                SupervisorId = supervisorId + 1
            });

            db.SaveChanges();

            var service = CreateService(db, currentUserProfile);

            // Act & Assert
            var result = Assert.Throws<UnauthorizedAccessException>(() => service.Authorize(educationalInstitutionId + 1));

            // Assert
            Assert.Equal(AuthorizationService.Error.EducationalInstitutionForbidden, result.Message);
        }


        public class AuthorizedResource : IAuthorizedResource
        {
            public UserProfileType PermissionType { get; set; }
            public int? SupervisorId { get; set; }
            public int? EducationalInstitutionId { get; set; }
        }

        public class AuthorizedResourceCreateDto : IAuthorizedResourceCreateDto
        {
            public UserProfileType PermissionType { get; set; }
            public int? SupervisorId { get; set; }
            public int? EducationalInstitutionId { get; set; }
        }

        public class AuthorizedDocumentTemplateEditDto : IAuthorizedDocumentTemplateEditDto
        {
            public UserProfileType PermissionType { get; set; }
            public int? SupervisorId { get; set; }
            public int? EducationalInstitutionId { get; set; }
        }

        public class AuthorizedResourceEditDto : IAuthorizedResourceEditDto
        {
            public int? SupervisorId { get; set; }
            public int? EducationalInstitutionId { get; set; }
        }

        private AuthorizationService CreateService(
            IAppDbContext db,
            ICurrentUserProfileService currentUserProfileService = null,
            IPersonDataService personDataService = null,
            IGdprAuditService gdprAuditService = null)
        {
            return new AuthorizationService(
                db,
                currentUserProfile: currentUserProfileService ?? ServiceFactory.CreateCurrentUserProfileService(),
                personDataService: personDataService ?? ServiceFactory.CreatePersonDataService(),
                gdprAuditService: gdprAuditService ?? ServiceFactory.CreateGdprAuditService()
                );
        }
    }
}
