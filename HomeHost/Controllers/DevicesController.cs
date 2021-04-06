using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using HomeHost.Models;
using HomeHost.Utils;

namespace HomeHost.Controllers
{
    [ApiController]
    public class DevicesController : ControllerBase
    {
        public static string GET(string Url)
        {
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
                req.Timeout = 1000;
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.Stream stream = resp.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                string Out = sr.ReadToEnd();
                sr.Close();
                return Out;
            }
            catch { }
            return null;
        }
        [NonAction]
        private ActionResult JsonResult(dynamic model)
        {
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(model), "application/json");
        }
       

        [AcceptVerbs("GET","OPTIONS")]
        [Route("~/find")]
        public async Task<ActionResult> Find()
        {
            List<Device> devs = new List<Device>();            

            for (int i = 50; i < 90; i++)
            {
                try
                {
                    string ip = $"http://192.168.1.{i}/whoami";
                    string res;
                    if ((res = GET(ip)) != null)
                    {
                        ApiResult x = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult>(res);
                        devs.Add(new Device()
                        {
                            ip = $"192.168.1.{i}",
                            uid = x.data.uid,
                            type = x.data.type
                        });
                    }
                }
                catch { }
            }
             
            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                devs = devs
            });
        }

        [AcceptVerbs("GET")]
        [Route("~/testfind")]
        public async Task<ActionResult> TestFind()
        {
            List<Room> rooms = new List<Room>();
            for (int i = 0; i < 2; i++)
            { 
                List<Device> devs = new List<Device>();
                for (int j = 0; j < 4-i*2; j++)
                {
                    devs.Add(new Device()
                    {
                        ip = $"192.168.1.{51 + (i+j) * 3}",
                        uid = (j+i).ToString(),
                        data = 22.5+i+j
                    });
                }
                rooms.Add(new Room()
                {
                    Name = $"Room{i+1}",
                    Devices = devs,
                });
            }

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                rooms = rooms
            });
        }


        [AcceptVerbs("GET")]
        [Route("~/ask/{ip}")]
        public async Task<ActionResult> Ask(string ip)
        {
            ip = $"http://{ip}/status";
            dynamic res;
            if ((res = GET(ip)) != null)
            {
                ApiResult x = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResult>(res);
                res = x.data;
            }

            return JsonResult(new
            {
                error_code = 200,
                error_text = "OK",
                result = res
            });
        }
    }
}
