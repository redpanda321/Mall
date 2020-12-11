using Mall.Core;
using Mall.PaymentPlugin;

namespace Mall.WeixinPaymentBase
{
    public class Config:ConfigBase
    {

        public string AppId { get; set; }


        public string AppSecret { get; set; }


        public string MCHID { get; set; }


        public string Key { get; set; }

        public string Logo { get; set; }

        public string HelpImage { get; set; }

        public string pkcs12 { get; set; }
    }
}
