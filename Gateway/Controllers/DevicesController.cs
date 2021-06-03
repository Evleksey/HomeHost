using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Gateway.Models;
using Gateway.Managers;
using Gateway.Utils;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Gateway.Controllers
{
    [ApiController]
    [Route("device")]
    public class DevicesController : HostBaseController
    {
        private readonly IConfiguration _configuration;

        public DevicesController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

    

        [Authorize]
        [AcceptVerbs("POST", "OPTIONS")]
        [Route("set/{id}/{state}")]
        public async Task<ActionResult> SetState(int id,  bool state)
        {
            if (!User.HasClaim(c => c.Type == ClaimsIdentity.DefaultRoleClaimType))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }

            string allowed = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Select(c => c.Value).First();

            var dm = new DeviceManager(_configuration);

            var result = await dm.SetState(id, state, allowed);

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

        [Authorize]
        [AcceptVerbs("POST", "OPTIONS")]
        [Route("set/{id}")]
        public async Task<ActionResult> SetDevice([FromBody] APIDevice model)
        {
            if (!User.HasClaim(c => c.Type == ClaimsIdentity.DefaultRoleClaimType && c.Value == "admin"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }

            var dm = new DeviceManager(_configuration);

            var result = dm.SetDevice(model);

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

        [Authorize]
        [AcceptVerbs("GET", "OPTIONS")]
        [Route("search")]
        public async Task<ActionResult> Search()
        {
            if (!User.HasClaim(c => c.Type == ClaimsIdentity.DefaultRoleClaimType && c.Value == "admin"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }

            var dm = new DeviceManager(_configuration);

            var result = dm.Search();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

    }
}
