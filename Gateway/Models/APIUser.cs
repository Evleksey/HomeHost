using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class APIUser
    {
        public int id { get; set; }
        public string Name { get; set; }
        public bool isAdmin { get; set; }
        public int RoomId { get; set; }
    }
}
