using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aoww.Services.Enums;

namespace aoww.Services.Models
{
    public class AoTransaction
    {
        public required string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int? BlockHeight { get; set; }
        public required string From { get; set; }
        public string? To { get; set; }

        public string? Cursor { get; set; }

        //public string? Data { get; set; }

    }
}
