using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Core.Helper
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Primitives;
    using Nop.Core.Infrastructure;
    using System;
    using System.Collections;
    using System.Threading;

    namespace Mall.Core.Helper
    {
        /// <summary>
        /// 自定义缓存帮助类
        /// </summary>
        public static class CacheHelper
        {
            static IMemoryCache cache;
            static object cacheLocker = new object();

           

            static CacheHelper()
            {
                cache =   EngineContext.Current.Resolve<IMemoryCache>();
            }

            /// <summary>
            /// 获得指定键的缓存值
            /// </summary>
            /// <param name="key">缓存键</param>
            /// <returns>缓存值</returns>
            public static object Get(string key)
            {
                return cache.Get(key);
            }

            /// <summary>
            /// 获得指定键的缓存值
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key">缓存键</param>
            /// <returns></returns>
            public static T Get<T>(string key)
            {
                T result = (T)cache.Get(key);
                return result;
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中
            /// </summary>
            /// <param name="key">缓存键</param>
            /// <param name="value">缓存值</param>
            public static void Insert(string key, object value)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);


                  
                    cache.Set(key, value);
                    
                }
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中，并指定过期时间
            /// </summary>
            /// <param name="key">缓存键</param>
            /// <param name="data">缓存值</param>
            /// <param name="cacheTime">缓存过期时间(秒)</param>
            public static void Insert(string key, object value, int cacheTime)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);
                    cache.Set(key, value,  DateTime.Now.AddSeconds(cacheTime));
                }
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中，并指定过期时间
            /// </summary>
            /// <param name="key">缓存键</param>
            /// <param name="value">缓存值</param>
            /// <param name="cacheTime">缓存过期时间</param>
            public static void Insert(string key, object value, DateTime cacheTime)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);
                    cache.Set(key, value,  cacheTime);
                }
            }

            /// <summary>
            /// 从缓存中移除指定键的缓存值
            /// </summary>
            /// <param name="key">缓存键</param>
            public static void Remove(string key)
            {
                cache.Remove(key);
            }

            /// <summary>
            /// 清空所有缓存对象
            /// </summary>
            public static void Clear()
            {

                cache = new MemoryCache(new MemoryCacheOptions());




            }

            /// <summary>
            /// 缓存是否存在
            /// </summary>
            /// <param name="key">缓存键</param>
            /// <returns></returns>
            public static bool Exists(string key)
            {
                if (cache.Get(key) != null)
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key">缓存键</param>
            /// <param name="value">缓存值</param>
            public static void Insert<T>(string key, T value)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);
                    cache.Set(key, value);
                }
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中，并指定过期时间
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key">缓存键</param>
            /// <param name="value">缓存值</param>
            /// <param name="cacheTime">缓存过期时间(秒)</param>
            public static void Insert<T>(string key, T value, int cacheTime)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);
                    cache.Set(key, value,  DateTime.Now.AddSeconds(cacheTime));
                }
            }

            /// <summary>
            /// 将指定键的对象添加到缓存中，并指定过期时间
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key">缓存键</param>
            /// <param name="value">缓存值</param>
            /// <param name="cacheTime">缓存过期时间</param>
            public static void Insert<T>(string key, T value, DateTime cacheTime)
            {
                lock (cacheLocker)
                {
                    if (cache.Get(key) != null)
                        cache.Remove(key);
                    cache.Set(key, value,  cacheTime);
                }
            }
        }
    }
}
