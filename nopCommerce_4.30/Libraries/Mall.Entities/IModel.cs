using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// 修改的字段列名
        /// </summary>
        IEnumerable<string> ModifiedColumns { get; }
        /// <summary>
        /// 修改实体完成
        /// </summary>
        void ModifiedComplete();
    }
}
