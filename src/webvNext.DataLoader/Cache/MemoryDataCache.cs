using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webvNext.DataLoader.Cache
{
    public class MemoryDataCache
    {
        private Dictionary<string, CacheObject> storage = new();

        //private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);


        /// <summary>
        /// Get object based on key, or generate the value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="generate"></param>
        /// <param name="expireDate"></param>
        /// <param name="forceRefresh"></param>
        /// <param name="serializerType">JSON or XML serializer</param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string key, Func<Task<T>> generate, TimeSpan? cacheDuration = null, bool forceRefresh = false)
        {
            T? value;

            //Force bypass of cache?
            if (!forceRefresh)
            {
                //Check cache
                value = GetFromCache<T>(key);

                if (!EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    return value;
                }
            }

            value = await generate().ConfigureAwait(false);
            Set(key, value, cacheDuration.HasValue ? DateTimeOffset.UtcNow.Add(cacheDuration.Value) : null);

            return value;

        }

        /// <summary>
        /// Get value from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="serializerType">JSON or XML serializer</param>
        /// <returns></returns>
        public T? GetFromCache<T>(string key)
        {
            //Get cache value
            try
            {
                var value = storage.GetValueOrDefault(key);

                if (value == null)
                    return default;
                else if (value.IsValid)
                    return ((CacheObject<T>)value).File;
                else
                {
                    //Delete old value
                    //Do not await
                    Delete(key);
                }
            }
            catch
            {
                //Restoring from cache might throw an error
                //Don't crash the app, return default value
            }

            return default;
        }

        private void Delete(string key)
        {
            storage.Remove(key);
        }

        /// <summary>
        /// Set value in cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireDate"></param>
        /// <param name="serializerType">JSON or XML serializer</param>
        /// <returns></returns>
        public void Set<T>(string key, T value, DateTimeOffset? expireDate = null)
        {
            CacheObject<T> cacheFile = new CacheObject<T>() { File = value, ExpireDateTime = expireDate };

            storage[key] = cacheFile;
        }

        public void Clear()
        {
            storage = new();
        }
    }
}
