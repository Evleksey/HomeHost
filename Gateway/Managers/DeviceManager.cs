using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Gateway.DB;
using Gateway.Models;
using Sentry;
using Grpc.Core;

namespace Gateway.Managers
{
    public  class DeviceManager
    {
        private readonly IConfiguration _configuration;
        private readonly IGoogleOAuth2 _auth;

        public DeviceManager(IConfiguration configuration, IGoogleOAuth2 auth)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _auth = auth ?? throw new ArgumentNullException(nameof(_auth));
        }

        public bool SetDevice(APIDevice model)
        {
            using (var db = new HomeAutomationDatabaseContext())
            {
                var lm = new LoggingManager(_configuration);

                try
                {
                    if (model.Id == -1)
                    {
                        db.Add(new Device()
                        {
                            Ip = model.Ip,
                            Name = model.Name,
                            RoomId = model.RoomId
                        });
                        db.SaveChanges();

                        _ = lm.LogEvent(2, $"New device added {model.Name}", Guid.Empty);
                    }
                    else
                    {
                        var device = db.Devices.Where(c => c.Id == model.Id).FirstOrDefault();
                        if (device != null)
                        {
                            device.Name = model.Name;
                            device.RoomId = model.RoomId;
                            db.SaveChanges();
                        }

                        _ = lm.LogEvent(3, $"Device settings changed for {model.Name}", Guid.Empty);
                    }

                    return true;
                }
                catch(Exception e)
                {
                    SentrySdk.CaptureMessage(e.Message);
                    _ = lm.LogEvent(0, $"Error adding to db {e.Message} :\n {e.StackTrace}", Guid.Empty);
                    return false;
                }
            }
        }

        public async Task<bool> SetState(int id, bool state, string allowed)
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var device = db.Devices.Where(c => c.Id == id && ((c.RoomId.ToString() == allowed) || allowed == "admin")).FirstOrDefault();

                if (device != null)
                {
                    var metaData = new Metadata();
                    metaData.Add(new Metadata.Entry("Authorization", $"Bearer {_auth.AccessToken}"));
                    var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCSet")); 
                    var client = new Setter.SetterClient(channel);
                    try
                    {
                        var reply = await client.SetStateAsync(new SetRequest { Ip = device.Ip, State = state }, metaData);

                        return reply.Ok;
                    }
                    catch (Exception e)
                    {
                        SentrySdk.CaptureMessage(e.Message);
                        _ = lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);
                        return false;
                    }
                }
                return false;
            }
        }

        public async Task<GetStateReply> GetState(int id)
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var device = db.Devices.Where(c => c.Id == id).FirstOrDefault();

                if (device != null)
                {
                    var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCGet"));
                    var client = new Getter.GetterClient(channel);
                    try
                    {
                        var reply = await client.GetStateAsync(new GetRequest { Ip = device.Ip});

                        return reply;
                    }
                    catch (Exception e)
                    {
                        SentrySdk.CaptureMessage(e.Message);
                        _ = lm.LogEvent(5, $"Failed to get state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);
                        return null;
                    }
                }
                return null;
            }
        }

        public async Task<bool> Search()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                
                GetInfoReply reply;

                for (int i = 80; i < 90; i++)
                {
                    //var client = new Getter.GetterClient(channel);
                    try
                    {
                        var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCGet"));
                        var client = new Getter.GetterClient(channel);
                        reply = await client.GetInfoAsync(new GetRequest { Ip = $"192.168.1.{i}"});
                        if (reply != null && reply.Id != -1)
                        try
                        {
                            var device = db.Devices.Where(c => c.DeviceId == reply.Id).FirstOrDefault();

                            if (device == null)
                            {
                                db.Add(new Device()
                                {
                                    DeviceId = reply.Id,
                                    Ip = $"192.168.1.{i}",
                                    Name = "New device",
                                    RoomId = 1,
                                    Type = reply.Type
                                });
                                db.SaveChanges();

                                _ = lm.LogEvent(2, $"New device added at 192.168.1.{i}", Guid.Empty);
                            }
                            else
                            {
                                device.Ip = $"192.168.1.{i}";
                                db.SaveChanges();

                                _ = lm.LogEvent(3, $"Device settings changed for {device.Name}", Guid.Empty);
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            SentrySdk.CaptureMessage(e.Message);
                            _ = lm.LogEvent(0, $"Error adding to db {e.Message} :\n {e.StackTrace}", Guid.Empty);
                            return false;
                        }
                        await channel.ShutdownAsync();
                        channel.Dispose();

                    }
                    catch (Exception e)
                    {
                        SentrySdk.CaptureMessage(e.Message);
                        //_ = lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
                    }
                }
                return true;
            }
        }

        public async Task<bool> GetTest()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCGet"));
                var client = new Getter.GetterClient(channel);
                try
                {
                    var reply = await client.GetStatusAsync(new GetStatusRequest {});
                    return reply != null;
                }
                catch(Exception e)
                {
                    SentrySdk.CaptureMessage(e.Message);
                    //_ = lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
                }                
                return false;
            }
        }

        public async Task<bool> SetTest()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCSet"));
                var client = new Setter.SetterClient(channel);
                try
                {
                    var reply = await client.GetStatusAsync(new GetSetterStatusRequest { });
                    return reply != null;
                }
                catch(Exception e)
                {
                    SentrySdk.CaptureMessage(e.Message);
                }
                return false;
            }
        }

        public async Task<bool>DbAccessTest()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
                var client = new Logon.LogonClient(channel);
                try
                {
                    var reply = await client.CheckDbStatusAsync(new CheckRequest { });
                    return reply != null;
                }
                catch(Exception e)
                {
                    SentrySdk.CaptureMessage(e.Message);
                    //_ = lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
                }
                return false;
            }
        }

        public async Task<bool> AccessServiceTest()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCLogin"));
                var client = new Logon.LogonClient(channel);
                try
                {
                    var reply = await client.CheckStatusAsync(new CheckRequest { });
                    return reply != null;
                }
                catch(Exception e)
                {
                    SentrySdk.CaptureMessage(e.Message);
                    //_ = lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
                }
                return false;
            }
        }

    }
}
