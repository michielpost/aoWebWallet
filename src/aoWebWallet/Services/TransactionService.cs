using aoWebWallet.Extensions;
using aoWebWallet.Models;
using aoww.ProcesModels.Action;
using ArweaveAO.Requests;
using ArweaveAO.Responses;
using ArweaveBlazor;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class TransactionService(ArweaveService arweaveService, 
        ArweaveAO.AODataClient aODataClient,
        TokenDataService tokenDataService) : ObservableObject
    {
        public void Reset()
        {
            LastTransaction.Data = null;
            DryRunResult.Data = null;
        }
        public DataLoaderViewModel<Transaction> LastTransaction { get; set; } = new();
        public DataLoaderViewModel<MessageResult> DryRunResult { get; set; } = new();

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

        public Task<MessageResult?> DryRunAction(Wallet wallet, AoAction action)
             => DryRunResult.DataLoader.LoadAsync(async () =>
             {
                 DryRunResult.Data = null;

                 var target = action.Target?.Value ?? string.Empty;
                 var druRunRequest = new DryRunRequest()
                 {
                     Target = target,
                     Owner = wallet.Address,
                     Tags = action.ToDryRunTags(),
                     Data = action.DataValue
                 };

                 var result = await aODataClient.DryRun(target, druRunRequest);

                 var balanceInputs = action.AllInputs.Where(x => x.ParamType == ActionParamType.Balance);
                 foreach (var balanceInput in balanceInputs) 
                 {
                     if (balanceInput.Value == null)
                         continue;

                     var token = tokenDataService.TokenList.Where(x => x.TokenId == balanceInput.Args.FirstOrDefault()).FirstOrDefault();

                     if(token?.TokenData?.Denomination != null)
                     {
                         string original1 = $"You received {balanceInput.Value}";
                         string original2 = $"You transferred {balanceInput.Value}";

                         long longValue = long.Parse(balanceInput.Value);
                         var formatValue = BalanceHelper.FormatBalance(longValue, token.TokenData.Denomination.Value);

                         string replace1 = $"You received {formatValue} {token.TokenData.Ticker}";
                         string replace2 = $"You transferred {formatValue} {token.TokenData.Ticker}";

                         foreach(var msg in result?.Messages ?? new())
                         {
                             msg.Data = RemoveColorCodes(msg.Data);
                             msg.Data = msg.Data.Replace(original1, replace1);
                             msg.Data = msg.Data.Replace(original2, replace2);
                         }

                     }
                 }

                 return result;
             }, x => DryRunResult.Data = x);

        static string RemoveColorCodes(string? input)
        {
            if (input == null)
                return string.Empty;

            // Define a regular expression pattern to match color codes
            string pattern = @"\x1B\[[0-9;]*[mK]";

            // Replace color codes with an empty string
            string output = System.Text.RegularExpressions.Regex.Replace(input, pattern, "");
            return output;
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

            if (!string.IsNullOrEmpty(wallet.OwnerAddress) && ownerWallet?.Address == wallet.OwnerAddress
                && !string.IsNullOrEmpty(ownerWallet?.Jwk))
            {
                return await SendActionWithEval(ownerWallet.Jwk, wallet.Address, action);
            }

            if (!string.IsNullOrEmpty(wallet.Jwk))
                return await SendActionWithJwk(wallet.Jwk, action);

            //Console.WriteLine("No Wallet to send");
            return null;
        }

        private async Task<Transaction?> SendActionWithEvalWithArConnect(string processId, AoAction action)
        {
            var activeAddress = await GetActiveArConnectAddress();
            if (string.IsNullOrEmpty(activeAddress))
                return null;

            return await SendActionWithEval(null, processId, action);
        }

        private async Task<Transaction?> SendActionWithArConnect(AoAction action)
        {
            var activeAddress = await GetActiveArConnectAddress();
            if (string.IsNullOrEmpty(activeAddress))
                return null;

            return await SendActionWithJwk(null, action);
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

               var idResult = await arweaveService.SendAsync(jwk, action.Target.Value, null, action.DataValue, transferTags);

               return new Transaction { Id = idResult };
           }, x => LastTransaction.Data = x);

       


    }
}
