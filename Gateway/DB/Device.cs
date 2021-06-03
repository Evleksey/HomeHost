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
        public string Type { get; set; }
        public int? DeviceId { get; set; }
        public double? Temprature { get; set; }
        public double? Humidity { get; set; }
        public bool? Power { get; set; }

        public virtual Room Room { get; set; }
    }
}
