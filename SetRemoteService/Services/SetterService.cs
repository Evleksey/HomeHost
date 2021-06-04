using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Refit;

namespace SetRemoteService
{
    public class Result
    {
        public string status { get; set; }
    }
    public class SetterService : Setter.SetterBase
    {
        public interface IDevice 
        {
            [Get("/setpower/{state}")]
            Task<Result> SetState(bool state);
        }

        private readonly ILogger<SetterService> _logger;
        public SetterService(ILogger<SetterService> logger)
        {
            _logger = logger;
        }

        public override Task<SetReply> SetState(SetRequest request, ServerCallContext context)
        {
            var refit = RestService.For<IDevice>("http://" + request.Ip);
            try
            {
                var result = refit.SetState(request.State).Result;
                return Task.FromResult(new SetReply
                {
                    Ok = result.status == "OK"
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new SetReply
                {
                    Ok = false
                });
            }
        }

        public override Task<SetReply> GetStatus(GetSetterStatusRequest request, ServerCallContext context)
        {
            return Task.FromResult(new SetReply
            {
                Ok = true
            });
        }
    }
}
