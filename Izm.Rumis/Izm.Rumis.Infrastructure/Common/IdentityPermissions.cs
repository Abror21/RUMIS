using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Izm.Rumis.Infrastructure.Common
{
    public static class IdentityPermissions
    {
        public const string ApplicationView = "application.view";
        public const string ApplicationEdit = "application.edit";

        public const string ClassifierView = "classifier.view";
        public const string ClassifierEdit = "classifier.edit";

        public const string DocumentTemplateView = "document_template.view";
        public const string DocumentTemplateEdit = "document_template.edit";

        public const string ParameterView = "parameter.view";
        public const string ParameterEdit = "parameter.edit";

        public const string ResourceView = "resource.view";
        public const string ResourceEdit = "resource.edit";

        public const string TextTemplateView = "text_template.view";
        public const string TextTemplateEdit = "text_template.edit";

        public const string UserView = "user.view";
        public const string UserEdit = "user.edit";

        public const string UserProfileView = "user_profile.view";
        public const string UserProfileEdit = "user_profile.edit";

        public const string EducationalInstitutionView = "EducationalInstitution.view";
        public const string EducationalInstitutionEdit = "EducationalInstitution.edit";

        public const string SupervisorView = "Supervisor.view";
        public const string SupervisorEdit = "Supervisor.edit";

        public const string IdentityRoleView = "identity_role.view";
        public const string IdentityRoleEdit = "identity_role.edit";

        public const string RoleView = "role.view";
        public const string RoleEdit = "role.edit";

        public const string LogView = "log.view";

        // add custom permissions here

        public static IEnumerable<string> All => GetAll();

        public static IEnumerable<string> GetAll()
        {
            var type = typeof(IdentityPermissions);
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(t => t.IsLiteral && !t.IsInitOnly).Select(t => t.GetRawConstantValue() as string).ToList();
        }
    }
}
