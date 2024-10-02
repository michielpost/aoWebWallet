using System.Text;

namespace aoww.ProcesModels.Action
{
    public class AoAction
    {
        public List<ActionParam> Params { get; set; } = new();
        public bool HasData { get; set; } = false;
        public string? DataValue { get; set; }

        public ActionParam? Target => Params.Where(x => x.ParamType == ActionParamType.Target).FirstOrDefault();
        public IEnumerable<ActionParam> AllWithoutTarget => Params.Where(x => x.ParamType != ActionParamType.Target);
        public IEnumerable<ActionParam> Filled => Params.Where(x => x.ParamType == ActionParamType.Filled);
        public IEnumerable<ActionParam> AllInputs => Params.Where(x =>
                                                        x.ParamType != ActionParamType.Filled
                                                        && x.ParamType != ActionParamType.Target);

        public string? IsValid()
        {
            if (Target == null)
                return "No Target process specified.";

            foreach (var input in AllInputs)
            {
                if (string.IsNullOrEmpty(input.Value))
                    return $"Please enter a value for {input.Key}";
            }

            return null;
        }

        public string ToQueryString()
        {
            if (Target == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append($"{Target.Key}={Target.Value}&");

            foreach (var param in Filled)
            {
                sb.Append($"{param.Key}={param.Value}&");
            }

            foreach (var param in AllInputs)
            {
                var args = string.Join(';', param.Args);
                if (args.Length > 0)
                {
                    sb.Append($"X-{param.ParamType}={param.Key};{args}&");
                }
                else
                {
                    sb.Append($"X-{param.ParamType}={param.Key}&");
                }
            }

            if(HasData)
                sb.Append($"X-HasData=1&");

            return sb.ToString().TrimEnd('&');
        }

        public static AoAction CreateFromQueryString(string qstring)
        {
            // Parsing query string
            var queryStringValues = System.Web.HttpUtility.ParseQueryString(qstring);

            AoAction action = new AoAction();

            foreach (var key in queryStringValues.AllKeys)
            {
                if (key == null)
                    continue;

                var values = queryStringValues.GetValues(key);
                if (values == null || !values.Any())
                    continue;

                foreach (var val in values)
                {
                    string actionKey = key;
                    string? actionValue = val.ToString();
                    ActionParamType actionParamType = ActionParamType.Filled;

                    var actionValueSplit = actionValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    actionValue = actionValueSplit.First();
                    List<string> args = actionValueSplit.Skip(1).ToList();

                    if (key.Equals($"X-{nameof(HasData)}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (actionValue == "1" || actionValue == "true")
                            action.HasData = true;

                        continue;
                    }

                    if (key.Equals("Target", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Target;
                    if (key.Equals("X-Quantity", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Quantity;
                    if (key.Equals("X-Balance", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Balance;
                    else if (key.Equals("X-Process", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Process;
                    else if (key.Equals("X-Integer", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Integer;
                    else if (key.Equals("X-Input", StringComparison.InvariantCultureIgnoreCase))
                        actionParamType = ActionParamType.Input;

                    if (actionParamType != ActionParamType.Filled
                        && actionParamType != ActionParamType.Target)
                    {
                        actionKey = actionValue;
                        actionValue = null;
                    }

                    action.Params.Add(new ActionParam
                    {
                        Key = actionKey,
                        Value = actionValue,
                        Args = args,
                        ParamType = actionParamType
                    });

                }
            }

            return action;
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
