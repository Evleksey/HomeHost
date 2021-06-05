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
        private readonly IGoogleOAuth2 _auth;

        public AccessController(IConfiguration configuration, IGoogleOAuth2 auth)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));

        }


        [AcceptVerbs("POST", "OPTIONS")]
        [Route("~/login")]
        public async Task<ActionResult> Login([FromBody] LoginInfo info)
        {
            var um = new UsersManager(_configuration, _auth);

            var result = um.Login(info.Username, info.Password, out ClaimsIdentity identity);
            if (result)
            {
                var now = DateTime.UtcNow;
                var jwt = new JwtSecurityToken(
                         issuer: AuthOptions.ISSUER,
                         audience: AuthOptions.AUDIENCE,                         
                         notBefore: now,
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
            var um = new UsersManager(_configuration, _auth);

            var result = um.Login(info.Username, info.Password, out ClaimsIdentity identity);
            if (result)
            {
                result = um.ChangePassword(info.Username, info.Password, info.NewPassword);
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
