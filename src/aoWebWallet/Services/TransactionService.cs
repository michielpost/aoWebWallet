using aoWebWallet.Extensions;
using aoWebWallet.Models;
using ArweaveAO.Requests;
using ArweaveBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class TransactionService(ArweaveService arweaveService, ArweaveAO.AODataClient aODataClient) : ObservableObject
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

        public async Task DryRunAction(Wallet wallet, Wallet? ownerWallet, AoAction action)
        {
            var target = action.Target?.Value ?? string.Empty;
            var druRunRequest = new DryRunRequest()
            {
                Target = target,
                Tags = action.ToDryRunTags()
            };

            var result = aODataClient.DryRun(target, druRunRequest);

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

            if (!string.IsNullOrEmpty(wallet.OwnerAddress) && ownerWallet?.Address == wallet.OwnerAddress
                && !string.IsNullOrEmpty(ownerWallet?.Jwk))
            {
                await SendActionWithEval(ownerWallet.Jwk, wallet.Address, action);
            }

            if (!string.IsNullOrEmpty(wallet.Jwk))
                await SendActionWithJwk(wallet.Jwk, action);

            Console.WriteLine("No Wallet to send");
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
