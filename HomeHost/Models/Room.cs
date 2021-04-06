using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHost.Models
{
    public class Room
    {
        public Guid RoomId { get; set; }
        public string Name { get; set; }
        public List<Device> Devices { get; set; }

    }
}
