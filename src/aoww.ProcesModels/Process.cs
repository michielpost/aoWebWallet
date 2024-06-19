using aoww.ProcesModels.Interfaces;
using aoww.ProcesModels.Metadata;

namespace aoww.ProcesModels
{
    public abstract class Process : IProcessMetadata
    {
        public string ProcessId { get; set; }

        public Process(string processId) => ProcessId = processId;

        public abstract List<ActionMetadata> GetActionMetadata();
    }
}
