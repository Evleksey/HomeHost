using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Gateway.Models;
using Sentry;

namespace Gateway.Managers
{
    public class UsersManager
    {
        private readonly IConfiguration _configuration;

        public UsersManager(IConfiguration configuration)
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
                SentrySdk.CaptureMessage(e.Message);
                lm.LogEvent(1, $"Login attempt for {username} failed {e.Message}, check gRPC", Guid.Empty);
                return false;
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var lm = new LoggingManager(_configuration);
            using var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
            var client = new Logon.LogonClient(channel);
            try
            {
                var reply = client.ChangePassword(new ChangePasswordRequest { Name = username, OldPassword = oldPassword, NewPassword = newPassword });

                lm.LogEvent(1, $"Password change for {username}, result: {reply.Success} ", Guid.Empty);

                return reply.Success;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureMessage(e.Message);
                lm.LogEvent(1, $"Password change failed attempt for {username} {e.Message}, check gRPC", Guid.Empty);
                return false;
            }
        }

        public bool ChangeRole(int userId, string role )
        {
            var lm = new LoggingManager(_configuration);

            using var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
            var client = new Logon.LogonClient(channel);
            try
            {
                var reply = client.ChangeRole(new RoleChangeRequest { Uid = userId.ToString(), Role = role});

                lm.LogEvent(1, $"Role change for {userId}, result: {reply.Success} ", Guid.Empty);

                return reply.Success;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureMessage(e.Message);
                lm.LogEvent(1, $"Role change failed  for {userId} {e.Message}, check gRPC", Guid.Empty);
                return false;
            }
        }

        public List<APIUser> GetUsers()
        {
            var lm = new LoggingManager(_configuration);

            using var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
            var client = new Logon.LogonClient(channel);
            try
            {
                var reply = client.GetUsers(new UsersRequest {});

                lm.LogEvent(1, $"{reply.Users.ToList().Count()} users retreived ", Guid.Empty);

                var users = reply.Users.ToList();
                var result = new List<APIUser>();

                foreach (var user in users)
                {
                    result.Add(new APIUser
                    {
                        id = user.Id,
                        isAdmin = user.Role == "Admin",
                        Name = user.Name,
                        RoomId = user.Role == "Admin" ? 0 : int.Parse(user.Role)
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                SentrySdk.CaptureMessage(e.Message);
                lm.LogEvent(1, $"Failed users retreive , check gRPC", Guid.Empty);
                return null;
            }
        }
    }
}
