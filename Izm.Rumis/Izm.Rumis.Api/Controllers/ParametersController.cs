using Izm.Rumis.Api.Attributes;
using Izm.Rumis.Api.Models;
using Izm.Rumis.Application.Contracts;
using Izm.Rumis.Domain.Constants;
using Izm.Rumis.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Izm.Rumis.Api.Controllers
{
    public class ParametersController : ApiController
    {
        private readonly IParameterService service;
        private readonly ICurrentUserProfileService currentUserProfile;

        public ParametersController(IParameterService service, ICurrentUserProfileService currentUserProfile)
        {
            this.service = service;
            this.currentUserProfile = currentUserProfile;
        }

        /// <summary>
        /// Parameters visible to any user
        /// </summary>
        public readonly string[] PublicParameters = new string[] {
            ParameterCode.AppTitle,
            ParameterCode.AppUrl,
            ParameterCode.PageSize
        };

        /// <summary>
        /// Additional visible parameters to authenticated user
        /// </summary>
        public readonly string[] PrivateParameters = new string[] {
            // add private parameters here
            // ParameterCodes.XXX
        };

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ParameterModel>>> Get(CancellationToken cancellationToken = default)
        {
            var data = await service.Get().OrderBy(t => t.Code).ListAsync(map: t => new ParameterModel
            {
                Code = t.Code,
                Id = t.Id,
                Value = t.Value
            }, cancellationToken: cancellationToken);

            var result = new List<ParameterModel>();

            // TODO: permission handling
            if (currentUserProfile.HasPermission(Permission.ParameterView))
            {
                // can view all parameters
                result.AddRange(data);
            }
            else
            {
                result.AddRange(data.Where(t => PublicParameters.Contains(t.Code)));

                if (User.Identity.IsAuthenticated)
                    result.AddRange(data.Where(t => PrivateParameters.Contains(t.Code)));
            }

            return result.Distinct().ToList();
        }

        [HttpPut("{id}")]
        [PermissionAuthorize(Permission.ParameterEdit)]
        public async Task<ActionResult> Put(int id, ParameterUpdateModel model, CancellationToken cancellationToken = default)
        {
            await service.UpdateAsync(id, model.Value, cancellationToken);
            return NoContent();
        }
    }
}
