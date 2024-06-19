using aoww.ProcesModels.Action;
using aoww.ProcesModels.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoww.ProcesModels
{
    public class TokenProcess : Process
    {
        public TokenProcess(string processId) : base(processId)
        {
        }

        public override List<ActionMetadata> GetActionMetadata()
        {
            return new List<ActionMetadata>()
            {
                new ActionMetadata
                {
                    Name = "Send",
                    ActionType = ActionType.Message,
                    AoAction = TokenProcess.CreateForTokenTransaction(this.ProcessId)
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
    }
}
