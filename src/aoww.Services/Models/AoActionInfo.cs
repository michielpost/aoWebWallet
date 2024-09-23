using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.Services.Models
{
    public class AoActionInfo
    {
        public required string Name { get; set; }

        public List<AoTagInfo> Tags { get; set; } = new();
    }

    public class AoTagInfo
    {
        public required string Name { get; set; }

    }
}
