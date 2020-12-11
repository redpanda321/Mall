using Senparc.CO2NET.Helpers;
using System;

namespace Hishop.Weixin.MP.Util
{
    public class CheckSignature
    {
        public static readonly string Token = "weixin_test";

        public static bool Check(string signature, string timestamp, string nonce, string token)
        {
            token = token ?? Token;
            string[] array = new string[] { timestamp, nonce, token };
            Array.Sort<string>(array);
           
            string str = EncryptHelper.GetSha1(string.Join("", array));

            return (signature == str.ToLower());
        }
    }
}

