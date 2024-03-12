using Microsoft.Extensions.DependencyInjection;

namespace ArweaveBlazor
{
    public static class ServiceCollectionExtensions
    {
        public static void AddArweaveBlazor(this IServiceCollection services)
        {
            services.AddScoped<ArweaveService>();
        }
    }
}
