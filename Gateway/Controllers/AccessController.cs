using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Net.Client;
using Gateway.Managers;
using Gateway.Models;
using Gateway.Auth;
using Gateway.Utils;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;


namespace Gateway.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class AccessController : HostBaseController//ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AccessController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        [AcceptVerbs("POST", "OPTIONS")]
        [Route("~/login")]
        public async Task<ActionResult> Login([FromBody] LoginInfo info)
        {
            var lm = new LoginManager(_configuration);

            var result = lm.Login(info.Username, info.Password, out ClaimsIdentity identity);
            if (result)
            {
                var now = DateTime.UtcNow;
                var jwt = new JwtSecurityToken(
                         issuer: AuthOptions.ISSUER,
                         audience: AuthOptions.AUDIENCE,
                         notBefore: now,
                         claims: identity.Claims,
                         expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                         signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                return JsonResult(new
                {
                    error_code = 200,
                    error_text = "OK",
                    data = result,
                    auth = encodedJwt
                });
            }
            return JsonResult(new
            {
                error_code = 401,
                error_text = "NotOK",
                data = result
            });
        }

        [AcceptVerbs("POST", "OPTIONS")]
        [Route("~/changepassword")]
        public async Task<ActionResult> ChangePassword([FromBody] LoginInfo info)
        {
            var lm = new LoginManager(_configuration);

            var result = lm.Login(info.Username, info.Password, out ClaimsIdentity identity);
            if (result)
            {
                result = lm.ChangePassword(info.Username, info.Password, info.NewPassword);
                return JsonResult(new
                {
                    error_code = 200,
                    error_text = "OK",
                    data = result,
                });
            }
            return JsonResult(new
            {
                error_code = 401,
                error_text = "No no no no. Bad hacker!",
                data = result
            });
        }
    }
}
