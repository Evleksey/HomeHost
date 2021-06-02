using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Gateway.Models;
using Gateway.Managers;
using Gateway.Utils;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace Gateway.Controllers
{
    [Route("room")]
    [ApiController]
    public class RoomsController : HostBaseController
    {
        [Authorize]
        [AcceptVerbs("GET", "OPTIONS")]
        [Route("getall")]
        public async Task<ActionResult> GetAll()
        {
            if (!User.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }

            string allowed = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value).First();

            var rm = new RoomsManager();
            var result = rm.Get(allowed);

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                rooms = result
            });
        }

        [Authorize]
        [AcceptVerbs("POST", "OPTIONS")]
        [Route("set")]
        public async Task<ActionResult> SetRoom([FromBody] APIRoom model)
        {
            if (!User.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "admin"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }

            var dm = new RoomsManager();
            var result = dm.SetRoom(model);

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }


    }
}
