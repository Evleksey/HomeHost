using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.DB
{
    public partial class Device
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public int RoomId { get; set; }

        public virtual Room Room { get; set; }
    }
}
