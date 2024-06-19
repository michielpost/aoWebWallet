using aoww.ProcesModels.Interfaces;
using aoww.ProcesModels.Metadata;

namespace aoww.ProcesModels
{
    public abstract class Process : IProcessMetadata
    {
        public string ProcessId { get; set; }

        public Process(string processId) => ProcessId = processId;

        public abstract List<ActionMetadata> GetActionMetadata();

        public IEnumerable<ActionMetadata> GetDryRunActions()
        {
            var all = GetActionMetadata();

            return all.Where(x => x.ActionType == ActionType.DryRun);
        }

        public IEnumerable<ActionMetadata> GetMessageActions()
        {
            var all = GetActionMetadata();

            return all.Where(x => x.ActionType == ActionType.Message);
        }

        public IEnumerable<ActionMetadata> GetAutoRunActions()
        {
            var all = GetDryRunActions();

            return all.Where(x => x.AutoRun);
        }

    }
}
