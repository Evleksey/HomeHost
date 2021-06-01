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
    public class GetterService : Getter.GetterBase
    {

        public interface IDevice
        {
            [Get("/status")]
            Task<dynamic> GetStatus();

            [Get("/whoami")]
            Task<dynamic> GetInfo();
        }

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

        public override Task<GetReply> GetInfo(GetRequest request, ServerCallContext context)
        {
            var result = GET(request.Ip + "/whoami");
            return Task.FromResult(new GetReply
            {
                Message = result
            });
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
