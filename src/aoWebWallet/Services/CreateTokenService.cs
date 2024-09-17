using aoWebWallet.Models;
using ArweaveBlazor;
using ArweaveBlazor.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using webvNext.DataLoader;

namespace aoWebWallet.Services
{
    public class CreateTokenService(ArweaveService arweaveService, TransactionService transactionService) : ObservableObject
    {
        public DataLoaderViewModel<string> CreateTokenProgress { get; set; } = new();

        public void Reset()
        {
            CreateTokenProgress = new();
        }

        public Task<string?> CreateToken(Wallet wallet, CreateTokenModel tokenModel)
            => CreateTokenProgress.DataLoader.LoadAsync(async () =>
            {

                var address = wallet.Address;
                string? jwk = wallet.Jwk;

                string data = EmbeddedResourceReader.ReadResource("aoWebWallet.ProcessTemplates.token.lua");
                
                data = data.Replace("ao.id", $"\"{address}\"");
                data = data.Replace("$Denomination$", tokenModel.Denomination.ToString());
                data = data.Replace("$Ticker$", tokenModel.Ticker);
                data = data.Replace("$Logo$", tokenModel.LogoUrl);

                string moduleId = "zx6_08gJzKNXxLCplINj6TPv9-ElRgeRqr9F6riRBK8";
                //string previewModuleId = "PSPMkkFrJzYI2bQbkmeEQ5ONmeR-FJZu0fNQoSCU1-I";

                CreateTokenProgress.DataLoader.ProgressMsg = "Deploying new process...";
                CreateTokenProgress.ForcePropertyChanged();

                string newProcessId = await arweaveService.CreateProcess(jwk, moduleId, new List<Tag> {
                new Tag { Name = "App-Name", Value  = "aoww" },
                new Tag() { Name = "Name", Value = tokenModel.Name ?? string.Empty},

            }
                );

                Console.WriteLine("process finish");


                if (string.IsNullOrWhiteSpace(newProcessId))
                {
                    CreateTokenProgress.DataLoader.ProgressMsg = "Failed to create new process";
                    return null;
                }
                else
                {
                    Console.WriteLine("processId: " + newProcessId);
                }

                string? dataId = null;
                int retryCount = 0;
                const int maxRetries = 10;
                const int retryDelay = 2000; // 2 seconds

                CreateTokenProgress.DataLoader.ProgressMsg = "Submitting token data...";
                CreateTokenProgress.ForcePropertyChanged();

                while (dataId == null && retryCount < maxRetries)
                {
                    try
                    {
                        await Task.Delay(retryDelay);
                        dataId = await arweaveService.SendAsync(jwk, newProcessId, address, data, new List<Tag>
                    {
                        new Tag() { Name = "Action", Value = "Eval"},
                    });
                        Console.WriteLine("DataId: " + dataId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Attempt {retryCount + 1} failed: {ex.Message}");
                        retryCount++;
                    }
                }

                if (dataId == null)
                {
                    CreateTokenProgress.DataLoader.ProgressMsg = $"Failed to reach process after maximum retries ({maxRetries})";
                    return null;
                }

                if (dataId != null)
                {
                    CreateTokenProgress.DataLoader.ProgressMsg = "Minting new tokens...";
                    CreateTokenProgress.ForcePropertyChanged();

                    var mintResult = await arweaveService.SendAsync(jwk, newProcessId, null, null, new List<ArweaveBlazor.Models.Tag>
                    {
                        new ArweaveBlazor.Models.Tag { Name = "Target", Value = newProcessId},
                        new ArweaveBlazor.Models.Tag { Name = "Action", Value = "Mint"},
                        new ArweaveBlazor.Models.Tag { Name = "Quantity", Value = tokenModel.MintQuantityForTag}
                    });
                    Console.WriteLine("mintResult: " + mintResult);

                }

                CreateTokenProgress.DataLoader.ProgressMsg = "Token created successfully!";
                return newProcessId;
            }, x => CreateTokenProgress.Data = x);
    }
}