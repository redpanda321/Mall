using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mall.Entities;

namespace Mall.Service
{
    public class WXSmallProgramService : ServiceBase, IWXSmallProgramService
    {
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="mWXSmallChoiceProductsInfo"></param>
        public void AddWXSmallProducts(WXSmallChoiceProductInfo mWXSmallChoiceProductsInfo)
        {
            DbFactory.Default.Add(mWXSmallChoiceProductsInfo);
        }

        /// <summary>
        /// 获取所有的商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<Entities.ProductInfo> GetWXSmallProducts(int page, int rows, ProductQuery productQuery)
        {
            var model = new QueryPageModel<ProductInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder countsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();
            countsb.Append(" select count(1) from Mall_Product pt left join Mall_WXSmallChoiceProduct ps on pt.Id=ps.ProductId left join Mall_Shop s on pt.ShopId=s.Id ");
            sqlsb.Append(" select pt.*,s.ShopName from Mall_Product pt left join Mall_WXSmallChoiceProduct ps on pt.Id=ps.ProductId ");
            sqlsb.Append(" left join Mall_Shop s on pt.ShopId=s.Id ");
            wheresb.Append(" where pt.IsDeleted=FALSE and ps.ProductId>0 ");
            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
                wheresb.AppendFormat(" and pt.ProductName like '%{0}%' ", productQuery.KeyWords);
            if (!string.IsNullOrWhiteSpace(productQuery.ShopName))
                wheresb.AppendFormat(" and s.ShopName like '%{0}%' ", productQuery.ShopName);
            wheresb.Append(" order by ps.ProductId ");
            var start = (page - 1) * rows;
            var end = page * rows;
            countsb.Append(wheresb);
            sqlsb.Append(wheresb);
            sqlsb.Append(" limit " + start + "," + rows);
            var list = DbFactory.Default.Query<ProductInfo>(sqlsb.ToString()).ToList();
            //var list = Context.Database.SqlQuery<Entities.ProductInfo>(sqlsb.ToString()).ToList();
            //var shops = Context.ShopInfo;
            var products = list.ToArray().Select(item =>
            {
                var shop = DbFactory.Default.Get<ShopInfo>().Where(s => s.Id == item.ShopId).FirstOrDefault();
                if (shop != null)
                    item.ShopName = shop.ShopName;
                return item;
            });
            model.Models = products.ToList();
            var count = 0;
            count = DbFactory.Default.Query<int>(countsb.ToString()).FirstOrDefault();
            model.Total = count;
            return model;
        }

        /// <summary>
        /// 获取商品
        /// </summary>
        /// <returns></returns>
        public List<WXSmallChoiceProductInfo> GetWXSmallProducts()
        {
            return DbFactory.Default.Get<WXSmallChoiceProductInfo>().ToList();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        public void DeleteWXSmallProductById(long Id)
        {
            DbFactory.Default.Del<WXSmallChoiceProductInfo>(item => item.ProductId == Id);
        }
      
        public QueryPageModel<ProductInfo> GetWXSmallHomeProducts(int pageNo, int pageSize)
        {
            var data = DbFactory.Default.Get<ProductInfo>().Where(p => p.IsDeleted == false).InnerJoin<Entities.WXSmallChoiceProductInfo>((mp, p) => mp.Id == p.ProductId && p.ProductId > 0).ToPagedList(pageNo, pageSize);
            return new QueryPageModel<ProductInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        public void DeleteWXSmallProductByIds(List<long> ids)
        {
            DbFactory.Default.Del<WXSmallChoiceProductInfo>(item => item.ProductId.ExIn(ids));

        }
    }
}
