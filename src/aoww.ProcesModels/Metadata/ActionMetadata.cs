﻿using aoww.ProcesModels.Action;
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

        public Action<bool>? IsEnabled { get; set; }

        public string? LastResult { get; set; }

        //Run on initialization
        public bool AutoRun { get; set; }
    }


    public enum ActionType
    {
        Message,
        DryRun
    }
}
