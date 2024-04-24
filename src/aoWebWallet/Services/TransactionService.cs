using aoWebWallet.Extensions;
using aoWebWallet.Models;
using ArweaveBlazor;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class TransactionService(ArweaveService arweaveService)
    {
        public DataLoaderViewModel<Transaction> LastTransactionId { get; set; } = new();

        public async Task<string?> GetActiveArConnectAddress()
        {
            bool hasArConnectExtension = await arweaveService.HasArConnectAsync();

            if (hasArConnectExtension)
            {
                var address = await arweaveService.GetActiveAddress();

                return address;
                
            }

            return null;
        }

        public async Task<Transaction?> SendAction(Wallet wallet, Wallet? ownerWallet, AoAction action)
        {
            if (wallet.Source == WalletTypes.ArConnect)
            {
                var activeAddress = await GetActiveArConnectAddress();
                if(activeAddress == wallet.Address)
                    return await SendActionWithArConnect(action);
            }

            if (ownerWallet?.Source == WalletTypes.ArConnect)
            {
                var activeAddress = await GetActiveArConnectAddress();
                if (activeAddress == ownerWallet.Address)
                    return await SendActionWithEvalWithArConnect(wallet.Address, action);
            }

            if (!string.IsNullOrEmpty(wallet.OwnerAddress) && ownerWallet?.Address == wallet.Address
                && !string.IsNullOrEmpty(ownerWallet?.Jwk))
            {
                return await SendActionWithEval(ownerWallet.Jwk, wallet.Address, action);

            }

            if (!string.IsNullOrEmpty(wallet.Jwk))
                return await SendActionWithJwk(wallet.Jwk, action);

            return null;

        }

        private Task<Transaction?> SendActionWithEvalWithArConnect(string processId, AoAction action)
          => LastTransactionId.DataLoader.LoadAsync(async () =>
          {
              var activeAddress = await GetActiveArConnectAddress();
              if (string.IsNullOrEmpty(activeAddress))
                  return null;

              return await SendActionWithEval(null, processId, action);
          });

        private Task<Transaction?> SendActionWithEval(string? jwk, string processId, AoAction action)
         => LastTransactionId.DataLoader.LoadAsync(async () =>
         {

             var transferTags = action.ToEvalTags();

             var data = $"Send({transferTags.ToSendCommand()})";

             var evalTags = new List<ArweaveBlazor.Models.Tag>
             {
                    new ArweaveBlazor.Models.Tag() { Name = "Action", Value = "Eval"},
                    new ArweaveBlazor.Models.Tag() { Name = "X-Wallet", Value = "aoww"},
             };

             var idResult = await arweaveService.SendAsync(jwk, processId, null, data, evalTags);

             return new Transaction { Id = idResult };
         });

        private Task<Transaction?> SendActionWithArConnect(AoAction action)
           => LastTransactionId.DataLoader.LoadAsync(async () =>
           {
               var activeAddress = await GetActiveArConnectAddress();
               if (string.IsNullOrEmpty(activeAddress))
                   return null;

               return await SendActionWithJwk(null, action);
           });

        private Task<Transaction?> SendActionWithJwk(string? jwk, AoAction action)
           => LastTransactionId.DataLoader.LoadAsync(async () =>
           {
               if (action.Target?.Value == null)
                   return null;

               var transferTags = action.ToTags();
               transferTags.Add(new ArweaveBlazor.Models.Tag() { Name = "X-Wallet", Value = "aoww" });

               var idResult = await arweaveService.SendAsync(jwk, action.Target.Value, null, null, transferTags);

               return new Transaction { Id = idResult };
           });

       


    }
}
