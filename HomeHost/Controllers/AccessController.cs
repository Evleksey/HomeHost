using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        [NonAction]
        private ActionResult JsonResult(dynamic model)
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(model), "application/json");
        }


        [AcceptVerbs("GET", "OPTIONS")]
        [Route("~/login")]
        public async Task<ActionResult> Login()
        {


            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                devs = 0
            });
        }
    }
}
