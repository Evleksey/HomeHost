﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class User
    {
        public string Name { get; set; }
        public bool isAdmin { get; set; }
        public int RoomId { get; set; }
    }
}
