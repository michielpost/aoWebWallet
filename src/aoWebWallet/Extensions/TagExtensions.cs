using ArweaveBlazor.Models;

namespace aoWebWallet.Extensions
{
    public static class TagExtensions
    {
        public static string ToSendCommand(this List<ArweaveBlazor.Models.Tag> tagList)
        {
            var tagListString = string.Join(", ", tagList.Select(x => x.ToSendCommand()));
            return "{" + tagListString + "}";
        }

        public static string ToSendCommand(this ArweaveBlazor.Models.Tag tag)
        {
            return $"{tag.Name} = \"{tag.Value}\"";
        }
    }
}
