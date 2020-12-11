using Mall.Core;
using Microsoft.Extensions.Caching.Memory;
using Nop.Core.Infrastructure;
using System;
using System.Collections;

using System.Collections.Generic;

namespace Mall.Strategy.Cache
{
    public class AspNetCache : ICache
    {
        private IMemoryCache cache;
        static object cacheLocker = new object();

        public AspNetCache()
        {
            cache = EngineContext.Current.Resolve<IMemoryCache>();
        }

        /// <summary>
        /// 获得指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值</returns>
        public object Get(string key)
        {
            return cache.Get(key);
        }

        public T Get<T>(string key)
        {
            T result = default(T);
            var obj = cache.Get(key);
            if (null != obj)
            {
                result = (T)obj;
            }
            return result;
        }

        /// <summary>
        /// 将指定键的对象添加到缓存中
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Insert(string key, object value)
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
        /// <param name="cacheTime">缓存过期时间</param>
        public void Insert(string key, object value, int cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);

                cache.Set(key, value, DateTime.Now.AddSeconds(cacheTime));

            }
        }


        public void Insert(string key, object value, DateTime cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Set(key, value, cacheTime);
            }
        }

        /// <summary>
        /// 从缓存中移除指定键的缓存值
        /// </summary>
        /// <param name="key">缓存键</param>
        public void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// 清空所有缓存对象
        /// </summary>
        public void Clear()
        {
            return;
        }

        public void Send(string key, object data)
        {
            return;
        }

        public void Recieve<T>(string key, Core.Cache.DoSub dosub)
        {
            return;
        }

        public void RegisterSubscribe<T>(string key, Core.Cache.DoSub dosub)
        {
            return;
        }

        public void UnRegisterSubscrib(string key)
        {
            return;
        }

        public bool Exists(string key)
        {
            if (cache.Get(key) != null)
                return true;
            else
                return false;
        }

        public void Insert<T>(string key, T value)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Set(key, value);
            }
        }

        public void Insert<T>(string key, T value, int cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);

                cache.Set(key, value, DateTime.Now.AddSeconds(cacheTime));
            }
        }

        public void Insert<T>(string key, T value, DateTime cacheTime)
        {
            lock (cacheLocker)
            {
                if (cache.Get(key) != null)
                    cache.Remove(key);
                cache.Set(key, value, cacheTime);
            }
        }

        public bool SetNX(string key, string value)
        {
            throw new NotImplementedException();
        }

        public bool SetNX(string key, string value, int cacheTime)
        {
            throw new NotImplementedException();
        }

        public ICacheLocker GetCacheLocker(string key)
        {
            throw new NotImplementedException();
        }

        const int DEFAULT_TMEOUT = 600;//默认超时时间（单位秒）

        private int _timeout = DEFAULT_TMEOUT;

        /// <summary>
        /// 缓存过期时间
        /// </summary>
        /// <value></value>
        public int TimeOut
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value > 0 ? value : DEFAULT_TMEOUT;
            }
        }




    }
}
