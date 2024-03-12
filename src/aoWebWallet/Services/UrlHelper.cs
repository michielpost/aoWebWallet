namespace aoWebWallet.Services
{
    public static class UrlHelper
    {
        public static string? GetArweaveUrl(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return $"https://arweave.net/{id}";
        }
    }
}
