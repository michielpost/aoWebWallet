using aoww.ProcesModels.Action;
using aoww.ProcesModels.Metadata;
using aoww.ProcesModels.SchemaProtocol.Models;

namespace aoww.ProcesModels.SchemaProtocol.Extensions
{
    public static class SchemaProtocolExtensions
    {
        public static ActionMetadata? ToActionMetadata(this KeyValuePair<string, SchemaProtocolActionModel> input, string targetProcessId)
        {
            AoAction? aoAction = input.Value.ToAoAction();
            if (aoAction == null)
                return null;

            if(!aoAction.Params.Where(x => x.ParamType == ActionParamType.Target).Any())
                aoAction.Params.Add(new ActionParam() { Key = "Target", Value = targetProcessId, ParamType = ActionParamType.Target });

            ActionMetadata result = new ActionMetadata
            {
                Name = input.Key,
                Title = input.Value.Title,
                Description = input.Value.Description,
                ActionType = ActionType.Message,
                AoAction = aoAction
            };


            return result;
        }

        public static AoAction? ToAoAction(this SchemaProtocolActionModel input)
        {
            var props = input?.Schema?.Tags?.Properties;
            if (props == null)
                return null;

            List<ActionParam> paramList = props.Select(x => x.ToParams())
                .Where(x => x != null)
                .Select(x => x!).ToList();

            AoAction result = new AoAction
            {
                Params = paramList
            };


            return result;
        }

        public static ActionParam? ToParams(this KeyValuePair<string, SchemaProtocolPropertyModel> input)
        {
            ActionParam result = new ActionParam
            {
                Key = input.Key,
                Value = input.Value.Const,
                 ParamType = ActionParamType.Input
            };

            if (!string.IsNullOrWhiteSpace(result.Value))
                result.ParamType = ActionParamType.Filled;
            else if(input.Value.Type == "integer")
                result.ParamType = ActionParamType.Integer;


            return result;
        }
    }
}
