

using System.Collections.Generic;

namespace Izm.Rumis.Domain.Constants
{
    public static class ClassifierTypes
    {
        // define known classifier types here
        public const string ApplicantRole = "applicant_role";
        public const string ApplicationCancelReason = "application_cancel_reason";
        public const string ApplicationStatus = "application_status";
        public const string ClassifierGroup = "classifier_group";
        public const string ClassifierType = "classifier_type";
        public const string ContactType = "contact_type";
        public const string DocumentTag = "document_tag";
        public const string DocumentType = "document_type";
        public const string EducationalInstitutionJobPosition = "educational_institution_job_position";
        public const string EducationalInstitutionStatus = "educational_institution_status";
        public const string Institution = "institution";
        public const string NotificationType = "notification_type";
        public const string PersonalDataSpecialistReasonForRequest = "personal_data_specialist_reason_for_request";
        public const string Placeholder = "placeholder";
        public const string PnaCancelingReason = "pna_canceling_reason";
        public const string PnaStatus = "pna_status";
        public const string ResourceAcquisitionType = "resource_acquisition_type";
        public const string ResourceBodyType = "resource_body_type";
        public const string ResourceGroup = "resource_group";
        public const string ResourceLocation = "resource_location";
        public const string ResourceManufacturer = "resource_manufacturer";
        public const string ResourceModelName = "resource_model_name";
        public const string ResourceParameter = "resource_parameter";
        public const string ResourceParameterGroup = "resource_parameter_group";
        public const string ResourceParameterUnitOfMeasurement = "resource_parameter_unit_of_measurement";
        public const string ResourceReturnStatus = "resource_return_status";
        public const string ResourceStatus = "resource_status";
        public const string ResourceSubType = "resource_subtype";
        public const string ResourceTargetPersonEducationalStatus = "resource_target_person_educational_status";
        public const string ResourceTargetPersonEducationalSubStatus = "resource_target_person_educational_sub_status";
        public const string ResourceTargetPersonType = "resource_target_person_type";
        public const string ResourceTargetPersonWorkStatus = "resource_target_person_work_status";
        public const string ResourceType = "resource_type";
        public const string ResourceUniversal = "resource_universal";
        public const string ResourceUsingPurpose = "resource_using_purpose";
        public const string SocialStatus = "social_status";
        public const string TargetGroup = "target_group";

        public static IEnumerable<string> RequiredStatuses => new string[]
        {
            ClassifierGroup,
            ClassifierType
        };
    }
}
