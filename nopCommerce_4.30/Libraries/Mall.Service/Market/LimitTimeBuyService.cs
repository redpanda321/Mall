using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel.WeiXin;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Service
{
    /// <summary>
    /// 限时购服务实现
    /// </summary>
    public class LimitTimeBuyService : ServiceBase, ILimitTimeBuyService
    {
        #region 平台

        /// <summary>
        /// 设置活动参数
        /// </summary>
        public void UpdateConfig(FlashSaleConfigModel data)
        {
            var model = DbFactory.Default.Get<FlashSaleConfigInfo>().FirstOrDefault();
            if (model == null)
            {
                model = new FlashSaleConfigInfo();
                model.IsNormalPurchase = data.IsNormalPurchase;
                model.Preheat = data.Preheat;
                DbFactory.Default.Add(model);
            }
            else
            {
                model.IsNormalPurchase = data.IsNormalPurchase;
                model.Preheat = data.Preheat;
                DbFactory.Default.Update(model);
            }
            string cacheKey = CacheKeyCollection.CACH_FLASHSALECONFIG;
            Cache.Remove(cacheKey);
        }

        public void AddFlashSaleDetails(List<FlashSaleDetailInfo> list)
        {
            DbFactory.Default.Add<FlashSaleDetailInfo>(list);
        }


        /// <summary>
        /// 删除
        /// </summary>
        public void Delete(long id, long shopId)
        {
            var m = DbFactory.Default.Get<FlashSaleInfo>().Where(a => a.Id == id && a.ShopId == shopId).FirstOrDefault();
            if (m != null)
            {
                DbFactory.Default.InTransaction(() =>
                {
                    DbFactory.Default.Del<FlashSaleDetailInfo>().Where(n => n.FlashSaleId == m.Id).Succeed();
                    DbFactory.Default.Del<FlashSaleInfo>(m);
                });
                //var list = Context.FlashSaleDetailInfo.Where(a => a.FlashSaleId == m.Id).ToList();
                //Context.FlashSaleDetailInfo.RemoveRange(list);
                //Context.SaveChanges();
            }
        }

        public FlashSaleConfigModel GetConfig()
        {
            string cacheKey = CacheKeyCollection.CACH_FLASHSALECONFIG;
            if (Cache.Exists(cacheKey))
                return Cache.Get<FlashSaleConfigModel>(cacheKey);

            var model = DbFactory.Default.Get<FlashSaleConfigInfo>().FirstOrDefault();
            if (model == null)
            {
                model = new FlashSaleConfigInfo();
                model.IsNormalPurchase = true;
                model.Preheat = 24;
                DbFactory.Default.Add(model);
            }
            var result = new FlashSaleConfigModel(model.Preheat, model.IsNormalPurchase);
            Cache.Insert<FlashSaleConfigModel>(cacheKey, result, 600);
            return result;
        }

     

        /// <summary>
        /// 更新限时购服务设置
        /// </summary>
        public void UpdateServiceSetting(LimitTimeBuySettingModel model)
        {
            var market = DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();
            if (market != null && market.Id != 0)
            {
                market.Price = model.Price;
                DbFactory.Default.Update(market);
            }
            else
            {
                MarketSettingInfo info = new MarketSettingInfo()
                {
                    Price = model.Price,
                    TypeId = MarketType.LimitTimeBuy
                };
                DbFactory.Default.Add(info);
            }
        }

        /// <summary>
        /// 获取限时购服务设置
        /// </summary>
        /// <returns></returns>
        public LimitTimeBuySettingModel GetServiceSetting()
        {
            int days = 0;
            var result = DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();
            if (result == null || result.Id == 0)
                return null;

            var model = new LimitTimeBuySettingModel() { Price = result.Price, ReviceDays = 0 };

            var setting = DbFactory.Default.Get<MarketSettingMetaInfo>(p => p.MarketId == result.Id).ToList();
            if (setting.Count != 0)
            {
                var marketMeta = setting.FirstOrDefault(m => m.MetaKey.ToLower() == "revicedays");
                if (marketMeta != null)
                {
                    var ReviceDays = marketMeta.MetaValue;
                    if (int.TryParse(ReviceDays, out days)) { }
                }
                model.ReviceDays = days;
            }
            return model;
        }

        /// <summary>
        /// 更新限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        public void AddServiceCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new MallException("分类不能为空，添加失败.");
            }
            var market = DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();
            var settings = DbFactory.Default.Get<MarketSettingMetaInfo>(p => p.MarketId == market.Id).ToList();
            if (market != null && market.Id != 0)
            {

                var meta = settings.FirstOrDefault(m => m.MarketId == market.Id && m.MetaKey.ToLower() == "categories");
                if (meta == null || meta.Id == 0)
                {
                    DbFactory.Default.Add(new MarketSettingMetaInfo
                    {
                        MetaKey = "Categories",
                        MetaValue = categoryName,
                        MarketId = market.Id
                    });
                }
                else
                {
                    if (meta.MetaValue.Split(',').Any(c => c.Equals(categoryName)))
                    {
                        throw new MallException("添加的限时购分类已经存在，添加失败.");
                    }
                    else
                    {
                        meta.MetaValue += string.Format(",{0}", categoryName);
                        DbFactory.Default.Update(meta);
                    }

                }
            }
        }

        /// <summary>
        /// 删除一个限时购服务分类
        /// </summary>
        /// <param name="categoryId"></param>
        public void DeleteServiceCategory(string categoryName)
        {
            //if( string.IsNullOrWhiteSpace( categoryName ) )
            //{
            //    throw new MallException( "分类不能为空，添加失败." );
            //}
            var market = DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();

            if (DbFactory.Default.Get<FlashSaleInfo>().Where(m =>
                m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate < DateTime.Now && m.EndDate > DateTime.Now
                || m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && m.BeginDate < DateTime.Now
                || m.CategoryName.Equals(categoryName) && m.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing && m.EndDate > DateTime.Now).Exist())
            {
                throw new MallException("该分类不能被删除，有未审核或者其他正在进行中的限时购.");
            }

            if (market != null && market.Id != 0)
            {
                var settings = DbFactory.Default.Get<MarketSettingMetaInfo>(p => p.MarketId == market.Id).ToList();
                var cate = settings.FirstOrDefault(m => m.MetaKey.ToLower().Equals("categories"));
                if (cate != null && cate.Id != 0)
                {
                    var cateList = cate.MetaValue.Split(',').ToList();
                    if (cateList.Count == 1)
                    {
                        throw new MallException("分类不能少于一条.");
                    }
                    cateList.Remove(categoryName);
                    cate.MetaValue = string.Join(",", cateList);
                    DbFactory.Default.Update(cate);
                }
            }
        }

        /// <summary>
        /// 获取限时购服务分类
        /// </summary>
        /// <returns></returns>
        public string[] GetServiceCategories()
        {
            var result = new List<string>();
            var market = DbFactory.Default.Get<MarketSettingInfo>().Where(m => m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();
            if (market != null && market.Id != 0)
            {
                var settings = DbFactory.Default.Get<MarketSettingMetaInfo>(p => p.MarketId == market.Id).ToList();
                var cate = settings.FirstOrDefault(m => m.MetaKey.ToLower().Equals("categories"));
                if (cate != null && cate.Id != 0)
                {
                    result = cate.MetaValue.Split(',').ToList();
                }
            }
            return result.ToArray();
        }

        #endregion

        #region 商家

        /// <summary>
        /// 根据店铺Id获取该店铺购买的限时购营销服务信息
        /// </summary>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public ActiveMarketServiceInfo GetMarketService(long shopId)
        {
            if (shopId <= 0)
            {
                throw new MallException("ShopId不能识别");
            }
            var market = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(m => m.ShopId == shopId && m.TypeId == MarketType.LimitTimeBuy).FirstOrDefault();
            return market;
        }

        #endregion

        #region 前台

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsLimitTimeMarketItem(long id)
        {
            if (id <= 0)
            {
                return false;
            }

            var item = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.ProductId == id
                && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing
                && p.BeginDate <= DateTime.Now
                && p.EndDate > DateTime.Now).Exist();
            return item;
        }

        /// <summary>
        /// 根据商品Id获取一个限时购的详细信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public FlashSaleInfo GetLimitTimeMarketItemByProductId(long pid)
        {
            if (pid <= 0)
            {
                throw new MallException("商品Id不能识别");
            }
            FlashSaleInfo result = null;
            var now = DateTime.Now;
            //var sql = new Sql("SELECT * FROM Mall_flashsale WHERE ProductId=@0 AND Status=@1 AND BeginDate<=@2 AND EndDate > @3", pid, FlashSaleInfo.FlashSaleStatus.Ongoing.GetHashCode(), now, now);
            result = DbFactory.Default.Get<FlashSaleInfo>().Where(e => e.ProductId == pid && e.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && e.BeginDate <= now && e.EndDate > now).FirstOrDefault();
            //在一个事务中，不能使用多个连接
            //result = DbFactory.Default.FirstOrDefault<FlashSaleInfo>(sql);//, new { ProductId = pid, Status = FlashSaleInfo.FlashSaleStatus.Ongoing, BeginDate = now, EndDate = now });

            return result;
        }

        public FlashSaleInfo GetFlashSaleInfoByProductIdAndShopId(long productId, long shopId)
        {
            var now = DateTime.Now;
            return DbFactory.Default.Get<FlashSaleInfo>().Where(e => e.ProductId == productId && e.ShopId == shopId && e.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && e.EndDate > now).FirstOrDefault();
        }

        #endregion


        #region 新版本限时购

        public QueryPageModel<FlashSaleInfo> GetAll(FlashSaleQuery query)
        {
            ExpiredDataChangeStatus();
            var db = DbFactory.Default.Get<FlashSaleInfo>()
                .LeftJoin<ProductInfo>((fsi, pi) => fsi.ProductId == pi.Id)
                .LeftJoin<ShopInfo>((fsi, si) => fsi.ShopId == si.Id);

            var config = GetConfig();

            if (query.IsStart == 1)//已开始
            {
                db.Where(p => p.BeginDate < DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing);

            }
            else if (query.IsStart == 2)  //即将开始 
            {
                var date = DateTime.Now.AddHours(config.Preheat);
                db.Where(p => p.BeginDate < date && p.BeginDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing);
            }
            else
            {
                if (query.IsPreheat)
                {
                    //if (config.Preheat > 0)
                    //{
                        var date = DateTime.Now.AddHours(config.Preheat);
                        db.Where(p => p.BeginDate < date);
                    //}
                }
            }

            if (query.OrderType != 0)
            {
                db.Where(item => item.EndDate > DateTime.Now);
            }
            if (query.ShopId.HasValue)
            {
                db.Where(item => query.ShopId == item.ShopId);
            }
            if (!string.IsNullOrWhiteSpace(query.ItemName))
            {
                db.Where<ProductInfo>(item => item.ProductName.Contains(query.ItemName));
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                db.Where<ShopInfo>(item => item.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                db.Where(d => d.CategoryName == query.CategoryName);
            }
            var Date = DateTime.Now;
            if (query.AuditStatus.HasValue)
            {
                var status = query.AuditStatus;
                FlashSaleInfo.FlashSaleStatus s = (FlashSaleInfo.FlashSaleStatus)status;
                db.Where(p => p.Status == s);
            }
            //db.Where<ProductInfo>(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.IsDeleted == false);
            if (query.CheckProductStatus) db.Where<ProductInfo>(p => p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale);


            switch (query.OrderKey)
            {
                case 2:
                    if (query.OrderType == 1)
                    {
                        db.OrderBy(item => item.SaleCount);
                    }
                    else
                    {
                        db.OrderByDescending(item => item.SaleCount);
                    }
                    break;
                //case 3:
                //    if( query.OrderType == 2 )
                //        orderby = limit.GetOrderBy( d => d.OrderBy( item => item.Price ) );
                //    else
                //        orderby = limit.GetOrderBy( d => d.OrderByDescending( item => item.Price ) ); 
                //    break;
                case 4:
                    if (query.OrderType == 1)
                    {
                        db.OrderBy(item => item.EndDate);
                    }
                    else
                    {
                        db.OrderByDescending(item => item.EndDate);
                    }
                    break;
                case 5:
                    if (query.OrderType == 1)
                    {
                        db.OrderBy(item => item.BeginDate);
                    }
                    else
                    {
                        db.OrderByDescending(item => item.BeginDate);
                    }
                    break;
                default:
                    db.OrderByDescending(item => item.Id);
                    break;
            }
            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<FlashSaleInfo>() { Models = data, Total = data.TotalRecordCount };
        }

   

        public QueryPageModel<FlashSaleInfo> GetFlashSaleInfos(LimitTimeQuery query)
        {
            ExpiredDataChangeStatus();
            var db = WhereBuilder(query);

            switch (query.Sort.ToLower())
            {
                case "begindate":
                    if (query.IsAsc) db.OrderBy(p => p.BeginDate);
                    else db.OrderByDescending(p => p.BeginDate);
                    break;
                case "enddate":
                    if (query.IsAsc) db.OrderBy(p => p.EndDate);
                    else db.OrderByDescending(p => p.EndDate);
                    break;
                case "salecount":
                    if (query.IsAsc) db.OrderBy(p => p.SaleCount);
                    else db.OrderByDescending(p => p.SaleCount);
                    break;
                case "limitcountofthepeople":
                    if (query.IsAsc) db.OrderBy(p => p.LimitCountOfThePeople);
                    else db.OrderByDescending(p => p.LimitCountOfThePeople);
                    break;
                default:
                    db.OrderByDescending(o => o.BeginDate);
                    break;
            }

            var datas = db.Select().ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<FlashSaleInfo>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
        }

        public int GetFlashSaleCount(LimitTimeQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        private GetBuilder<FlashSaleInfo> WhereBuilder(LimitTimeQuery query)
        {
            var db = DbFactory.Default
               .Get<FlashSaleInfo>()
               .InnerJoin<ShopInfo>((fsi, si) => fsi.ShopId == si.Id)
               .InnerJoin<ProductInfo>((fsi, pi) => fsi.ProductId == pi.Id);
            if (!string.IsNullOrEmpty(query.ShopName))
                db.Where<ShopInfo>(p => p.ShopName.Contains(query.ShopName));

            if (!string.IsNullOrWhiteSpace(query.ProductName))
                db.Where<ProductInfo>(p => p.ProductName.Contains(query.ProductName));

            if (!string.IsNullOrEmpty(query.Title))
                db.Where(p => p.Title.Contains(query.Title));

            if (query.StartDate.HasValue)
                db.Where(a => a.BeginDate >= query.StartDate);

            if (query.EndDate.HasValue)
            {
                query.EndDate = query.EndDate.Value.AddDays(1);
                db.Where(a => a.BeginDate <= query.EndDate);
            }
            if (query.ShopId.HasValue)
                db.Where(a => a.ShopId == query.ShopId.Value);

            if (query.Status.HasValue)
            {
                var status = (FlashSaleInfo.FlashSaleStatus)query.Status;
                if (status == FlashSaleInfo.FlashSaleStatus.Ended)
                    db.Where(p => p.EndDate < DateTime.Now && p.Status != FlashSaleInfo.FlashSaleStatus.Cancelled);
                else if (status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                    db.Where(p => p.Status == status);
                else if (status == FlashSaleInfo.FlashSaleStatus.Ongoing)
                    db.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now);
                else if (status == FlashSaleInfo.FlashSaleStatus.AuditFailed)
                    db.Where(a => a.Status == status);
                else if (status == FlashSaleInfo.FlashSaleStatus.Cancelled)
                    db.Where(a => a.Status == status);
                else if (status == FlashSaleInfo.FlashSaleStatus.NotBegin)
                    db.Where(p => p.Status != FlashSaleInfo.FlashSaleStatus.WaitForAuditing && p.Status != FlashSaleInfo.FlashSaleStatus.AuditFailed && p.BeginDate > DateTime.Now);
            }


            return db;
        }



        private void ExpiredDataChangeStatus()
        {
            //TOGO:FG 非合理任务执行方式
            DbFactory.Default
                .Set<FlashSaleInfo>()
                .Set(n => n.Status, FlashSaleInfo.FlashSaleStatus.Ended)
                .Where(n => n.EndDate <= DateTime.Now && n.Status != FlashSaleInfo.FlashSaleStatus.Cancelled)
                .Succeed();
        }

        public FlashSaleModel GetDetailInfo(long productId)
        {
            var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == productId).FirstOrDefault();
            var fs = new FlashSaleModel();
            fs.ProductId = productId;
            fs.ProductImg = product.ImagePath;
            List<FlashSaleDetailModel> list = new List<FlashSaleDetailModel>();
            var result = DbFactory.Default.Get<SKUInfo>().Where(p => p.ProductId == productId).ToList();
            #region 阶梯商品价格--ZYF
            var price = GetMinLadderPrice(productId);
            #endregion
            foreach (var item in result)
            {
                FlashSaleDetailModel mdoel = new FlashSaleDetailModel();
                mdoel.SkuId = item.Id;
                mdoel.Color = item.Color;
                mdoel.Size = item.Size;
                mdoel.Version = item.Version;
                mdoel.Stock = (int)item.Stock;

                mdoel.SalePrice = price > 0 ? price : item.SalePrice;
                mdoel.CostPrice = item.CostPrice;
                list.Add(mdoel);
            }
            fs.Details = list;
            return fs;
        }

        public FlashSaleDetailInfo GetDetail(string skuid)
        {
            //return context.FlashSaleDetailInfo.Include("").FirstOrDefault( p => p.SkuId == skuid);

            var x = DbFactory.Default.Get<FlashSaleDetailInfo>().InnerJoin<FlashSaleInfo>((fsdi, fsi) => fsdi.FlashSaleId == fsi.Id)
                .Where(p => p.SkuId == skuid)
                .Where<FlashSaleInfo>(p => p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now)
                .Select()
                .FirstOrDefault();

            return x;
        }

        public FlashSaleModel GetFlaseSaleByProductId(long pid)
        {
            if (pid <= 0)
            {
                throw new MallException("商品Id不能识别");
            }

            var model = DbFactory.Default
                .Get<FlashSaleInfo>()
                .Where(m => m.ProductId == pid && m.Status == FlashSaleInfo.FlashSaleStatus.Ongoing &&
                    m.BeginDate <= DateTime.Now && m.EndDate > DateTime.Now)
                .FirstOrDefault();
            FlashSaleModel result = new FlashSaleModel();
            if (model != null)
            {
                var product = ProductManagerApplication.GetProduct(model.ProductId);
                result.Id = model.Id;
                result.Title = model.Title;
                result.ShopId = model.ShopId;
                result.ProductId = model.ProductId;
                result.Status = model.Status;
                result.ProductName = product.ProductName;
                result.ProductImg = product.RelativePath;
                result.StatusStr = model.Status.ToDescription();
                result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
                result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
                result.LimitCountOfThePeople = model.LimitCountOfThePeople;
                result.SaleCount = model.SaleCount;
                result.CategoryName = model.CategoryName;
                result.MinPrice = model.MinPrice;
                result.Details = new List<FlashSaleDetailModel>();
                return result;
            }
            else
                return null;
        }

        public FlashSaleModel Get(long id)
        {
            var model = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (model == null)
            {
                throw new MallException("活动不存在!");
            }
            var product = ProductManagerApplication.GetProduct(model.ProductId);
            FlashSaleModel result = new FlashSaleModel();
            result.Id = model.Id;
            result.Title = model.Title;
            result.ShopId = model.ShopId;
            result.ProductId = model.ProductId;
            result.Status = model.Status;
            result.ProductName = product.ProductName;
            result.ProductImg = product.RelativePath;
            result.MarketPrice = product.MarketPrice;
            result.StatusStr = model.Status.ToDescription();
            result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
            result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
            result.LimitCountOfThePeople = model.LimitCountOfThePeople;
            result.SaleCount = model.SaleCount;
            result.CategoryName = model.CategoryName;
            result.MinPrice = result.SkuMinPrice = result.SkuMaxPrice = model.MinPrice;
            result.Details = new List<FlashSaleDetailModel>();

            var details = DbFactory.Default.Get<FlashSaleDetailInfo>().Where(p => p.FlashSaleId == result.Id).ToList();
            var skus = DbFactory.Default.Get<SKUInfo>().Where(p => p.ProductId == model.ProductId).ToList();
            #region 阶梯商品价格--ZYF
            var price = GetMinLadderPrice(model.ProductId);
            #endregion
            if (details != null && details.Count() > 0)
            {
                result.SkuMinPrice = details.Min(t => t.Price);
                result.SkuMaxPrice = details.Max(t => t.Price);
            }

            foreach (var sku in skus)
            {
                var detail = details.FirstOrDefault(p => p.SkuId == sku.Id);
                FlashSaleDetailModel d = new FlashSaleDetailModel();
                d.Id = detail == null ? 0 : detail.Id;
                d.SkuId = sku.Id;
                d.Price = detail == null ? sku.SalePrice : (decimal)detail.Price;
                d.Color = sku.Color;
                d.Size = sku.Size;
                d.Version = sku.Version;
                d.Stock = (int)sku.Stock;
                d.TotalCount = detail == null ? 0 : Math.Min((int)sku.Stock, detail.TotalCount);
                d.CostPrice = sku.CostPrice;
                d.SalePrice = price > 0 ? price : sku.SalePrice;
                d.minMath = 0;
                result.Details.Add(d);
            }
            result.Quantity = result.Details.Sum(a => a.TotalCount);

            //if( details != null )
            //{
            //    foreach( var detail in details )
            //    {
            //        var sku = context.SKUInfo.FirstOrDefault( p => p.Id == detail.SkuId );
            //        if( sku == null )
            //        {
            //            //如果sku为空，证明限时购的sku记录与商品的不一致
            //            //证明商品在限时购已存在的情况下修改了sku相关信息
            //            //暂时还没做处理
            //            break;
            //        }
            //        FlashSaleDetailModel d = new FlashSaleDetailModel();
            //        d.Id = detail.Id;
            //        d.SkuId = detail.SkuId;
            //        d.Price = ( decimal )detail.Price;
            //        d.Color = sku.Color;
            //        d.Size = sku.Size;
            //        d.Version = sku.Version;
            //        d.Stock = ( int )sku.Stock;
            //        d.CostPrice = sku.CostPrice;
            //        d.SalePrice = sku.SalePrice;
            //        result.Details.Add( d );
            //    }
            //}

            return result;
        }


        private void CheckFlashSale(FlashSaleModel model)
        {
            if (DbFactory.Default.Get<FlashSaleInfo>().Where(p => (p.Id != 0 && p.Id != model.Id && p.ShopId == model.ShopId && p.ProductId == model.ProductId && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) ||
              (p.Id != 0 && p.Id != model.Id && p.ShopId == model.ShopId && p.ProductId == model.ProductId && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)).Exist())
            {
                throw new MallException("此商品已存在限时购活动");
            }
            var co = DbFactory.Default.Get<ActiveMarketServiceInfo>().Where(a => a.TypeId == MarketType.LimitTimeBuy && a.ShopId == model.ShopId).FirstOrDefault();
            if (co == null)
            {
                throw new MallException("您没有订购此服务");
            }
            var date = Convert.ToDateTime(model.EndDate);
            var endtime = MarketApplication.GetServiceEndTime(co.Id);
            var newEnd = DateTime.Parse(endtime.ToString("yyyy-MM-dd") + " 23:59:59");
            if (newEnd < date)
            {
                //throw new MallException("结束日期不能超过购买限时购服务的日期");
                throw new MallException(string.Format("活动结束时间不得超过服务到期时间，<br/>您的服务到期时间为{0}", newEnd.ToString("yyyy-MM-dd HH:mm:ss")));
            }
        }

        public void AddFlashSale(FlashSaleModel model)
        {
            CheckFlashSale(model);

            var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == model.ProductId).FirstOrDefault();
            DbFactory.Default.InTransaction(() =>
            {
                FlashSaleInfo flashSale = new FlashSaleInfo();
                flashSale.Title = model.Title;
                flashSale.ShopId = model.ShopId;
                flashSale.ProductId = model.ProductId;
                flashSale.Status = model.Status;
                flashSale.BeginDate = DateTime.Parse(model.BeginDate);
                flashSale.EndDate = DateTime.Parse(model.EndDate);
                flashSale.CategoryName = model.CategoryName;
                flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
                flashSale.SaleCount = model.SaleCount;
                flashSale.ImagePath = product.RelativePath;
                flashSale.MinPrice = model.Details.Min(p => p.Price);
                DbFactory.Default.Add(flashSale);

                foreach (var detail in model.Details)
                {
                    FlashSaleDetailInfo fsd = new FlashSaleDetailInfo();
                    fsd.FlashSaleId = flashSale.Id;
                    fsd.ProductId = flashSale.ProductId;
                    fsd.SkuId = detail.SkuId;
                    fsd.Price = detail.Price;
                    fsd.TotalCount = detail.TotalCount;
                    DbFactory.Default.Add(fsd);
                }
            });
        }

        public void UpdateFlashSale(FlashSaleModel model)
        {
            CheckFlashSale(model);
            FlashSaleInfo flashSale = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            if (flashSale == null)
            {
                throw new MallException("此活动已被删除");
            }
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<FlashSaleDetailInfo>().Where(p => p.ProductId == model.ProductId).Succeed();
                if (flashSale.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                {
                    if (flashSale.ProductId != model.ProductId)
                    {
                        var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == model.ProductId).FirstOrDefault();
                        flashSale.ImagePath = product.RelativePath;
                    }

                    flashSale.Title = model.Title;
                    flashSale.ShopId = model.ShopId;
                    flashSale.ProductId = model.ProductId;
                    flashSale.BeginDate = DateTime.Parse(model.BeginDate);
                    flashSale.CategoryName = model.CategoryName;
                    flashSale.EndDate = DateTime.Parse(model.EndDate);
                    flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
                    flashSale.MinPrice = model.Details.Min(p => p.Price);

                    if (model.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                    {
                        flashSale.Status = FlashSaleInfo.FlashSaleStatus.WaitForAuditing;
                    }
                    DbFactory.Default.Update(flashSale);

                    foreach (var detail in model.Details)
                    {
                        FlashSaleDetailInfo fsd = new FlashSaleDetailInfo();
                        fsd.ProductId = flashSale.ProductId;
                        fsd.SkuId = detail.SkuId;
                        fsd.Price = detail.Price;
                        fsd.TotalCount = detail.TotalCount;
                        fsd.FlashSaleId = flashSale.Id;
                        DbFactory.Default.Add(fsd);
                    }
                }
                else if (flashSale.Status == FlashSaleInfo.FlashSaleStatus.Ongoing)
                {
                    flashSale.Title = model.Title;
                    flashSale.CategoryName = model.CategoryName;
                    flashSale.LimitCountOfThePeople = model.LimitCountOfThePeople;
                    flashSale.EndDate = DateTime.Parse(model.EndDate);
                    flashSale.MinPrice = model.Details.Min(p => p.Price);
                    if (model.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                    {
                        flashSale.Status = FlashSaleInfo.FlashSaleStatus.WaitForAuditing;
                    }
                    DbFactory.Default.Update(flashSale);
                    foreach (var detail in model.Details)
                    {
                        FlashSaleDetailInfo fsd = new FlashSaleDetailInfo();
                        fsd.ProductId = flashSale.ProductId;
                        fsd.SkuId = detail.SkuId;
                        fsd.Price = detail.Price;
                        fsd.TotalCount = detail.TotalCount;
                        fsd.FlashSaleId = flashSale.Id;
                        DbFactory.Default.Add(fsd);
                    }
                }
            });
        }

        public void Pass(long id)
        {
            var model = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (model == null || model.EndDate <= DateTime.Now)
            {
                throw new MallException("活动不存在或过期!");
            }
            var detail = DbFactory.Default.Get<FlashSaleDetailInfo>().Where(p => p.FlashSaleId == model.Id && p.ProductId == model.ProductId).FirstOrDefault();
            model.Status = FlashSaleInfo.FlashSaleStatus.Ongoing;
            model.MinPrice = (decimal)detail.Price;
            DbFactory.Default.Update(model);
        }

        public void Refuse(long id)
        {
            var model = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (model == null)
            {
                throw new MallException("活动不存在!");
            }

            model.Status = FlashSaleInfo.FlashSaleStatus.AuditFailed;
            DbFactory.Default.Update(model);
        }

        public void Cancel(long id)
        {
            var model = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (model == null)
            {
                throw new MallException("活动不存在!");
            }

            model.Status = FlashSaleInfo.FlashSaleStatus.Cancelled;
            model.EndDate = DateTime.Now;
            DbFactory.Default.Update(model);
        }

        public bool IsAdd(long productid)
        {
            var result = DbFactory.Default.Get<FlashSaleInfo>().Where(p => (p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) || (
                p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)).Exist();

            return !result;
        }
        public bool IsEdit(long productid, long id)
        {
            bool result = true;
            if (id > 0)
            {
                result = DbFactory.Default.Get<FlashSaleInfo>().Where(p => ((p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing) || (
                    p.ProductId == productid && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)) && p.Id != id).Exist();
            }
            else
            {
                result = IsAdd(productid);
            }
            return !result;
        }

        public void IncreaseSaleCount(List<long> orderids)
        {
            //限时购销量
            var orderid = orderids[0];
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == orderid).FirstOrDefault();

            if (order == null || order.OrderType != OrderInfo.OrderTypes.LimitBuy) return;
            long productid = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == orderid).FirstOrDefault().ProductId;
            var now = DateTime.Now;
            var result = DbFactory.Default.Get<FlashSaleInfo>().Where(e => e.ProductId == productid && e.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && e.BeginDate <= now && e.EndDate > now).FirstOrDefault(); ;
            if (result != null)
            {
                long saleCount = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId == orderid).FirstOrDefault().Quantity;
                result.SaleCount = result.SaleCount + (int)saleCount;
                DbFactory.Default.Update(result);
            }
        }

        /// <summary>
        /// 订单退款减少销量
        /// </summary>
        /// <param name="orderids"></param>
        public void ReduceSaleCount(OrderRefundInfo refund)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(p => p.Id == refund.OrderId).FirstOrDefault();
            if (order == null) return;
            var orderItem = OrderApplication.GetOrderItem(refund.OrderItemId);
            //限时购销量
            var orderid = orderItem.OrderId;
            long productid = orderItem.ProductId;
            var result = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == orderItem.FlashSaleId && p.ProductId == productid && p.BeginDate < DateTime.Now && p.EndDate > DateTime.Now && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing).FirstOrDefault();
            if (result != null)
            {
                var refoundInfo = DbFactory.Default.Get<OrderRefundInfo>().Where(p => p.OrderId == orderid).FirstOrDefault();

                if (refoundInfo != null)
                {
                    long saleCount = 0;
                    //退款取订单详情数量
                    if (refoundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                    {
                        saleCount = orderItem.Quantity;
                    }
                    //退货退款，取退货数量
                    else if (refoundInfo.RefundMode == OrderRefundInfo.OrderRefundMode.ReturnGoodsRefund)
                    {
                        if (refund.IsReturn)
                        {
                            //判断是否有会员发货，没有会员发货视为弃货，不扣销量
                            if (!string.IsNullOrEmpty(refoundInfo.ExpressCompanyName) && !string.IsNullOrEmpty(refoundInfo.ShipOrderNumber))
                                saleCount = refoundInfo.ReturnQuantity;
                        }
                    }
                    //弃货不扣销量
                    var count = result.SaleCount - (int)saleCount;
                    result.SaleCount = count < 0 ? 0 : count;
                    DbFactory.Default.Update(result);
                }
            }
        }

        public FlashSaleModel IsFlashSaleDoesNotStarted(long productid)
        {
            string cacheKey = CacheKeyCollection.CACHE_PRODUCTLIMITNOTSTART(productid);
            if (Cache.Exists(cacheKey))
                return Cache.Get<FlashSaleModel>(cacheKey);

            var model = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.ProductId == productid
                && p.Status == FlashSaleInfo.FlashSaleStatus.Ongoing
                && p.BeginDate > DateTime.Now).FirstOrDefault();

            if (model != null)
            {
                var product = ProductManagerApplication.GetProduct(model.ProductId);
                FlashSaleModel result = new FlashSaleModel();
                result.Id = model.Id;
                result.Title = model.Title;
                result.ShopId = model.ShopId;
                result.ProductId = model.ProductId;
                result.Status = model.Status;
                result.ProductName = product.ProductName;
                result.ProductImg = product.ImagePath;
                result.StatusStr = model.Status.ToDescription();
                result.BeginDate = model.BeginDate.ToString("yyyy-MM-dd HH:mm");
                result.EndDate = model.EndDate.ToString("yyyy-MM-dd HH:mm");
                result.LimitCountOfThePeople = model.LimitCountOfThePeople;
                result.SaleCount = model.SaleCount;
                result.CategoryName = model.CategoryName;
                result.MinPrice = model.MinPrice;
                Cache.Insert<FlashSaleModel>(cacheKey, result, model.BeginDate.AddSeconds(-10));//缓存至开始时间前10秒
                return result;
            }
            Cache.Insert<FlashSaleModel>(cacheKey, null, 60);
            return null;
        }

        public List<FlashSalePrice> GetPriceByProducrIds(List<long> ids)
        {
            //List<FlashSalePrice> list = new List<FlashSalePrice>();
            //string idlist = ids != null && ids.Count > 0 ? string.Join(",", ids) : "";
            //var sql = new Sql("SELECT ProductId,MinPrice FROM Mall_FlashSale ");
            //sql.Append(string.Format(" WHERE {0} ", ids != null && ids.Count > 0 ? " ProductId IN (" + idlist + ") " : " 1=1 "));
            //sql.Append(" AND Status =@0 AND BeginDate < @1 AND EndDate > @2", FlashSaleInfo.FlashSaleStatus.Ongoing, DateTime.Now, DateTime.Now);

            //list = DbFactory.Default.Query<FlashSalePrice>(sql).ToList();//, new { @Status = FlashSaleInfo.FlashSaleStatus.Ongoing, @BeginDate = DateTime.Now, @EndDate = DateTime.Now }).ToList();
            var db = DbFactory.Default
                .Get<FlashSaleInfo>()
                .LeftJoin<ProductInfo>((fsi, pi) => fsi.ProductId == pi.Id)
                .Where(n => n.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && n.BeginDate < DateTime.Now && n.EndDate > DateTime.Now);
            if (ids != null && ids.Count > 0)
            {
                db.Where(n => n.ProductId.ExIn(ids));
            }
            var list = db.Select(n => new { n.Id, n.ProductId, n.MinPrice }).Select<ProductInfo>(p => p.ProductName)
               .ToList<FlashSalePrice>();
            return list;
        }

        /// <summary>
        /// 提前结束
        /// </summary>
        public void EndActive(long id)
        {
            var data = DbFactory.Default.Get<FlashSaleInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (data != null)
            {
                data.EndDate = DateTime.Now.AddMinutes(-1);
                DbFactory.Default.Update(data);
            }
        }

        public FlashSaleInfo GetFlashSaleInfo(long id)
        {
            return DbFactory.Default.Get<FlashSaleInfo>().Where(d => d.Id == id).FirstOrDefault();
        }
        #endregion

        #region 开团提醒定时任务

        public void AddRemind(long flashSaleId, string openId)
        {
#if DEBUG
            Log.Info("限时购 - 添加订阅用户");
#endif
            if (DbFactory.Default.Get<FlashSaleRemindInfo>().Where(p => p.FlashSaleId == flashSaleId && p.OpenId == openId).Exist())
            {
                return;
            }

            FlashSaleRemindInfo model = new FlashSaleRemindInfo();
            model.FlashSaleId = flashSaleId;
            model.OpenId = openId;
            model.RecordDate = DateTime.Now;
            DbFactory.Default.Add(model);

            var flashSale = DbFactory.Default.Get<FlashSaleInfo>().Where(p => p.Id == flashSaleId).FirstOrDefault();
            var product = ProductManagerApplication.GetProduct(flashSale.ProductId);
            string openid = openId;
            var msgdata = new WX_MsgTemplateSendDataModel();
            msgdata.first.value = "亲爱的用户，您已经成功设置限时购订阅提醒！";
            msgdata.first.color = "#000000";
            msgdata.keyword1.value = product.ProductName;
            msgdata.keyword1.color = "#FF0000";
            msgdata.keyword2.value = flashSale.MinPrice.ToString();
            msgdata.keyword2.color = "#FF0000";
            msgdata.keyword3.value = model.RecordDate.ToString("yyyy-MM-dd HH:mm");
            msgdata.keyword3.color = "#000000";
            msgdata.remark.value = "活动开始时将自动提醒您，请关注您的微信消息。";
            msgdata.remark.color = "#000000";
            //处理url
            var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //string url = _iwxtser.GetMessageTemplateShowUrl(MessageTypeEnum.SubscribeLimitTimeBuy);
            //Task.Factory.StartNew(() => ServiceProvider.Instance<IWXMsgTemplateService>.Create.SendMessageByTemplate(MessageTypeEnum.SubscribeLimitTimeBuy, 0, msgdata, url, openid));
        }

    
        #endregion

        public int GetMarketSaleCountForUserId(long pId, long userId)
        {
            int count = 0;

            var list = DbFactory.Default.Get<OrderItemInfo>().InnerJoin<OrderInfo>((oii, oi) => oii.OrderId == oi.Id)
                .Where(n => n.IsLimitBuy == true && n.ProductId == pId)
                .Where<OrderInfo>(n => n.UserId == userId && n.OrderStatus != OrderInfo.OrderOperateStatus.Close)
                .Select(n => n.Quantity)
                .Select<OrderInfo>(n => n.OrderDate)
                .ToList<dynamic>();//, new { UserId = userId, ProductId = pId }).ToList();
            if (list.Any())
            {
                var limit = DbFactory.Default.Get<FlashSaleInfo>().Where(n => n.ProductId == pId && n.Status == FlashSaleInfo.FlashSaleStatus.Ongoing && n.EndDate >= DateTime.Now).FirstOrDefault();
                if (limit != null)
                {
                    count = list.Where(a => a.OrderDate >= limit.BeginDate).Sum(r => (int)r.Quantity);
                }
            }

            return count;
        }

        /// <summary>
        /// 获取阶梯商品最小批量价格
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        private decimal GetMinLadderPrice(long pid)
        {
            var product = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id == pid).FirstOrDefault();
            var price = 0m;
            if (product.IsOpenLadder)
            {
                var ladder = DbFactory.Default.Get<ProductLadderPriceInfo>().Where(p => p.ProductId == pid)
                    .OrderBy(m => m.MinBath).FirstOrDefault();
                if (null != ladder)
                    price = ladder.Price;
            }
            return price;
        }

        /// <summary>
        /// 根据限时购id集合获取限时购详细列表
        /// </summary>
        /// <param name="flashSaleIds">限时购ids</param>
        /// <returns></returns>
        public List<FlashSaleDetailInfo> GetFlashSaleDetailByFlashSaleIds(IEnumerable<long> flashSaleIds)
        {
            return DbFactory.Default.Get<FlashSaleDetailInfo>().Where(s => s.ProductId.ExIn(flashSaleIds)).ToList();
        }
    }

}
