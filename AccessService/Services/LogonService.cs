using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessService.Models;

namespace AccessService
{
    public class LogonService : Logon.LogonBase
    {
        private readonly ILogger<LogonService> _logger;
        public LogonService(ILogger<LogonService> logger)
        {
            _logger = logger;
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var user = dc.UserLogins.Where(n => n.Login == request.Name && n.Password == request.Password).FirstOrDefault();
                    return Task.FromResult(new LoginReply
                    {
                        Success = user != null,
                        Uid = user != null ? user.Id.ToString() : "",
                        Role = "admin"
                    });
                }catch(Exception e)
                {
                    return Task.FromResult(new LoginReply
                    {
                        Success = false,
                    });
                }
            }
        } 
    }
}
