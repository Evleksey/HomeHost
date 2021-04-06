using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LoggingService.Models;

namespace LoggingService
{
    public class LoggingService : Logger.LoggerBase
    {
        private readonly ILogger<LoggingService> _logger;
        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public override Task<LogReply> LogEvent(LogRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                dc.Logs.Add(new Log()
                {
                    Id = Guid.NewGuid(),
                    EventType = request.Type.ToString(),
                    Message = request.Message,
                    ObjId = String.IsNullOrEmpty(request.ObjId) ? null : Guid.Parse(request.ObjId),
                    Datetime = DateTime.Now
                });
                dc.SaveChanges();
            }
            return Task.FromResult(new LogReply
            {
                Message = "OK"
            });
        }

        public override async Task GetLog(FullLogRequest request, IServerStreamWriter<FullLogReply> reply, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                var logs = dc.Logs.ToList();
                foreach (var item in logs)
                {
                    await reply.WriteAsync(new FullLogReply { Line = $"{item.EventType},{item.Message},{item.ObjId},{item.Datetime}" });
                }
            }
            
        }
    }
}
