using aoww.ProcesModels.Interfaces;
using aoww.ProcesModels.Metadata;

namespace aoww.ProcesModels
{
    public abstract class Process : IProcessMetadata
    {
        public string ProcessId { get; set; }

        public Process(string processId) => ProcessId = processId;

        public abstract List<ActionMetadata> GetActionMetadata();

        public IEnumerable<ActionMetadata> GetVisibleDryRunActions()
        {
            var all = GetActionMetadata();

            return all.Where(x => !x.IsHidden).Where(x => x.ActionType == ActionType.DryRun);
        }

        public IEnumerable<ActionMetadata> GetVisibleMessageActions()
        {
            var all = GetActionMetadata();

            return all.Where(x => !x.IsHidden).Where(x => x.ActionType == ActionType.Message);
        }

        public IEnumerable<ActionMetadata> GetAutoRunActions()
        {
            var all = GetActionMetadata();

            return all.Where(x => x.ActionType == ActionType.DryRun).Where(x => x.AutoRun);
        }

    }
}
