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

        public TestController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        

        //[Authorize]
        [AcceptVerbs("GET", "OPTIONS")]
        [Route("getServise")]
        public async Task<ActionResult> Search()
        {            

            var dm = new DeviceManager(_configuration);

            var result = await dm.Test();

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = result
            });
        }

    }
}