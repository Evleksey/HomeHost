using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gateway.DB;
using Gateway.Models;
using StackExchange.Redis;

namespace Gateway.Managers
{
    public class RoomsManager
    {
        public List<APIRoom> Get(string allowed)
        {
            var apirooms = new List<APIRoom>();

            using (var db = new HomeAutomationDatabaseContext())
            {
                try
                {
                    var rooms = db.Rooms.Where(c => (c.Id.ToString() == allowed) || allowed == "admin").ToList();

                    IDatabase cache = null;
                    bool redis_online = true;
                    try
                    {
                        cache = RedisConnection.Connection.GetDatabase();
                    }
                    catch(Exception e)
                    {
                        redis_online = false;
                    }

                    foreach (var room in rooms)
                    {
                        var devices = new List<APIDevice>();
                        var dbDevices = db.Devices.Where(c => (c.RoomId == room.Id)).ToList();
                        foreach (var device in dbDevices)
                        {
                            float hum = -1f, temp = -1;
                            bool pow = false;
                            if (redis_online)
                            {
                                temp = float.Parse(cache.StringGet($"Device_Temp:{device.Id}:"));
                                hum = float.Parse(cache.StringGet($"Device_Hum:{device.Id}:"));
                                pow = bool.Parse(cache.StringGet($"Device_Power:{device.Id}:"));
                            }

                            devices.Add(new APIDevice()
                            {
                                Id = device.Id,
                                Ip = device.Ip,
                                Name = device.Name,
                                RoomId = device.RoomId,
                                temprature = temp,
                                humidity = hum,
                                power = pow
                            });                            
                        }

                        apirooms.Add(new APIRoom()
                        {
                            Id = room.Id,
                            Name = room.Name,
                            Devices = devices
                        });
                    }
                    return apirooms;
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        public bool SetRoom(APIRoom model)
        {
            using (var db = new HomeAutomationDatabaseContext())
            {
                try
                {
                    if (model.Id == -1)
                    {
                        db.Rooms.Add(new Room()
                        {
                            Name = model.Name
                        });

                        db.SaveChanges();
                    }
                    else
                    {
                        var room = db.Rooms.FirstOrDefault(c => c.Id == model.Id);
                        room.Name = model.Name;
                        db.SaveChanges();

                    }

                    return true;
                }
                catch(Exception e)
                {
                    return false;
                }
            }
        }

    }
}
