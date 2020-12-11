using NetRube.Data;
using NPoco;

namespace Mall.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Db
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public partial class Record<T>
        {
            private bool? _enableLazyLoad = null;

            /// <summary>
            /// 是否开启延迟加载
            /// </summary>
            [ResultColumn]
            public bool EnableLazyLoad
            {
                get
                {
                    if (!_enableLazyLoad.HasValue)
                        _enableLazyLoad = DbFactory.Default.EnableLazyLoad;
                    return _enableLazyLoad.Value;
                }
            }

            /// <summary>
            /// 是否开启关联属性(true则关联属性为null或空集合)
            /// </summary>
            [ResultColumn]
            public bool IgnoreReference { get; set; }

            /// <summary>
            /// 获取图片服务器所在路径
            /// </summary>
            protected string ImageServerUrl = "";
        }
    }

    /// <summary>
    /// DTO基类
    /// </summary>
    public abstract class BaseModel
    {
        protected string ImageServerUrl = "";
    }
}
