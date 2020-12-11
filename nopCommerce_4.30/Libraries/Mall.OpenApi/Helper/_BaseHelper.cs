using Mall.IServices;
using Mall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.OpenApi
{
    public abstract class _BaseHelper
    {
        protected IRegionService _iRegionService;
        protected long shopId = 0;
        protected bool isSelfShop = false;
        protected string sellerName = string.Empty;
        protected ShopHelper _shophelper;
        public _BaseHelper()
        {
            _iRegionService = Mall.ServiceProvider.Instance<IRegionService>.Create;
        }
        /// <summary>
        /// 初始获取店铺信息
        /// </summary>
        /// <param name="app_key"></param>
        protected virtual void InitShopInfo(string app_key)
        {
            _shophelper = new ShopHelper(app_key);
            shopId = _shophelper.ShopId;
            sellerName = _shophelper.SellerName;
            isSelfShop = _shophelper.IsSelf;
        }
    }
}
