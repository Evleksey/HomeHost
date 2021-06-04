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
using Sentry;


namespace Gateway.Controllers
{
    [Route("update")]
    [ApiController]
    public class UpdateController : HostBaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHub _sentryHub;

        public UpdateController(IConfiguration configuration, IHub sentryHub)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sentryHub = sentryHub ?? throw new ArgumentNullException(nameof(configuration));
        }

        [AcceptVerbs("GET")]
        [Route("sensors")]
        public async Task<ActionResult> Update()
        {
            using(var dc = new HomeAutomationDatabaseContext())
            {
                var devices = dc.Devices.ToList();

                var dm = new DeviceManager(_configuration);

                foreach (var device in devices)
                {
                    var childSpan = _sentryHub.GetSpan()?.StartChild("update");
                    var result = dm.GetState(device.Id).Result;
                    if (result != null)
                    {
                        device.Temprature = result.Temprature;
                        device.Humidity = result.Humidity;
                        device.Power = result.Power;

                        dc.SaveChanges();
                        childSpan?.Finish(SpanStatus.Ok);
                    }
                    else { childSpan?.Finish(SpanStatus.Unavailable); }
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
