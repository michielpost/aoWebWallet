using aoww.ProcesModels.Action;

namespace aoWebWallet.Extensions
{
    public static class AcActionParamExtensions
    {
        public static List<ArweaveBlazor.Models.Tag> ToEvalTags(this AoAction action)
        {
            return action.Params.Select(x => new ArweaveBlazor.Models.Tag { Name = x.Key, Value = x.Value ?? string.Empty }).ToList();
        }

        public static List<ArweaveBlazor.Models.Tag> ToTags(this AoAction action)
        {
            return action.AllWithoutTarget.Select(x => new ArweaveBlazor.Models.Tag { Name = x.Key, Value = x.Value ?? string.Empty }).ToList();
        }

        public static List<ArweaveAO.Models.Tag> ToDryRunTags(this AoAction action)
        {
            return action.AllWithoutTarget.Select(x => new ArweaveAO.Models.Tag { Name = x.Key, Value = x.Value ?? string.Empty }).ToList();
        }
    }
}
