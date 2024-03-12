using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ArweaveAO
{
    public abstract class ClientAPI
    {
        protected readonly HttpClient Http;
        private readonly string BaseRoute;

        protected ClientAPI(string baseRoute, HttpClient http)
        {
            BaseRoute = baseRoute;
            Http = http;
        }

        protected async Task<TReturn?> GetAsync<TReturn>(string relativeUri)
        {
            HttpResponseMessage res = await Http.GetAsync($"{BaseRoute}/{relativeUri}");
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<TReturn>();
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }

        protected async Task<TReturn?> PostAsync<TReturn, TRequest>(string relativeUri, TRequest request)
        {
            HttpResponseMessage res = await Http.PostAsJsonAsync<TRequest>($"{BaseRoute}/{relativeUri}", request);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadFromJsonAsync<TReturn>();
            }
            else
            {
                string msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }
    }
}
