using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Configuration;

namespace NetRube.Data
{
    /// <summary>
    /// MongoDB客户端
    /// </summary>
    public class MongoDBClient
    {
        static MongoDBClient()
        {

            var builder = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true);
            var configuration = builder.Build();

            //  var connection = ConfigurationManager.ConnectionStrings["Mall:MongoDB"];
            var connection = configuration["Mall:MongoDB:ConnectionString"];

            if (string.IsNullOrEmpty(connection)) return;
            mongoUrl = new MongoUrl(connection);
        }

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMongoQueryable<T> AsQueryable<T>()
        {
            if (null == Database) return default(IMongoQueryable<T>);
            return Get<T>().AsQueryable();
        }

        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMongoCollection<T> Get<T>()
        {
            if (null == Database) return default(IMongoCollection<T>);
            return Database.GetCollection<T>(typeof(T).Name);
        }

        #region 私有属性

        /// <summary>
        /// 客户端锁
        /// </summary>
        static object clientLocker = new object();

        /// <summary>
        /// 用于保存客户端，全局只实例化一次
        /// </summary>
        static IMongoClient _client = null;

        /// <summary>
        /// Mongodb的连接串
        /// </summary>
        static MongoUrl mongoUrl = null;

        /// <summary>
        /// 客户端
        /// </summary>
        static IMongoClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (clientLocker)
                    {
                        if (null == mongoUrl) return null;
                        if (_client == null)
                            _client = new MongoClient(mongoUrl);
                    }
                }
                return _client;
            }
        }

        /// <summary>
        /// MongoDB数据库实例
        /// </summary>
        static IMongoDatabase Database
        {
            get
            {
                if (null == Client) return null;
                return Client.GetDatabase(mongoUrl.DatabaseName);
            }
        }

        #endregion
    }
}
