using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SetRemoteService 
{
    public class SetterService : Setter.SetterBase
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

        private readonly ILogger<SetterService> _logger;
        public SetterService(ILogger<SetterService> logger)
        {
            _logger = logger;
        }

        public override Task<SetReply> SetState(SetRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SetReply
            {
                Message = "OK"
            });
        }

        public override Task<SetReply> GetStatus(GetSetterStatusRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SetReply
            {
                Message = "OK"
            });
        }
    }
}
