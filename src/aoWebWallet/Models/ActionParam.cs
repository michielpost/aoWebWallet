﻿namespace aoWebWallet.Models
{
    public class AoAction
    {
        public List<ActionParam> Params { get; set; } = new();

        public ActionParam? Target => Params.Where(x => x.ParamType == ActionParamType.Target).FirstOrDefault();
        public IEnumerable<ActionParam> Filled => Params.Where(x => x.ParamType == ActionParamType.Filled);
        public IEnumerable<ActionParam> AllInputs => Params.Where(x => 
                                                        x.ParamType != ActionParamType.Filled
                                                        && x.ParamType != ActionParamType.Target);

        public string? IsValid()
        {
            if (Target == null)
                return "No Target process specified.";

            return null;
        }
    }

    public class ActionParam
    {
        public required string Key { get; set; }
        public string? Value { get; set; }

        /// <summary>
        /// Arguments (like TokenId)
        /// </summary>
        public List<string> Args { get; set; } = new();

        public ActionParamType ParamType { get; set; }

    }

    public enum ActionParamType
    {
        None = 0,
        Target,
        Filled,
        Input,
        Integer,
        Process,
        Balance, //Arg1: TokenId //Must have balance
        Quantity, //Arg1: TokenId //Does not care about balance
    }
}