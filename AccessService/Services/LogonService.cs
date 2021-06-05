using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace AccessService
{
    public class LogonService : Logon.LogonBase
    {
        private readonly ILogger<LogonService> _logger;
        public LogonService(ILogger<LogonService> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var user = dc.UserLogins.Where(n => n.Login == request.Name && n.Password == request.Password).FirstOrDefault();
                    if(user != null)
                        return Task.FromResult(new LoginReply
                        {
                            Success = user != null,
                            Uid = user != null ? user.Id.ToString() : "",
                            Role = user.Roomid == 0 ? "admin"  : user.Roomid.ToString()
                        }); 
                    return Task.FromResult(new LoginReply
                    {
                        Success = false
                    });
                }
                catch(Exception e)
                {
                    return Task.FromResult(new LoginReply
                    {
                        Success = false,
                    });
                }
            }
        }


        [Authorize]
        public override Task<ChangePasswordReply> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var user = dc.UserLogins.Where(n => n.Login == request.Name && n.Password == request.OldPassword).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = request.NewPassword;
                        dc.SaveChanges();
                    }
                    return Task.FromResult(new ChangePasswordReply
                    {
                        Success = user != null
                    });
                }
                catch (Exception e)
                {
                    return Task.FromResult(new ChangePasswordReply
                    {
                        Success = false
                    });
                }
            }
        }


        [Authorize]
        public override Task<ChangeReply> ChangeRole(RoleChangeRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var user = dc.UserLogins.Where(n => n.Id.ToString() == request.Uid).FirstOrDefault();
                    if (user != null)
                    {
                        user.Roomid = request.Role == "admin" ? 0 : int.Parse(request.Role);
                        dc.SaveChanges();
                    }
                    return Task.FromResult(new ChangeReply
                    {
                        Success = user != null
                    });
                }
                catch (Exception e)
                {
                    return Task.FromResult(new ChangeReply
                    {
                        Success = false
                    });
                }
            }
        }


        [Authorize]
        public override Task<UsersReply> GetUsers(UsersRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {                    
                    var users = dc.UserLogins.ToList();
                    if (users != null && users.Count > 0)
                    {

                        var reply = new UsersReply();
                        foreach (var user in users)
                        {
                            reply.Users.Add(new User
                            {
                                Id = user.Id,
                                Name = user.Login,
                                Role = user.Roomid == 0 ? "Admin" : user.Roomid.ToString()
                            });

                        }

                        return Task.FromResult(reply);
                    }
                }
                catch (Exception e)
                {
                    
                }
                return Task.FromResult(new UsersReply
                {
                });
            }
        }


        public override Task<CheckReply> CheckDbStatus(CheckRequest request, ServerCallContext context)
        {
            using (var dc = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var users = dc.UserLogins.ToList(); ;
                    if (users != null)
                    {

                        return Task.FromResult(new CheckReply
                        {
                            Success = users != null
                        });
                    }
                    return Task.FromResult(new CheckReply
                    {
                        Success = false
                    });
                }
                catch (Exception e)
                {
                    return Task.FromResult(new CheckReply
                    {
                        Success = false
                    });
                }
            }
        }

        public override Task<CheckReply> CheckStatus(CheckRequest request, ServerCallContext context)
        {            
            return Task.FromResult(new CheckReply
            {
                Success = true
            });              
        }
    }
}
