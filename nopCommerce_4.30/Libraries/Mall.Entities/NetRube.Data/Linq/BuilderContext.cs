using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetRube.Data
{
    /// <summary>
    /// 构建器状态上下文
    /// </summary>
    public class BuilderContext
    {
        public BuilderContext(Database database, List<object> agrs)
        {
            this.Database = database;
            this.Parameters = agrs ?? new List<object>();
            
        }
        /// <summary>
        /// 数据库查询对象
        /// </summary>
        public Database Database { get; private set; }
        /// <summary>
        /// 构建参数集合
        /// </summary>
        public List<object> Parameters { get; private set; }
        /// <summary>
        /// 是否启用表别名
        /// </summary>
        public bool WithTableName { get; set; }
        /// <summary>
        /// 忽略行数
        /// </summary>
        public int Skip { get; set; }
        /// <summary>
        /// 获取行数
        /// </summary>
        public int Take { get; set; }
    }
}
