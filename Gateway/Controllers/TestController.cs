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
    [Route("test")]
    public class TestController : HostBaseController
    {
        private readonly IConfiguration _configuration; 
        private readonly IGoogleOAuth2 _auth;

        public TestController(IConfiguration configuration,IGoogleOAuth2 auth)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
        }
        

        //[Authorize]
        [AcceptVerbs("GET")]
        [Route("getServise")]
        public async Task<ActionResult> Test1()
        {            

            var dm = new DeviceManager(_configuration, _auth);

            var result = await dm.GetTest();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

        [AcceptVerbs("GET")]
        [Route("setServise")]
        public async Task<ActionResult> Test2()
        {

            var dm = new DeviceManager(_configuration, _auth);

            var result = await dm.SetTest();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

        [AcceptVerbs("GET")]
        [Route("dbTest")]
        public async Task<ActionResult> Test3()
        {

            var dm = new DeviceManager(_configuration, _auth);

            var result = await dm.DbAccessTest();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

        [AcceptVerbs("GET")]
        [Route("accessServise")]
        public async Task<ActionResult> Test4()
        {

            var dm = new DeviceManager(_configuration, _auth);

            var result = await dm.DbAccessTest();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

    }
}