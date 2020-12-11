using NPoco;

namespace Mall.Entities
{
    public partial class MemberInfo
    {
        protected string ImageServerUrl = "";
        /// <summary>
        /// 头像
        /// </summary>
        [ResultColumn]
        public string PhotoUrl
        {
            get { return ImageServerUrl + Photo; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ImageServerUrl))
                    Photo = value.Replace(ImageServerUrl, "");
                else
                    Photo = value;
            }
        }
        /// <summary>
        /// 显示昵称
        /// <para>无昵称则显示用户名</para>
        /// </summary>
        [ResultColumn]
        public string ShowNick
        {
            get
            {
                string result = this.Nick;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = this.UserName;
                }
                return result;
            }
        }

        /// <summary>
        /// 是否是微信用户
        /// </summary>
        [ResultColumn]
        public bool IsWeiXinUser
        {
            get
            {
                return !string.IsNullOrEmpty(this.PasswordSalt) && this.PasswordSalt.StartsWith("o");
            }
        }
        /// <summary>
        /// 会员折扣(0.00-1)
        /// </summary>
        [ResultColumn]
        public decimal MemberDiscount { get; set; }

        /// <summary>
        /// 是否刚新注册用户
        /// </summary>
        [ResultColumn]
        public bool IsNewAccount { get; set; }
    }
}
