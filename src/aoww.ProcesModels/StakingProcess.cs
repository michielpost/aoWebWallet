using aoww.ProcesModels.Action;
using aoww.ProcesModels.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels
{
    public class StakingProcess : Process
    {
        public StakingProcess(string processId) : base(processId)
        {
        }

        public override List<ActionMetadata> GetActionMetadata()
        {
            return new List<ActionMetadata>()
            {
                new ActionMetadata
                {
                    Name = "View Stakers",
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionViewStakers()
                },
                new ActionMetadata
                {
                    Name = "Stake",
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionStake()
                },
                 new ActionMetadata
                {
                    Name = "Unstake",
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionUnStake()
                },
                // new ActionMetadata
                //{
                //    Name = "Finalize",
                //    ActionType = ActionType.Message,
                //    AoAction = CreateAoActionFinalize()
                //},
            };
        }

        private AoAction CreateAoActionViewStakers()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Stakers" },
                }
            };
        }

        private AoAction CreateAoActionStake()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Stake" },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Balance, Args = new List<string> { this.ProcessId } },
                    new ActionParam { Key= "UnstakeDelay", ParamType = ActionParamType.Integer },
                }
            };
        }

        private AoAction CreateAoActionUnStake()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Unstake" },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Balance, Args = new List<string> { this.ProcessId } },
                }
            };
        }

        private AoAction CreateAoActionFinalize()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Finalize" },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Balance, Args = new List<string> { this.ProcessId } },
                }
            };
        }

    }
}
