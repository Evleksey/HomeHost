using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class APIDevice
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int RoomId { get; set; }
        public dynamic data { get; set; }

    }
}
