using ArweaveBlazor.Models;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArweaveBlazor
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class ArweaveService : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private readonly Lazy<Task<IJSObjectReference>> arweaveTask;
        //private readonly Lazy<Task<IJSObjectReference>> aoTask;

        public ArweaveService(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => LoadScripts("./_content/ArweaveBlazor/arweaveJsInterop.js", jsRuntime).AsTask());
            arweaveTask = new(() => LoadScripts("https://unpkg.com/arweave/bundles/web.bundle.min.js", jsRuntime).AsTask());
            //aoTask = new(() => LoadScripts("https://www.unpkg.com/@permaweb/aoconnect@0.0.45/dist/browser.js", jsRuntime).AsTask());
            InitArweave();
        }

        private async ValueTask InitArweave()
        {
            var module = await moduleTask.Value;

            await module.InvokeVoidAsync("InitArweave");
        }

        public ValueTask<IJSObjectReference> LoadScripts(string url, IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<IJSObjectReference>("import", url);
        }

        public async ValueTask<bool> HasArConnectAsync()
        {
            var module = await moduleTask.Value;

            var result = await module.InvokeAsync<bool>("HasArConnect");
            Console.WriteLine($"Installed: {result}");
            return result;
        }

        public async ValueTask ConnectAsync(string[] permissions, string? appName = null, string? appLogo = null)
        {
            var module = await moduleTask.Value;

            try
            { 
            var appInfo = new
            {
                name = appName,
                logo = appLogo
            };

            await module.InvokeVoidAsync("Connect", permissions, appInfo);
            }
            catch (JSException jsex)
            { }

            Console.WriteLine($"Connect");
        }

        public async ValueTask DisconnectAsync()
        {
            var module = await moduleTask.Value;

            try
            {
                await module.InvokeVoidAsync("Disconnect");
            }
            catch(JSException jsex)
            { }
        }

        public async ValueTask<string> SendAsync(string processId, string? data, List<Tag>? tags = null)
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<string>("Send", processId, data, tags);
            return result;
        }

        public async ValueTask<T?> SendDryRunAsync<T>(string processId, string? data, List<Tag>? tags = null)
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<T?>("SendDryRun", processId, data, tags);
            return result;
        }


        public async ValueTask<T?> GetResultAsync<T>(string processId, string msgId)
        {
            var module = await moduleTask.Value;

            var result = await module.InvokeAsync<T?>("GetResult", processId, msgId);
            return result;
        }

        public async ValueTask GetResultsAsync(string processId, int limit)
        {
            var module = await moduleTask.Value;

            try
            {
                await module.InvokeVoidAsync("GetResults", processId, limit);
            }
            catch (JSException jsex)
            { }
        }

        public async ValueTask<string> GetWalletBalanceAsync(string address)
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<string>("GetWalletBalance", address);
            return result;
        }

        public async ValueTask<string?> GetActiveAddress()
        {
            var module = await moduleTask.Value;
            var result = await module.InvokeAsync<string?>("GetActiveAddress");
            return result;
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }

       

    }
}