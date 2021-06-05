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
    [Route("users")]
    [ApiController]
    public class UserController : HostBaseController
    {
        private readonly IConfiguration _configuration;
        private readonly GoogleOAuth2 _auth;

        public UserController(IConfiguration configuration, GoogleOAuth2 auth)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));

        }

        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<ActionResult> Get()
        {
            if (!User.HasClaim(c => c.Type == ClaimsIdentity.DefaultRoleClaimType && c.Value == "admin"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }            

            var um = new UsersManager(_configuration, _auth);
            var result = um.GetUsers();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                rooms = result
            });
        }    

        [Authorize]
        [AcceptVerbs("POST", "OPTIONS")]
        [Route("~/user/{userId}/setroom/{roomId}")]
        public async Task<ActionResult> SetUserRole(int userId, string roomId)
        {
            if (!User.HasClaim(c => c.Type == ClaimsIdentity.DefaultRoleClaimType && c.Value == "admin"))
            {
                return JsonResult(new
                {
                    error_code = 401,
                    error_text = "Unauthorised"
                });
            }          

            var um = new UsersManager(_configuration, _auth);
            var result = um.ChangeRole(userId, roomId);

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }
    }
}
