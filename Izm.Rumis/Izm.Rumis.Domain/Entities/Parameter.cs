using System.ComponentModel.DataAnnotations;

namespace Izm.Rumis.Domain.Entities
{
    public class Parameter : Entity<int>
    {
        [Required]
        [MaxLength(100)]
        public string Code { get; set; }

        [Required]
        [MaxLength(250)]
        public string Value { get; set; }
    }

    public static class ParameterCode
    {
        public const string AppTitle = "app_title";
        public const string AppUrl = "app_url";
        public const string DataImportMaxSize = "data_import_max_size";
        public const string DataImportExtensions = "data_import_extensions";
        public const string DataVersion = "data_version";
        public const string DocumentTemplateMaxSize = "doc_template_max_size";
        public const string DocumentTemplateExtensions = "doc_template_extensions";

        public const string ApplicationAttachmentMaxSize = "application_attachment_max_size";
        public const string PageSize = "page_size";
        public const string TaskManagerIntervalInMinutes = "task_manager_interval_in_minutes";

        public const string ResourceImportMaxErrorCount = "resource_import_max_error_count";

        // TASKS

        // Application monitoring task
        public const string ApplicationMonitoringTaskEnabled = "application_monitoring_task_enabled";
        public const string ApplicationMonitoringTaskIntervalInMinutes = "application_monitoring_task_interval_in_minutes";
        public const string ApplicationMonitoringTaskStartTime = "application_monitoring_task_start_time";

        // Clear logs task
        public const string ClearLogsTaskEnabled = "clear_logs_task_enabled";
        public const string ClearLogsTaskIntervalInMinutes = "clear_logs_task_interval_in_minutes";
        public const string ClearLogsTaskStartTime = "clear_logs_task_start_time";
        public const string ClearLogsTaskLogValidInDays = "clear_logs_task_log_valid_in_days";

        // Clear gdpr audits task
        public const string ClearGdprAuditsTaskEnabled = "clear_gdpr_audits_task_enabled";
        public const string ClearGdprAuditsTaskIntervalInMinutes = "clear_gdpr_audits_task_interval_in_minutes";
        public const string ClearGdprAuditsTaskStartTime = "clear_gdpr_audits_task_start_time";
        public const string ClearGdprAuditsTaskGdprValidInDays = "clear_gdpr_audits_task_gdpr_valid_in_days";

        // Educational institution synchronization task
        public const string EducationalInstitutionTaskEnabled = "educational_institution_task_enabled";
        public const string EducationalInstitutionTaskIntervalInMinutes = "educational_institution_task_interval_in_minutes";
        public const string EducationalInstitutionTaskStartTime = "educational_institution_task_start_time";

        // Session garage collection task
        public const string SessionGarbageCollectionTaskEnabled = "session_gargabe_collection_task_enabled";
        public const string SessionGarbageCollectionTaskIntervalInMinutes = "session_gargabe_collection_task_interval_in_minutes";
        public const string SessionGarbageCollectionTaskStartTime = "session_gargabe_collection_task_start_time";
    }
}
