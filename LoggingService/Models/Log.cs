using System;
using System.Collections.Generic;

#nullable disable

namespace LoggingService.Models
{
    public partial class Log
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public Guid? ObjId { get; set; }
        public DateTime Datetime { get; set; }
    }
}
