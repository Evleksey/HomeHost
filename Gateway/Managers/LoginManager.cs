using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gateway.Managers;

namespace Gateway.Managers
{
    public class LoginManager
    {
        private readonly IConfiguration _configuration;

        public LoginManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public bool Login(string username, string password, out ClaimsIdentity identity)
        {
            var lm = new LoggingManager(_configuration);
            var x = _configuration.GetConnectionString("gRPCLogin");
            using var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
            var client = new Logon.LogonClient(channel);
            try
            {
                var reply = client.Login(new LoginRequest { Name = username, Password = password });

                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, reply.Uid),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, reply.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);

                identity = claimsIdentity;

                lm.LogEvent(1, $"Login attempt for {username}, result: {reply.Success}", Guid.Empty);

                return reply.Success;
            }
            catch (Exception e)
            {
                identity = null;
                lm.LogEvent(1, $"Login attempt for {username} failed, check gRPC", Guid.Empty);
                return false;
            }
        }
    }
}
