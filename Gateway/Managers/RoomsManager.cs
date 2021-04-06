﻿using System;
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

                    var cache = RedisConnection.Connection.GetDatabase();

                    foreach (var room in rooms)
                    {
                        var devices = new List<APIDevice>();
                        foreach (var device in room.Devices)
                        {
                            var val = cache.StringGet($"Device_Data:{device.Id}:");

                            devices.Add(new APIDevice()
                            {
                                Id = device.Id,
                                Ip = device.Ip,
                                Name = device.Name,
                                RoomId = device.RoomId,
                                data = val
                            });
                            apirooms.Add(new APIRoom()
                            {
                                Id = room.Id,
                                Name = room.Name,
                                Devices = devices
                            });
                        }
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