using aoWebWallet.Models;
using Microsoft.Extensions.Options;

namespace aoWebWallet.Services
{
    public class GatewayUrlHelper
    {
        private readonly GatewayConfig config;

        public GatewayUrlHelper(IOptions<GatewayConfig> config)
        {
            this.config = config.Value;
        }

        public string? GetArweaveUrl(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            Uri combinedUri = new Uri(new Uri(config.GatewayUrl), id);
            return combinedUri.ToString();
        }
    }
}
