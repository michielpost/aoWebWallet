using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Reflection;
using MudBlazor;
using MudBlazor.Services;
using webvNext.DataLoader.Cache;
using Blazored.LocalStorage;
using aoWebWallet.Services;
using aoWebWallet.ViewModels;
using ArweaveAO;
using ArweaveBlazor;
using System.Globalization;
using ClipLazor.Extention;
using aoww.Services;
using aoww.Services.Models;
using aoWebWallet.Models;

namespace aoWebWallet
{
    public class Program
    {
        public static string? Version { get; set; }
        public static string PageTitlePostFix => "aoWebWallet";

        public static string? GetVersionHash()
        {
            if (Version != null)
            {
                string shortVersion = Version;
                int sep = shortVersion.LastIndexOf('-');
                if (sep >= 0 && sep < shortVersion.Length)
                    shortVersion = shortVersion.Substring(sep + 1);

                int sep1 = shortVersion.LastIndexOf('+');
                if (sep1 >= 0 && sep1 < shortVersion.Length)
                    shortVersion = shortVersion.Substring(sep1 + 1);

                if(shortVersion.Length > 7)
                    shortVersion = shortVersion.Substring(0, 7);

                return shortVersion;
            }
            return null;
        }

        public static string? GetVersionWithoutHash()
        {
            if (Version != null)
            {
                string shortVersion = Version;
                int sep = shortVersion.LastIndexOf('-');
                if (sep >= 0)
                    return shortVersion.Substring(0, sep);

                int sep1 = shortVersion.IndexOf('+');
                if (sep1 >= 0 && sep1 < shortVersion.Length)
                    shortVersion = shortVersion.Substring(0, sep1);

                return shortVersion;

            }
            return Version;
        }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);

            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, string baseAddress)
        {
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            //Set Version
            Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

            //Services
            services.AddScoped<TokenDataService>();
            services.AddScoped<StorageService>();
            services.AddScoped<ClipboardService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<GatewayUrlHelper>();

            services.AddSingleton<MemoryDataCache>();

            services.AddScoped<TokenClient>();
            services.AddScoped<GraphqlClient>();

            services.AddArweaveBlazor();
            services.AddScoped<AODataClient>();


            //Register ViewModels
            services.AddScoped<MainViewModel>();
            services.AddScoped<TokenDetailViewModel>();
            services.AddScoped<TransactionDetailViewModel>();
            services.AddScoped<WalletDetailViewModel>();

            services.AddBlazoredLocalStorage();

            services.AddClipboard();

            //Options
            services.AddSingleton(new GraphqlConfig());
            services.AddSingleton(new GatewayConfig());
        }
    }
}
