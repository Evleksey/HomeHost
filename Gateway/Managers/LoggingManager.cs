using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client;

namespace Gateway.Managers
{

    public enum EventTypes 
    {
        SwitchStateExternal,
        SwitchStateInternal,
        TempLog,
    }
    public class LoggingManager
    {
        private readonly IConfiguration _configuration;

        public LoggingManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<bool> LogEvent(int eventType, string message, Guid? objId)
        {
            using var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogs"));
            var client = new Logger.LoggerClient(channel);
            try
            {
                var reply = await client.LogEventAsync(new LogRequest {Type = eventType, Message = message, ObjId = objId?.ToString() ?? "" });

                return reply.Message == "OK";
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
