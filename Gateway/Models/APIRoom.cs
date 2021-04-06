using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class APIRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IEnumerable<APIDevice> Devices { get; set; }
    }
}
