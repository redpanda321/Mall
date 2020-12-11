using Mall.CommonModel;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mall.Service
{
    public class FullDiscountService : ServiceBase, IFullDiscountService
    {
        /// <summary>
        /// 当前活动类型
        /// </summary>
        private MarketActiveType CurrentActiveType = MarketActiveType.FullDiscount;

        #region 满减活动操作
        /// <summary>
        /// 新增满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public void AddActive(ActiveInfo data, IEnumerable<FullDiscountRuleInfo> rules, IEnumerable<ActiveProductInfo> products)
        {
            DbFactory.Default.InTransaction(() =>
            {
                data.ActiveType = CurrentActiveType;
                DbFactory.Default.Add(data);

                foreach (var item in rules)
                {
                    item.ActiveId = data.Id;
                }
                foreach (var item in products)
                {
                    item.ActiveId = data.Id;
                }
                DbFactory.Default.Add(rules);
                DbFactory.Default.Add(products);
            });
        }
        /// <summary>
        /// 更新满减活动
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rules"></param>
        /// <param name="products"></param>
        public void UpdateActive(ActiveInfo data, IEnumerable<FullDiscountRuleInfo> rules, IEnumerable<ActiveProductInfo> products)
        {
            DbFactory.Default.InTransaction(() =>
            {
                var obj = DbFactory.Default.Get<ActiveInfo>().Where(d => d.Id == data.Id).FirstOrDefault();
                obj.ShopId = data.ShopId;
                obj.ActiveName = data.ActiveName;
                obj.StartTime = data.StartTime;
                obj.EndTime = data.EndTime;
                obj.IsAllProduct = data.IsAllProduct;
                DbFactory.Default.Del<FullDiscountRuleInfo>().Where(d => d.ActiveId == data.Id).Succeed();
                foreach (var item in rules)
                {
                    item.ActiveId = data.Id;
                }
                DbFactory.Default.Add(rules);
                DbFactory.Default.Del<ActiveProductInfo>().Where(d => d.ActiveId == data.Id).Succeed();
                foreach (var item in products)
                {
                    item.ActiveId = data.Id;
                }
                DbFactory.Default.Add(products);
                DbFactory.Default.Update(obj);
            });
        }
        /// <summary>
        /// 删除满减活动
        /// </summary>
        /// <param name="id"></param>
        public void DeleteActive(long id)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<ActiveProductInfo>().Where(n => n.ActiveId == id).Succeed();
                DbFactory.Default.Del<FullDiscountRuleInfo>().Where(n => n.ActiveId == id).Succeed();
                DbFactory.Default.Del<ActiveInfo>().Where(n => n.ActiveType == CurrentActiveType && n.Id == id).Succeed();
            });
        }
        #endregion

        #region 满减活动查询
        /// <summary>
        /// 商品是否可以参加满减活动
        /// <para>不判断商品的销售状态</para>
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <returns></returns>
        public bool ProductCanJoinActive(long productId, long activeId)
        {
            var actsql = DbFactory.Default.Get<ActiveInfo>().Where(a => a.ActiveType == CurrentActiveType && a.EndTime > DateTime.Now);
            if (activeId > 0)
            {
                actsql.Where(d => d.Id != activeId);
            }
            var activeIds = actsql.Select(a => a.Id);
            var actProducts = DbFactory.Default.Get<ActiveProductInfo>().Where(a => (a.ProductId == productId || a.ProductId == -1) && a.ActiveId.ExIn(activeIds));
            return actProducts.Exist();
        }
        /// <summary>
        /// 过滤活动商品编号
        /// <para>返回可以参加活动的商品</para>
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeId">添加活动使用0</param>
        /// <param name="shopId">店铺编号</param>
        /// <returns></returns>
        public List<long> FilterActiveProductId(IEnumerable<long> productIds, long activeId, long shopId)
        {
            var actsql = DbFactory.Default.Get<ActiveInfo>().Where(a => a.ActiveType == CurrentActiveType && a.EndTime > DateTime.Now && a.ShopId == shopId);

            if (activeId > 0)
            {
                actsql.Where(d => d.Id != activeId);
            }
            var activeIds = actsql.Select(a => a.Id);
            var actProductIds = DbFactory.Default.Get<ActiveProductInfo>().Where(a => a.ActiveId.ExIn(activeIds)).Select(d => d.ProductId).ToList<long>();

            List<long> result = new List<long>();
            if (!actProductIds.Any(d => d == -1))
            {
                //过滤活动中的商品
                //actProductIds = Context.ProductInfo.Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                //&& d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                // && d.IsDeleted == false
                //&& actProductIds.Contains(d.Id)).Select(d => d.Id).ToList();
                //过滤非销售中的商品
                var okproductIds = DbFactory.Default.Get<ProductInfo>().Where(d => d.AuditStatus == ProductInfo.ProductAuditStatus.Audited
                  && d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                  && d.IsDeleted == false
                  && d.Id.ExIn(productIds)).Select(d => d.Id).ToList<long>();
                result = productIds.Where(d => !actProductIds.Contains(d)).Where(d => okproductIds.Contains(d)).ToList();
            }
            return result;
        }
        /// <summary>
        /// 根据商品ID取正在参与且进行中的活动信息
        /// </summary>
        /// <param name="proId"></param>
        /// <returns></returns>
        public ActiveInfo GetOngoingActiveByProductId(long proId, long shopId)
        {
            var goingActives = DbFactory.Default
                .Get<ActiveInfo>()
                .Where(a => a.ActiveType == CurrentActiveType && a.ShopId == shopId &&
                    a.StartTime <= DateTime.Now && a.EndTime > DateTime.Now)
                .ToList();
            var activeIds = goingActives.Select(a => a.Id).ToList();
            var productActiveId = DbFactory.Default
                .Get<ActiveProductInfo>()
                .Where(a => (a.ProductId == proId || a.ProductId == -1) && a.ActiveId.ExIn(activeIds))
                .Select(a => a.ActiveId)
                .FirstOrDefault<long>();
            var result = goingActives.Where(a => a.Id == productActiveId).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 获取某个店铺正在进行的满额减活动列表
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public List<Entities.ActiveInfo> GetOngoingActiveByShopId(long shopId)
        {
            var goingActives = DbFactory.Default.Get<Entities.ActiveInfo>().Where(a => a.ActiveType == CurrentActiveType && a.ShopId == shopId && a.StartTime <= DateTime.Now && a.EndTime > DateTime.Now).ToList();
            //  var activeIds = goingActives.Select(a => a.Id);
            return goingActives;
        }
        /// <summary>
        /// 根据正在进行的活动ID和商品ID获取满额减活动分组商品
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="activeIds"></param>
        /// <returns></returns>
        private IEnumerable<IGrouping<long, Entities.ActiveProductInfo>> GetActiveProductGroup(IEnumerable<long> productIds, IEnumerable<long> activeIds)
        {
            var productActives = DbFactory.Default.Get<Entities.ActiveProductInfo>().Where(a => (a.ProductId.ExIn(productIds) || a.ProductId == -1) && a.ActiveId.ExIn(activeIds)).ToList().GroupBy(a => a.ActiveId);

            return productActives;
        }

        /// <summary>
        /// 获取某个店铺的一批商品正在进行的满额减活动
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<Entities.ActiveInfo> GetOngoingActiveByProductIds(IEnumerable<long> productIds, long shopId)
        {
            var actives = GetOngoingActiveByShopId(shopId);
            if (actives == null || actives.Count == 0)
                return new List<Entities.ActiveInfo>();
            var activeIds = actives.Select(a => a.Id);
            var activeGroup = GetActiveProductGroup(productIds, activeIds);
            if (activeGroup == null || activeGroup.Count() == 0)
                return new List<Entities.ActiveInfo>();
            var ids = activeGroup.Select(a => a.Key);
            var onGoingActives = actives.Where(a => ids.Contains(a.Id)).ToList();
            foreach (var a in onGoingActives)
            {
                var p = activeGroup.Where(x => x.Key == a.Id).FirstOrDefault().Select(y => y).ToList();
                a.Products = p;
                a.Rules = GetActiveRules(a.Id);
            }
            return onGoingActives;
        }



        /// <summary>
        /// 获取满减活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActiveInfo GetActive(long id)
        {
            return DbFactory.Default.Get<ActiveInfo>().Where(d => d.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// 获取满减优惠阶梯
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<FullDiscountRuleInfo> GetActiveRules(long activeId)
        {
            return DbFactory.Default.Get<FullDiscountRuleInfo>().Where(d => d.ActiveId == activeId).ToList();
        }
        /// <summary>
        /// 获取满减商品
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<ActiveProductInfo> GetActiveProducts(long activeId)
        {
            var result = DbFactory.Default.Get<ActiveProductInfo>().Where(d => d.ActiveId == activeId).ToList();
            return result;
        }
        /// <summary>
        /// 获取活动列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ActiveInfo> GetActives(FullDiscountActiveQuery query)
        {
            QueryPageModel<ActiveInfo> result = new QueryPageModel<ActiveInfo>();
            var curtime = DateTime.Now;

            if (query.Status != null)  //有状态不过UNION all
            {
                #region 普通排序走ef
                var sql = DbFactory.Default.Get<ActiveInfo>();
                sql.Where(d => d.ActiveType == CurrentActiveType);
                if (query.StartTime.HasValue)
                {
                    var startTime = query.StartTime.Value.Date;
                    sql.Where(d => d.StartTime >= startTime);
                }
                if (query.EndTime.HasValue)
                {
                    DateTime EndTime = query.EndTime.Value.AddDays(1).Date;
                    sql.Where(d => d.StartTime <= EndTime);
                }
                if (query.ShopId.HasValue)
                {
                    sql.Where(d => d.ShopId == query.ShopId.Value);
                }
                if (!string.IsNullOrWhiteSpace(query.ActiveName))
                {
                    sql.Where(d => d.ActiveName.Contains(query.ActiveName));
                }
                switch (query.Status)
                {
                    case FullDiscountStatus.Ending:
                        sql.Where(d => d.EndTime < curtime);
                        break;
                    case FullDiscountStatus.Ongoing:
                        sql.Where(d => d.StartTime <= curtime && d.EndTime >= curtime);
                        break;
                    case FullDiscountStatus.WillStart:
                        sql.Where(d => d.StartTime > curtime && d.EndTime > d.StartTime);
                        break;
                }

                sql.OrderByDescending(o => o.StartTime);
                var datalist = sql.ToPagedList(query.PageNo, query.PageSize);
                result.Models = datalist;
                result.Total = datalist.TotalRecordCount;

                #endregion
            }
            else
            {
                var sqlbasestring = new Sql();
                sqlbasestring.Append(@"select * from (
                        SELECT *,1 as ordernum from Mall_Active where EndTime<NOW() {where}
                        UNION ALL
                        SELECT *,2 as ordernum from Mall_Active where StartTime>NOW() {where}
                        UNION ALL
                        SELECT *,3 as ordernum from Mall_Active where StartTime<=NOW() and EndTime>=now() {where}
                        ) as t");
                string wherestring = " and ActiveType=" + CurrentActiveType.GetHashCode().ToString() + " ";
                if (query.ShopId.HasValue)
                {
                    wherestring += " and ShopId=" + query.ShopId.ToString() + " ";
                }
                if (!string.IsNullOrWhiteSpace(query.ActiveName))
                {
                    wherestring += " and ActiveName like '%" + query.ActiveName + "%' ";
                }
                if (query.StartTime.HasValue)
                {
                    wherestring += " and StartTime >= '" + query.StartTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                if (query.EndTime.HasValue)
                {
                    DateTime EndTime = query.EndTime.Value.AddDays(1).Date;
                    wherestring += " and StartTime < '" + EndTime.ToString("yyyy-MM-dd HH:mm:ss") + "' ";
                }
                var sql = sqlbasestring.ToString().Replace("{where}", wherestring);
                string countsqlstring = @"select count(1) from (" + sql + ") as t";
                int pstart = (query.PageNo - 1) * query.PageSize;
                string sqlstring = sql + " ORDER BY ordernum DESC, StartTime desc limit " + pstart + "," + query.PageSize;
                var total = DbFactory.Default.Query<int>(countsqlstring).FirstOrDefault();
                result.Total = total;
                List<ActiveInfo> datas = DbFactory.Default.Query<ActiveInfo>(sqlstring).ToList();
                result.Models = datas;
            }
            return result;
        }
        /// <summary>
        /// 获取活动商品数量聚合
        /// </summary>
        /// <param name="activeId"></param>
        /// <returns></returns>
        public List<FullDiscountProductCountAggregate> GetActivesProductCountAggregate(IEnumerable<long> activeId)
        {
            var sql = DbFactory.Default
                .Get<ActiveProductInfo>()
                .InnerJoin<ProductInfo>((api, pi) => api.ProductId == pi.Id)
                .Where(d => d.ActiveId.ExIn(activeId))
                .Where<ProductInfo>(p => p.IsDeleted == false)
                .GroupBy(d => d.ActiveId)
                .Select(n => new
                {
                    ActiveId = n.ActiveId,
                    ProductCount = n.Id.ExCount(false),
                });
            var result = sql.ToList<FullDiscountProductCountAggregate>();
            return result;
        }
        #endregion

        #region 其他功能
        /// <summary>
        /// 获取可以参与满减活动的商品集
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="productName"></param>
        /// <param name="productCode"></param>
        /// <param name="categroyId"></param>
        /// <param name="selectedProductIds"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public QueryPageModel<ProductInfo> GetCanJoinProducts(long shopId
            , string productName = null, string productCode = null, long? categoryId = null, bool? isShopCategory = null
            , IEnumerable<long> selectedProductIds = null, int activeId = 0
            , int page = 1, int pagesize = 10)
        {
            QueryPageModel<ProductInfo> result = new QueryPageModel<ProductInfo>();
            //获取活动
            DateTime curtime = DateTime.Now;
            var actsql = DbFactory.Default.Get<ActiveInfo>().Where(d => d.EndTime >= curtime && d.ShopId == shopId);
            if (activeId > 0)
            {
                actsql.Where(d => d.Id != activeId);
            }
            var actids = actsql.Select(d => d.Id);
            var actproids = DbFactory.Default.Get<ActiveProductInfo>().Where(d => d.ActiveId.ExIn(actids)).Select(d => d.ProductId).ToList<long>();
            if (!actproids.Any(d => d == -1))
            {
                var prosql = DbFactory.Default
                    .Get<ProductInfo>().Where(d => d.ShopId == shopId && d.AuditStatus == ProductInfo.ProductAuditStatus.Audited &&
                        d.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && d.IsDeleted == false);
                prosql.Where(d => d.Id.ExNotIn(actproids));
                var skuinfosql = DbFactory.Default
                    .Get<SKUInfo>()
                    .Where<SKUInfo, ProductInfo>((si, pi) => si.ProductId == pi.Id && si.Stock > 0);
                prosql.Where(p => p.ExExists(skuinfosql));
                if (!string.IsNullOrWhiteSpace(productName))
                {
                    prosql.Where(d => d.ProductName.Contains(productName));
                }
                if (!string.IsNullOrWhiteSpace(productCode))
                {
                    prosql.Where(d => d.ProductCode.Contains(productCode));
                }
                if (categoryId.HasValue && categoryId > 0)
                {
                    var _cateid = categoryId.Value;
                    if (isShopCategory == true)
                    {
                        var shopCategoryIds = DbFactory.Default
                            .Get<ShopCategoryInfo>()
                            .Where(p => p.ShopId == shopId && (p.Id == _cateid || p.ParentCategoryId == _cateid))
                            .Select(p => p.Id);

                        var productIds = DbFactory.Default
                            .Get<ProductShopCategoryInfo>()
                            .Where(p => p.ShopCategoryId.ExIn(shopCategoryIds))
                            .Select(p => p.ProductId);

                        prosql.Where(p => p.Id.ExIn(productIds));
                    }
                    else
                    {
                        prosql.Where(item => ("|" + item.CategoryPath + "|").Contains("|" + _cateid + "|"));
                    }
                }
                if (selectedProductIds != null && selectedProductIds.Count() > 0)
                {
                    prosql.Where(d => d.Id.ExNotIn(selectedProductIds));
                }
                var datas = prosql.OrderByDescending(o => o.Id).ToPagedList(page, pagesize);
                result.Total = datas.TotalRecordCount;
                result.Models = datas;
            }
            return result;
        }
        /// <summary>
        /// 是否可以操作(添加/修改)活动
        /// </summary>
        /// <param name="active"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        public bool CanOperationActive(ActiveInfo active, IEnumerable<ActiveProductInfo> products)
        {
            bool result = false;
            DateTime now = DateTime.Now;
            var actsql = DbFactory.Default.Get<ActiveInfo>().Where(d => d.ActiveType == CurrentActiveType && d.EndTime > now && d.ShopId == active.ShopId);
            if (active.Id > 0)
            {
                actsql.Where(d => d.Id != active.Id);
            }
            var act = actsql.ToList();
            if (active.IsAllProduct && act.Count > 0)
            {
                return false;
            }
            if (!act.Any(d => d.IsAllProduct))
            {
                var actids = act.Select(d => d.Id);
                var proids = DbFactory.Default.Get<ActiveProductInfo>().Where(d => d.ActiveId.ExIn(actids)).Select(d => d.ProductId).ToList<long>();
                result = true;
                foreach (var item in products)
                {
                    if (proids.Contains(item.ProductId))
                    {
                        result = false;
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
