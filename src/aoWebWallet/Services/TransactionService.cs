using aoWebWallet.Extensions;
using aoWebWallet.Models;
using ArweaveBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class TransactionService(ArweaveService arweaveService) : ObservableObject
    {
        public void Reset()
        {
            LastTransaction.Data = null;
        }
        public DataLoaderViewModel<Transaction> LastTransaction { get; set; } = new();

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

        public async Task SendAction(Wallet wallet, Wallet? ownerWallet, AoAction action)
        {
            if (wallet.Source == WalletTypes.ArConnect)
            {
                var activeAddress = await GetActiveArConnectAddress();
                if(activeAddress == wallet.Address)
                    await SendActionWithArConnect(action);
            }

            if (ownerWallet?.Source == WalletTypes.ArConnect)
            {
                var activeAddress = await GetActiveArConnectAddress();
                if (activeAddress == ownerWallet.Address)
                    await SendActionWithEvalWithArConnect(wallet.Address, action);
            }

            if (!string.IsNullOrEmpty(wallet.OwnerAddress) && ownerWallet?.Address == wallet.Address
                && !string.IsNullOrEmpty(ownerWallet?.Jwk))
            {
                Console.WriteLine("eval");

                await SendActionWithEval(ownerWallet.Jwk, wallet.Address, action);

            }

            if (!string.IsNullOrEmpty(wallet.Jwk))
                await SendActionWithJwk(wallet.Jwk, action);

            Console.WriteLine("nothing");
            return;
        }

        private async Task SendActionWithEvalWithArConnect(string processId, AoAction action)
        {
            var activeAddress = await GetActiveArConnectAddress();
            if (string.IsNullOrEmpty(activeAddress))
                return;

            await SendActionWithEval(null, processId, action);
        }

        private async Task SendActionWithArConnect(AoAction action)
        {
            var activeAddress = await GetActiveArConnectAddress();
            if (string.IsNullOrEmpty(activeAddress))
                return;

            await SendActionWithJwk(null, action);
        }

        private Task<Transaction?> SendActionWithEval(string? jwk, string processId, AoAction action)
         => LastTransaction.DataLoader.LoadAsync(async () =>
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
         }, x => LastTransaction.Data = x);

        private Task<Transaction?> SendActionWithJwk(string? jwk, AoAction action)
           => LastTransaction.DataLoader.LoadAsync(async () =>
           {
               if (action.Target?.Value == null)
                   return null;

               var transferTags = action.ToTags();
               transferTags.Add(new ArweaveBlazor.Models.Tag() { Name = "X-Wallet", Value = "aoww" });

               var idResult = await arweaveService.SendAsync(jwk, action.Target.Value, null, null, transferTags);

               return new Transaction { Id = idResult };
           }, x => LastTransaction.Data = x);

       


    }
}
