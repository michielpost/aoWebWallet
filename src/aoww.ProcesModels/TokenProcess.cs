using aoww.ProcesModels.Action;
using aoww.ProcesModels.Metadata;
using ArweaveAO.Extensions;
using ArweaveAO.Models.Token;
using ArweaveAO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels
{
    public class TokenProcess : Process
    {
        public string? Name { get; set; }
        public string? Ticker { get; set; }
        public string? Logo { get; set; }
        public int? Denomination { get; set; }

        //Temp properties
        public long? balance;

        public TokenProcess(string processId) : base(processId)
        {
        }

        public override List<ActionMetadata> GetActionMetadata()
        {
            return new List<ActionMetadata>()
            {
                 new ActionMetadata
                {
                    Name = "Info",
                    AutoRun = Name == null,
                    IsHidden = true,
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionGetInfoBasic(),
                    ProcessResult = ProcessInfoBasicResult
                },
                 new ActionMetadata
                {
                    Name = "Balance",
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionBalance(),
                    ProcessResult = ProcessBalanceResult
                },
                 new ActionMetadata
                {
                    Name = "Balances",
                    IsHidden = true,
                    ActionType = ActionType.DryRun,
                    AoAction = CreateAoActionBalances(),
                },
                new ActionMetadata
                {
                    Name = "Send",
                    IsHidden = !balance.HasValue || balance <= 0,
                    ActionType = ActionType.Message,
                    AoAction = TokenProcess.CreateForTokenTransaction(this.ProcessId)
                },
                new ActionMetadata
                {
                    Name = "Mint",
                    IsHidden= true,
                    IsOwnerOnly = true,
                    ActionType = ActionType.Message,
                    AoAction = CreateAoActionMint()
                }
            };
        }



        public static AoAction CreateForTokenTransaction(string recipient, string tokenId)
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= tokenId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Transfer" },
                    new ActionParam { Key= "Recipient", ParamType = ActionParamType.Filled, Value = recipient },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Balance, Args = new List<string> { tokenId } }
                }

            };
        }

        public static AoAction CreateForTokenTransaction(string tokenId)
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= tokenId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Transfer" },
                    new ActionParam { Key= "Recipient", ParamType = ActionParamType.Process },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Balance, Args = new List<string> { tokenId } }
                }

            };
        }

        private AoAction CreateAoActionGetInfoBasic()
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

        private AoAction CreateAoActionBalance()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Balance" },
                }
            };
        }

        private AoAction CreateAoActionBalances()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Balances" },
                }
            };
        }

        private AoAction CreateAoActionMint()
        {
            return new AoAction
            {
                Params = new List<ActionParam>
                {
                    new ActionParam { Key= "Target", ParamType = ActionParamType.Target, Value= this.ProcessId },
                    new ActionParam { Key= "Action", ParamType = ActionParamType.Filled, Value= "Mint" },
                    new ActionParam { Key= "Quantity", ParamType = ActionParamType.Quantity, Args = new List<string> { this.ProcessId } }
                }
            };
        }

        private void ProcessInfoBasicResult(MessageResult? result)
        {
            if (result == null)
                return;

            //Get name and logo
            this.Name = result.GetFirstTagValue("Name");
            this.Logo = result.GetFirstTagValue("Logo");
        }

        private void ProcessBalanceResult(MessageResult? result)
        {
            if (result == null)
                return;

            string? balance = result.GetFirstTagValue("Balance");
            if (!string.IsNullOrWhiteSpace(balance) && long.TryParse(balance, out long balanceLong))
                this.balance = balanceLong;
        }
    }
}
