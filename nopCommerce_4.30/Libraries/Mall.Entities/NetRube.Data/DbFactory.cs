using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetRube.Data
{
    /// <summary>
    /// 数据上下文工厂
    /// </summary>
    public class DbFactory : IDisposable
    {
        private static ThreadLocal<Database> dataContext = new ThreadLocal<Database>();
        private static ThreadLocal<MongoDBClient> mongodbContext = new ThreadLocal<MongoDBClient>();

        /// <summary>
        /// 默认连接数据上下文（连接字符串：mysql）
        /// </summary>
        public static Database Default
        {
            get
            {
                if (!dataContext.IsValueCreated)
                {
                    dataContext.Value = new Database("mysql");
                }
                return dataContext.Value;
            }
        }

        /// <summary>
        /// MongoDB连接上下文（连接字符串：MongoDB）
        /// </summary>
        public static MongoDBClient MongoDB
        {
            get
            {
                if (!mongodbContext.IsValueCreated)
                {
                    mongodbContext.Value = new MongoDBClient();
                }
                return mongodbContext.Value;
            }
        }

        //public Database Dynamic(string dataSource, string dataBase, string userid, string pwd)
        //{
        //    if (!dataContext3.IsValueCreated)
        //    {
        //        dataContext3.Value = new Database(string.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3}", dataSource, dataBase, userid, pwd.Base64Decode_()), "System.Data.SqlClient");
        //    }
        //    return dataContext3.Value;
        //}

        //public Database Dynamic(string connectionString)
        //{
        //    if (!dataContext3.IsValueCreated)
        //    {
        //        dataContext3.Value = new Database(connectionString, "System.Data.SqlClient");
        //    }
        //    return dataContext3.Value;
        //}

        //public Database Dynamic(string connectionString, string providerName)
        //{
        //    if (!dataContext3.IsValueCreated)
        //    {
        //        dataContext3.Value = new Database(connectionString, providerName);
        //    }
        //    return dataContext3.Value;
        //}

        //public Database Dynamic(string connectionString, DbProviderFactory provider)
        //{
        //    if (!dataContext3.IsValueCreated)
        //    {
        //        dataContext3.Value = new Database(connectionString, provider);
        //    }
        //    return dataContext3.Value;
        //}

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            dataContext.Value = null;
            dataContext.Dispose();
            mongodbContext.Value = null;
            dataContext.Dispose();
          
        }
    }
}
