using aoww.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.Services.Extensions
{
    public static class EdgeResultExtensions
    {
        public static string? GetFirstTagValue(this Edge edge, string tag)
        {
            if (edge == null || edge.Node == null)
                return null;

            return edge.Node.Tags.Where(x => x.Name == tag).Select(x => x.Value).FirstOrDefault();
        }
       
    }
}
