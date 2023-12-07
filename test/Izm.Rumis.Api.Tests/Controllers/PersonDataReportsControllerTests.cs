using Izm.Rumis.Api.Controllers;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Api.Tests.Setup.Common;
using Izm.Rumis.Api.Tests.Setup.Services;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Izm.Rumis.Api.Tests.Controllers
{
    public sealed class PersonDataReportsControllerTests
    {
        private PersonDataReportGenerateRequest personDataReportGenerateRequest { get; } = new PersonDataReportGenerateRequest
        {
            Notes = "someNotes",
            DataOwnerPrivatePersonalIdentifier = "00000000001",
            ReasonId = Guid.NewGuid()
        };

        [Fact]
        public async Task Generate_ForwardsDataToService()
        {
            // Assign
            var personDataReportService = ServiceFactory.CreatePersonDataReportService();

            var controller = GetController(personDataReportService);

            // Act
            _ = await controller.Generate(personDataReportGenerateRequest);

            var with = personDataReportService.GenerateAsyncCalledWith;

            // Assert
            Assert.Equal(with.Notes, personDataReportGenerateRequest.Notes);
            Assert.Equal(with.DataOwnerPrivatePersonalIdentifier, personDataReportGenerateRequest.DataOwnerPrivatePersonalIdentifier);
            Assert.Equal(with.ReasonId, personDataReportGenerateRequest.ReasonId);
        }

        [Theory]
        [InlineData(3, null, null)]
        [InlineData(2, "2000-01-01", null)]
        [InlineData(2, null, "3000-01-01")]
        [InlineData(1, "2000-01-01", "3000-01-01")]
        public async Task Generate_ReturnsData(int resultCount, string dateFrom, string dateTo)
        {
            // Assign
            var personDataReportService = ServiceFactory.CreatePersonDataReportService();

            personDataReportService.GenerateData = new TestAsyncEnumerable<GdprAudit>(new List<GdprAudit>()
            {
                new GdprAudit { Created = DateTime.Parse("1500-01-01") },
                new GdprAudit { Created = DateTime.Parse("2500-01-01") },
                new GdprAudit { Created = DateTime.Parse("3500-01-01") }
            });

            var request = new PersonDataReportGenerateRequest
            {
                DateFrom = dateFrom == null ? null : DateTime.Parse(dateFrom),
                DateTo = dateTo == null ? null : DateTime.Parse(dateTo),
                Notes = "someNotes",
                DataOwnerPrivatePersonalIdentifier = "00000000000",
                ReasonId = Guid.NewGuid()
            };

            var controller = GetController(personDataReportService);

            // Act
            var result = await controller.Generate(request);

            // Assert
            Assert.Equal(resultCount, result.Value.Count());
        }

        [Fact]
        public async Task Generate_Throws_InvalidDateRange()
        {
            // Assign
            var personDataReportService = ServiceFactory.CreatePersonDataReportService();

            var controller = GetController(personDataReportService);

            var request = new PersonDataReportGenerateRequest
            {
                DateFrom = DateTime.Parse("2000-01-01"),
                DateTo = DateTime.Parse("1000-01-01"),
                Notes = "someNotes",
                DataOwnerPrivatePersonalIdentifier = "00000000000",
                ReasonId = Guid.NewGuid()
            };

            // Act & Assert
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                controller.Generate(request)
                );

            // Assert
            Assert.Equal(PersonDataReportsController.Error.InvalidDateRange, result.Message);
        }

        private PersonDataReportsController GetController(IPersonDataReportService personDataReportService = null)
        {
            return new PersonDataReportsController(
                personDataReportService ?? ServiceFactory.CreatePersonDataReportService()
                );
        }
    }
}
