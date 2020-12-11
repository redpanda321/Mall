using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class WXSmallProgramApplication
    {
        private static IWXSmallProgramService _iWXSmallProgramService =  EngineContext.Current.Resolve<IWXSmallProgramService>();
        private static IProductService _iProductService =  EngineContext.Current.Resolve<IProductService>();


        public static void SetWXSmallProducts(string productIds)
        {
            List<Entities.ProductInfo> lProductInfo = new List<Entities.ProductInfo>();
            var lbId = _iWXSmallProgramService.GetWXSmallProducts().Select(item => item.ProductId).ToList();
            if (!string.IsNullOrEmpty(productIds))
            {
                var productIdsArr = productIds.Split(',').Select(item => long.Parse(item)).ToList();
                lProductInfo = _iProductService.GetAllProductByIds(productIdsArr);
                foreach (Entities.ProductInfo item in lProductInfo)
                {
                    if (!lbId.Contains(Convert.ToInt32(item.Id)))
                    {
                        Entities.WXSmallChoiceProductInfo mProductsInfo = new Entities.WXSmallChoiceProductInfo()
                        {
                            ProductId = Convert.ToInt32(item.Id)
                        };
                        _iWXSmallProgramService.AddWXSmallProducts(mProductsInfo);
                    }
                }
            }
        }
    }
}
