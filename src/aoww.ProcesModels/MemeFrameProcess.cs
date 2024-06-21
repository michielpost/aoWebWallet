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
    /// <summary>
    /// https://github.com/permaweb/memeframes
    /// </summary>
    public class MemeFrameProcess : Process
    {
        public string? MintTokenId { get; set; }

        public string? Description { get; set; }
        public string? FrameId { get; set; }

        public TokenProcess TokenProcess { get; set; }
        public StakingProcess StakingProcess { get; set; }

        public MemeFrameProcess(string processId) : base(processId) {
        
            TokenProcess = new TokenProcess(processId);
            StakingProcess = new StakingProcess(processId);
        }

        public override List<ActionMetadata> GetActionMetadata()
        {
            var actions = new List<ActionMetadata>();

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

            actions.AddRange(TokenProcess.GetActionMetadata());
            actions.AddRange(StakingProcess.GetActionMetadata());

            actions.AddRange(new List<ActionMetadata>()
            {
                new ActionMetadata
                {
                    Name = "Get Description",
                    AutoRun = Description == null,
                    IsHidden = true,
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionGetInfoDescription(),
                    ProcessResult = ProcessInfoDescriptionResult
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
                },
                //new ActionMetadata
                //{
                //    Name = "Get Votes",
                //    ActionType = ActionType.DryRun,
                //    AoAction = CreateAoActionGetVotes()
                //},
                new ActionMetadata
                {
                    Name = "Get Frame",
                    AutoRun = true,
                    IsHidden = true,
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionGetFrame(),
                    ProcessResult = (x) =>
                    {
                        this.FrameId = x?.Messages?.FirstOrDefault()?.Data;
                    }
                }
            });

            

            return actions;
        }

        

        private void ProcessInfoDescriptionResult(MessageResult? result)
        {
            if (result == null)
                return;

            //Get name and logo
            this.Description = result.Messages.FirstOrDefault()?.Data;
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

       

        private AoAction CreateAoActionGetInfoDescription()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Get-Info" },
                }
            };
        }
               

        private AoAction CreateAoActionGetVotes()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Votes" },
                }
            };
        }

        private AoAction CreateAoActionGetFrame()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Get-Frame" },
                }
            };
        }
    }
}
