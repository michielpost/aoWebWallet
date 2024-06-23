using aoww.ProcesModels.Action;
using ArweaveAO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels.Metadata
{
    public class ActionMetadata
    {
        public required string Name { get; set; }
        public int Order { get; set; }

        public ActionType ActionType { get; set; }

        public required AoAction AoAction { get; set; }

        public Action<MessageResult?>? ProcessResult { get; set; }
        public Func<bool>? IsEnabled { get; set; }

        public string? LastResult { get; set; }

        //Run on initialization
        public bool AutoRun { get; set; }
        public bool IsHidden { get; set; }
        public bool IsOwnerOnly { get; set; }
    }


    public enum ActionType
    {
        Message,
        DryRun
    }
}
