using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webvNext.DataLoader.Cache
{
    /// <summary>
    /// Used as a wrapper around the stored file to keep metadata
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheObject
    {
        /// <summary>
        /// Expire date of cached file
        /// </summary>
        public DateTimeOffset? ExpireDateTime { get; set; }

        /// <summary>
        /// Is the cache file valid?
        /// </summary>
        public bool IsValid
        {
            get
            {
                return (ExpireDateTime == null || ExpireDateTime.Value > DateTimeOffset.UtcNow);
            }
        }
    }

    /// <summary>
    /// Used as a wrapper around the stored file to keep metadata
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheObject<T> : CacheObject
    {
        /// <summary>
        /// Actual file being stored
        /// </summary>
        public T? File { get; set; }
    }
}
