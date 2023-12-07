using Izm.Rumis.Application.Exceptions;
using Izm.Rumis.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionMiddleware> logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var exData = AnalyzeException(exception);

            if (exData == null && exception.InnerException != null)
            {
                var innerExData = AnalyzeException(exception.InnerException);

                if (innerExData != null)
                    exData = innerExData;
            }

            if (exData == null)
                exData = new ExceptionData
                {
                    Code = HttpStatusCode.InternalServerError
                };

            logger.LogError(exception, exception.Message + (exData.Details != null ? string.Join("; ", exData.Details) : string.Empty));

            if (exception.InnerException != null)
                logger.LogError(exception.InnerException, exception.InnerException.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)exData.Code;

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                message = string.IsNullOrEmpty(exData.Message) ? "error.unknown" : exData.Message,
                details = (exData.Details ?? new string[] { }).Where(t => t != null).ToArray(),
                status = (int)exData.Code,
                traceId = context.TraceIdentifier
            }, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }

        private ExceptionData AnalyzeException(Exception exception)
        {
            HttpStatusCode code;
            string userMessage;
            IEnumerable<object> details = null;

            switch (exception)
            {
                case UnauthorizedAccessException unauthorizedEx:
                    code = HttpStatusCode.Forbidden;
                    userMessage = exception.Message ?? "error.accessDenied";
                    break;

                case InvalidOperationException invalidOperationEx:
                    code = HttpStatusCode.BadRequest;
                    userMessage = invalidOperationEx.Message ?? "error.invalidOperation";
                    break;

                // custom exceptions

                case DatabaseException dbEx:
                    code = HttpStatusCode.InternalServerError;
                    userMessage = "error.dbUpdate";

                    var inner = dbEx.InnerException;

                    while (inner.InnerException != null)
                        inner = inner.InnerException;

                    if (inner != null)
                    {
                        var exMessage = (inner.Message ?? string.Empty).ToLower();

                        if (exMessage.Contains("delete") && exMessage.Contains("reference"))
                            userMessage = "error.deleteReference";
                    }

                    break;

                case AccessDeniedException accessDeniedEx:
                    code = HttpStatusCode.Forbidden;
                    userMessage = accessDeniedEx.Message ?? "error.accessDenied";
                    break;

                case EntityNotFoundException entityNotFoundEx:
                    code = HttpStatusCode.NotFound;
                    userMessage = entityNotFoundEx.Message ?? "error.notFound";
                    break;

                case ReadOnlyException readOnlyEx:
                    code = HttpStatusCode.Forbidden;
                    userMessage = readOnlyEx.Message ?? "error.readOnly";
                    break;

                case NotSupportedException notSupportedEx:
                    code = HttpStatusCode.BadRequest;
                    userMessage = notSupportedEx.Message ?? "error.notSupported";
                    break;

                case ValidationException validationEx:
                    code = HttpStatusCode.BadRequest;
                    userMessage = validationEx.Message ?? "error.validation";
                    details = validationEx.Data;
                    break;

                case TemplateEmptyException templateEmptyEx:
                    code = HttpStatusCode.NotFound;
                    userMessage = templateEmptyEx.Message ?? "error.emptyTemplate";
                    details = new string[] { templateEmptyEx.Code };
                    break;

                default:
                    return null;
            }

            return new ExceptionData
            {
                Code = code,
                Message = userMessage,
                Details = details
            };
        }

        private class ExceptionData
        {
            public HttpStatusCode Code { get; set; }
            public string Message { get; set; }
            public IEnumerable<object> Details { get; set; }
        }
    }
}
