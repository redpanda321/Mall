namespace Mall.DTO
{
    public class QQGetAddressByLatLngResult
    {
        /// <summary>
        /// 状态码，0为正常,
        ///310请求参数信息有误，
        ///311Key格式错误,
        ///306请求有护持信息请检查字符串,
        ///110请求来源未被授权
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 状态说明
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 逆地址解析结果
        /// </summary>
        public QQResult result { get; set; }
    }
    public class QQResult
    {
        /// <summary>
        /// 地址描述
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 位置描述
        /// </summary>
        public QQFormattedAddresses formatted_addresses { get; set; }
        /// <summary>
        /// 地址部件，address不满足需求时可自行拼接
        /// </summary>
        public QQAddressComponent address_component { get; set; }
        /// <summary>
        /// 坐标相对位置参考
        /// </summary>
        public QQAddress_Reference address_reference { get; set; }
    }

    public class QQFormattedAddresses
    {
        /// <summary>
        /// 经过腾讯地图优化过的描述方式，更具人性化特点
        /// </summary>
        public string recommend { get; set; }
        /// <summary>
        /// 大致位置，可用于对位置的粗略描述
        /// </summary>
        public string rough { get; set; }
    }

    public class QQAddress_Reference
    {
        /// <summary>
        /// 乡镇街道
        /// </summary>
        public QQTown town { get; set; }
    }

    public class QQTown
    {
        /// <summary>
        /// 名称/标题
        /// </summary>
        public string title { get; set; }
    }

    public class QQAddressComponent
    {
        /// <summary>
        /// 国家
        /// </summary>
        public string nation { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 区，可能为空字串
        /// </summary>
        public string district { get; set; }
        /// <summary>
        /// 街道，可能为空字串
        /// </summary>
        public string street { get; set; }
        /// <summary>
        /// 门牌，可能为空字串
        /// </summary>
        public string street_number { get; set; }
    }
}
