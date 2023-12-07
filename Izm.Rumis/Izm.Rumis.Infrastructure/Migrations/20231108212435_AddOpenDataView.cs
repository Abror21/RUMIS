using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Izm.Rumis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenDataView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE OR REPLACE VIEW open_data AS " +
                "SELECT " +
                "   EducationalInstitutions.Name AS 'Izglītības iestāde', " +
                "   Supervisors.Name AS 'Vadošā iestāde', " +
                "   (" +
                "       SELECT ResourceType.Value " +
                "       FROM Classifiers AS ResourceType " +
                "       WHERE ResourceType.`Type` = 'resource_type' && ResourceType.Code = JSON_UNQUOTE(JSON_EXTRACT(ResourceSubType.Payload, '$.resource_type')) " +
                "       LIMIT 1" +
                "   ) AS 'Resursa veids', " +
                "   ResourceSubType.Value AS 'Resursa paveids', " +
                "   DATE(Applications.ApplicationDate) AS 'Datums', " +
                "   COUNT(*) AS 'Resursa pieteikumi', " +
                "   COUNT(CASE WHEN ApplicationStatus.Code = 'confirmed' THEN 1 ELSE NULL END) AS 'Piešķirti', " +
                "   COUNT(CASE WHEN ApplicationStatus.Code = 'declined' THEN 1 ELSE NULL END) AS 'Atteikti', " +
                "   COUNT(CASE WHEN ApplicationStatus.Code = 'postponed' THEN 1 ELSE NULL END) AS 'Atlikta piešķiršana' " +
                "FROM Applications " +
                "INNER JOIN EducationalInstitutions " +
                "   ON Applications.EducationalInstitutionId = EducationalInstitutions.Id " +
                "INNER JOIN Supervisors " +
                "   ON EducationalInstitutions.SupervisorId = Supervisors.Id " +
                "INNER JOIN Classifiers AS ResourceSubType " +
                "   ON Applications.ResourceSubTypeId = ResourceSubType.Id " +
                "INNER JOIN Classifiers AS ApplicationStatus " +
                "   ON Applications.ApplicationStatusId = ApplicationStatus.Id " +
                "GROUP BY EducationalInstitutions.Name, ResourceSubType.Value, DATE(Applications.ApplicationDate) " +
                "ORDER BY DATE(Applications.ApplicationDate) ASC;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW open_data");
        }
    }
}
