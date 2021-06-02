using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Refit;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GetRemoteService 
{
    public class InfoResult
    {
        public string type { get; set; }
        public int uid { get; set; }
    }
    public class GetterService : Getter.GetterBase
    {
        public interface IDevice
        {
            [Get("/status")]
            Task<dynamic> GetStatus();

            [Get("/whoami")]
            Task<InfoResult> GetInfo();
        }

        public static string GET(string Url)
        {
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create("http://" + Url);
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

        private readonly ILogger<GetterService> _logger;
        public GetterService(ILogger<GetterService> logger)
        {
            _logger = logger;
        }

        public override  Task<GetReply> GetState(GetRequest request, ServerCallContext context)
        {
            var result = GET(request.Ip + "/status");
            return  Task.FromResult(new GetReply
            {
                Message = result
            });
        }

        public override Task<GetInfoReply> GetInfo(GetRequest request, ServerCallContext context)
        {
            var refit = RestService.For<IDevice>("http://" + request.Ip);
            try
            {
                InfoResult result = refit.GetInfo().Result; 
                return Task.FromResult(new GetInfoReply
                {
                    Id = result.uid,
                    Type = result.type
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new GetInfoReply
                {
                    Id = -1,
                    Type = ""
                });
            }
        }

        public override Task<GetReply> GetStatus(GetStatusRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetReply
            {
                Message = "OK"
            });
        }
    }
}
