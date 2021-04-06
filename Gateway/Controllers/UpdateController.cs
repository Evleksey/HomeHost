using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.Models;
using Gateway.DB;
using Gateway.Utils;
using Gateway.Managers;
using Microsoft.Extensions.Configuration;


namespace Gateway.Controllers
{
    [Route("update")]
    [ApiController]
    public class UpdateController : HostBaseController
    {
        private readonly IConfiguration _configuration;

        public UpdateController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [AcceptVerbs("GET", "OPTIONS")]
        [Route("sensors")]
        public async Task<ActionResult> Update()
        {
            using(var db = new HomeAutomationDatabaseContext())
            {
                var devices = db.Devices.ToList();

                var dm = new DeviceManager(_configuration);

                foreach (var device in devices)
                {
                    var a = dm.GetState(device.Id).ToString();
                    var cache = RedisConnection.Connection.GetDatabase();
                    cache.StringSet($"Device_data:{device.Id}:", a);
                }
            }

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK"
            });
        }

    }
}
