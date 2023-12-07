using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Application.Services;
using Izm.Rumis.Application.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace Izm.Rumis.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Services
            services.AddScoped<IClassifierService, ClassifierService>();
            services.AddScoped<IDocumentTemplateService, DocumentTemplateService>();
            services.AddScoped<ISupervisorService, SupervisorService>();
            services.AddScoped<IParameterService, ParameterService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IEducationalInstitutionService, EducationalInstitutionService>();
            services.AddScoped<ITextTemplateService, TextTemplateService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IApplicationAttachmentService, ApplicationAttachmentService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IApplicationResourceService, ApplicationResourceService>();
            services.AddScoped<IGdprAuditService, GdprAuditService>();
            services.AddScoped<IPersonDataReportService, PersonDataReportService>();
            services.AddScoped<IApplicationDuplicateService, ApplicationDuplicateService>();

            // Validators
            services.AddScoped<IPersonValidator, PersonValidator>();
            services.AddScoped<IClassifierValidator, ClassifierValidator>();
            services.AddScoped<IApplicationValidator, ApplicationValidator>();
            services.AddScoped<IApplicationAttachmentValidator, ApplicationAttachmentValidator>();
            services.AddScoped<IApplicationResourceValidator, ApplicationResourceValidator>();
            services.AddScoped<IGdprAuditValidator, GdprAuditValidator>();
            services.AddScoped<IDocumentTemplateValidator, DocumentTemplateValidator>();
            services.AddScoped<IApplicationResourceAttachmentValidator, ApplicationResourceAttachmentValidator>();
            services.AddScoped<IResourceValidator, ResourceValidator>();
            services.AddScoped<IPersonDataReportValidator, PersonDataReportValidator>();

            return services;
        }
    }
}
