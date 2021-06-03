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

namespace Gateway.Managers
{
    public  class DeviceManager
    {
        private readonly IConfiguration _configuration;

        public DeviceManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        //private async Task<string> GET(string Url)
        //{
        //    try
        //    {
        //        System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
        //        req.Timeout = 1000;
        //        System.Net.WebResponse resp = req.GetResponse();
        //        System.IO.Stream stream = resp.GetResponseStream();
        //        System.IO.StreamReader sr = new System.IO.StreamReader(stream);
        //        string Out = await sr.ReadToEndAsync();
        //        sr.Close();
        //        return Out;
        //    }
        //    catch { }
        //    return null;
        //}

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

                        lm.LogEvent(2, $"New device added {model.Name}", Guid.Empty);
                    }
                    else
                    {
                        var device = db.Devices.Where(c => c.Id == model.Id).FirstOrDefault();
                        device.Name = model.Name;
                        device.RoomId = model.RoomId;
                        db.SaveChanges();

                        lm.LogEvent(3, $"Device settings changed for {model.Name}", Guid.Empty);
                    }

                    return true;
                }
                catch(Exception e)
                {
                    lm.LogEvent(0, $"Error adding to db {e.Message} :\n {e.StackTrace}", Guid.Empty);
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
                    var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCSet")); //"https://localhost:5001");
                    var client = new Setter.SetterClient(channel);
                    try
                    {
                        var reply = await client.SetStateAsync(new SetRequest { Ip = device.Ip, State = state });

                        return reply.Message == "OK";
                    }
                    catch (Exception e)
                    {
                        lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);
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
                        lm.LogEvent(5, $"Failed to get state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);
                        return null;
                    }
                }
                return null;
            }
        }

        public async Task<dynamic> Search()
        {
            var lm = new LoggingManager(_configuration);

            using (var db = new HomeAutomationDatabaseContext())
            {
                for (int i = 88; i < 89; i++)
                {             

                    var channel = GrpcChannel.ForAddress(_configuration.GetConnectionString("gRPCGet")); 
                    var client = new Getter.GetterClient(channel);
                    try
                    {
                        var reply = await client.GetInfoAsync(new GetRequest { Ip = $"192.168.1.{i}"});
                        if(reply != null)
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

                                lm.LogEvent(2, $"New device added at 192.168.1.{i}", Guid.Empty);
                            }
                            else
                            {
                                    device.Ip = $"192.168.1.{i}";
                                    db.SaveChanges();

                                lm.LogEvent(3, $"Device settings changed for {device.Name}", Guid.Empty);
                            }

                            return true;
                        }
                        catch (Exception e)
                        {
                            lm.LogEvent(0, $"Error adding to db {e.Message} :\n {e.StackTrace}", Guid.Empty);
                            return false;
                        }

                    }
                    catch (Exception e)
                    {
                        //lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
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
                catch (Exception e)
                {
                    //lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
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
                catch (Exception e)
                {
                    //lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
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
                catch (Exception e)
                {
                    //lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
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
                catch (Exception e)
                {
                    //lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", null);                        
                }
                return false;
            }
        }

    }
}
