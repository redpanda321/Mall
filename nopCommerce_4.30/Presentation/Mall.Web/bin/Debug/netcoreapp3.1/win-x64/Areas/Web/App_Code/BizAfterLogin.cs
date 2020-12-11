using Mall.Web.Framework;

namespace Mall.Web.Areas.Web
{
    public static class BizAfterLogin
    {
        public static void Run(long memberId)
        {
            CartHelper cart = new CartHelper();
            //同步客户端购物车信息至服务器
            cart.UpdateCartInfoFromCookieToServer(memberId);
            BranchCartHelper branchcart = new BranchCartHelper();
            //同步客户端购物车信息至服务器
            branchcart.UpdateCartInfoFromCookieToServer(memberId);
        }

    }
}