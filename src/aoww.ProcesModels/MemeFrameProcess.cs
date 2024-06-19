using aoww.ProcesModels.Action;
using aoww.ProcesModels.Metadata;
using ArweaveAO.Extensions;
using ArweaveAO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels
{
    public class MemeFrameProcess : Process
    {
        public string? MintTokenId { get; set; }

        public string? Name { get; set; }
        public string? Logo { get; set; }

        public MemeFrameProcess(string processId) : base(processId) { }

        public override List<ActionMetadata> GetActionMetadata()
        {
            var actions = new List<ActionMetadata>()
            {
                new ActionMetadata
                {
                    Name = "Info",
                    AutoRun = true,
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionGetInfo(),
                    ProcessResult = ProcessInfoResult
                },
                new ActionMetadata
                {
                    Name = "View Stakers",
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionViewStakers()
                },
                //new ActionMetadata
                //{
                //    Name = "Get Frame",
                //    ActionType = ActionType.DryRun,
                //    AoAction = TokenProcess.CreateForTokenTransaction(this.ProcessId)
                //},
                
                new ActionMetadata
                {
                    Name = "Stake",
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionStake()
                },
                new ActionMetadata
                {
                    Name = "Vote yay",
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionVote("yay")
                },
                new ActionMetadata
                {
                    Name = "Vote nay",
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionVote("nay")
                }
                ,
                //new ActionMetadata
                //{
                //    Name = "Get Votes",
                //    ActionType = ActionType.DryRun,
                //    AoAction = TokenProcess.CreateForTokenTransaction(this.ProcessId)
                //}
            };

            if (MintTokenId != null)
            {
                actions.Add(
                    new ActionMetadata
                    {
                        Name = "Mint",
                        ActionType = ActionType.Message,
                        AoAction = TokenProcess.CreateForTokenTransaction(this.ProcessId, this.MintTokenId)
                    }
                    );
            }

            return actions;
        }

        private void ProcessInfoResult(MessageResult? result)
        {
            if (result == null)
                return;

            //Get name and logo
            this.Name = result.GetFirstTagValue("Name");
            this.Logo = result.GetFirstTagValue("Logo");
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

        private AoAction CreateAoActionVote(string vote)
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Vote" },
                    new ActionParam { Key= "Side", ParamType = ActionParamType.Filled, Value = vote },
                    new ActionParam { Key= "VoteId", ParamType = ActionParamType.Process },
                }
            };
        }

        private AoAction CreateAoActionGetInfo()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Info" },
                }
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
    }
}
