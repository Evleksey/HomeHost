using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Utils
{
    [ApiController]
    public class HostBaseController : ControllerBase
    {

        [NonAction]
        protected ActionResult JsonResult(dynamic model)
        {
            return Content(JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            }), "application/json");
        }
    }
}
