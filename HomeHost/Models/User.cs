using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeHost.Models
{
    public class User
    {
        public string Name { get; set; }
        public bool isAdmin { get; set; }
        public List<Guid> AllowedRooms { get; set; }
    }
}
