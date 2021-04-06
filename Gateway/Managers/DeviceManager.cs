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
                        lm.LogEvent(5, $"Failed to set state to {device.Name} : {e.Message} : \n {e.StackTrace}", id.ToString());
                        return false;
                    }
                }
                return false;
            }
        }

        public async Task<string> GetState(int id)
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

                        return reply.Message;
                    }
                    catch (Exception e)
                    {
                        lm.LogEvent(5, $"Failed to get state to {device.Name} : {e.Message} : \n {e.StackTrace}", id.ToString());
                        return null;
                    }
                }
                return null;
            }
        }

        //public async Task<dynamic> Call(string ip)
        //{
        //    var result = await GET(ip);
        //    return result;
        //}

        //public async Task<dynamic> Search()
        //{
        //    using (var db = new HomeAutomationDatabaseContext())
        //    {
        //        var devices = db.Devices.ToList(); ;
        //       for (int i = 1; i < 255; i++)
        //       {
        //            var res = await GET($"192.168.1.{i}/id");
        //            //если нашли новый id обновляем
        //            //если нашли известный - обновляем
        //       }
                
        //    }
        //    return null;
        //}
    }
}
