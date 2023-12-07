using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Izm.Rumis.Domain.Constants
{
    public static class Permission
    {
        public const string ApplicationEdit = "application.edit";
        public const string ApplicationView = "application.view";

        public const string ApplicationResourceEdit = "application_resource.edit";
        public const string ApplicationResourceReassign = "application_resource.reassign";
        public const string ApplicationResourceView = "application_resource.view";

        public const string ClassifierEdit = "classifier.edit";
        public const string ClassifierView = "classifier.view";

        public const string DocumentTemplateEdit = "document_template.edit";
        public const string DocumentTemplateView = "document_template.view";

        public const string EducationalInstitutionEdit = "educational_institution.edit";
        public const string EducationalInstitutionView = "educational_institution.view";

        public const string LogEdit = "log.edit";
        public const string LogView = "log.view";

        public const string ParameterEdit = "parameter.edit";
        public const string ParameterView = "parameter.view";

        public const string PersonDataReportView = "person_data_report.view";

        public const string ReportEdit = "report.edit";
        public const string ReportView = "report.view";

        public const string ResourceEdit = "resource.edit";
        public const string ResourceView = "resource.view";

        public const string RoleEdit = "role.edit";
        public const string RoleView = "role.view";

        public const string SupervisorEdit = "supervisor.edit";
        public const string SupervisorView = "supervisor.view";

        public const string TextTemplateEdit = "text_template.edit";
        public const string TextTemplateView = "text_template.view";

        public const string UserEdit = "user.edit";
        public const string UserView = "user.view";

        public const string UserProfileEdit = "user_person.edit";
        public const string UserProfileView = "user_person.view";

        public const string ViisServicesView = "viis_services.view";

        public static IEnumerable<string> All => GetAll();

        public static IEnumerable<string> GetAll()
        {
            var type = typeof(Permission);
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(t => t.IsLiteral && !t.IsInitOnly).Select(t => t.GetRawConstantValue() as string).ToList();
        }
    }
}
