using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.DB
{
    public partial class Room
    {
        public Room()
        {
            Devices = new HashSet<Device>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Device> Devices { get; set; }
    }
}
