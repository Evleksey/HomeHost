using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Models
{
    public class ApiResult
    {
        public int error_code { get; set; }
        public string error_text { get; set; }
        public dynamic data { get; set; }
    }
}
