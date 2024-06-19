using aoww.ProcesModels.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels.Interfaces
{
    public interface IProcessMetadata
    {
        List<ActionMetadata> GetActionMetadata();
    }
}
