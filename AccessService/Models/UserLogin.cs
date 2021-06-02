using System;
using System.Collections.Generic;

#nullable disable

namespace AccessService.Models
{
    public partial class UserLogin
    {
        public int Id { get; set; }
        public Guid Uid { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int? Roomid { get; set; }
    }
}
