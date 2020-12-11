using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Application
{
    /// <summary>
    /// 门店应用服务
    /// </summary>
    public class ShopBranchApplication : BaseApplicaion<IShopBranchService>
    {
        private static IAppMessageService _appMessageService =  EngineContext.Current.Resolve<IAppMessageService>();
        private static ICouponService _iCouponService =  EngineContext.Current.Resolve<ICouponService>();

        #region 密码加密处理
        /// <summary>
        /// 二次加盐后的密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GetPasswrodEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }
        /// <summary>
        /// 取密码盐
        /// </summary>
        /// <returns></returns>
        public static string GetSalt()
        {
            return Guid.NewGuid().ToString("N").Substring(12);
        }
        #endregion 密码加密处理

        #region 查询相关
        /// <summary>
        /// 获取门店
        /// </summary>
        /// <returns></returns>
        public static ShopBranch GetShopBranchById(long id)
        {
            var branch = Service.GetShopBranchById(id);

            //  var shopBranch = AutoMapper.Mapper.Map<ShopBranchInfo, ShopBranch>(branch);
            var shopBranch = branch.Map< ShopBranch>();


            if (shopBranch != null)
            {
                shopBranch.AddressFullName = RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 0);
            }

            if (branch != null)
            {
                var branchManager = GetShopBranchManagerByShopBranchId(branch.Id);
                if (branchManager != null)
                    shopBranch.UserName = branchManager.UserName;//补充管理员名称


                var tags = Service.GetShopBranchInTagByBranchs(new List<long> { id });
                if (tags.Count > 0)//补充标签
                    shopBranch.ShopBranchTagId = string.Join(",", tags.Select(p => p.TagId));
            }
            return shopBranch;
        }


        /// <summary>
        /// 根据 IDs批量获取门店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchByIds(List<long> ids)
        {
            var branchInfos = Service.GetShopBranchByIds(ids);
            //  var shopBranchs = AutoMapper.Mapper.Map<List<ShopBranch>>(branchInfos);
            var shopBranchs = branchInfos.Map<List<ShopBranch>>();


            return shopBranchs;
        }

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static ShopBranch GetShopBranchByContact(string contact)
        {
            var branchInfo = Service.GetShopBranchByContact(contact);
            return branchInfo.Map<DTO.ShopBranch>();
        }

        /// <summary>
        /// 分页查询门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetShopBranchs(ShopBranchQuery query)
        {
            var data = Service.GetShopBranchs(query);
            var branchs = data.Models;
            var tags = Service.GetShopBranchInTagByBranchs(branchs.Select(p => p.Id).ToList());
            var shops = GetService<IShopService>().GetShops(branchs.Select(p => p.ShopId).ToList());
            var shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = branchs.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    AddressFullName = RegionApplication.GetFullName(e.AddressId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + e.AddressDetail,
                    AddressId = e.AddressId,
                    ContactPhone = e.ContactPhone,
                    ContactUser = e.ContactUser,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    ShopId = e.ShopId,
                    Status = e.Status,
                    ShopBranchInTagNames = string.Join(",", tags.Where(p => p.ShopBranchId == e.Id).Select(p => p.Title)),
                    ShopName = shops.FirstOrDefault(p => p.Id == e.ShopId)?.ShopName ?? string.Empty
                }).ToList(),
                Total = data.Total
            };
            return shopBranchs;
        }
        public static List<ShopBranch> GetShopBranchByShopId(long shopId)
        {
            var shopBranch = Service.GetShopBranchByShopId(shopId).ToList();
            return shopBranch.Map<List<ShopBranch>>();
        }
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchs(List<long> ids)
        {
            var shopBranchs = Service.GetShopBranchs(ids).Map<List<ShopBranch>>();
            //补充地址详细信息,地址库采用了缓存，循环取
            foreach (var b in shopBranchs)
            {
                b.AddressFullName = RegionApplication.GetFullName(b.AddressId);
                b.RegionIdPath = RegionApplication.GetRegionPath(b.AddressId);
            }
            return shopBranchs;
        }
        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <returns></returns>
        public static List<ShopBranchSkuInfo> GetSkus(long shopId, List<long> shopBranchIds, List<string> skuids = null)
        {
            return Service.GetSkus(shopId, shopBranchIds, skuids: skuids);
        }

        public static List<ShopBranchSkuInfo> GetSkus(long shop, List<long> branchs)
        {
            return Service.GetSkus(shop, branchs);
        }
        public static List<ShopBranchSkuInfo> GetSkus(long shop, long branch, ShopBranchSkuStatus? status = ShopBranchSkuStatus.Normal)
        {
            return Service.GetSkus(shop, new List<long> { branch }, status);
        }
        /// <summary>
        /// 根据SKU AUTOID取门店SKU
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuids"></param>
        /// <returns></returns>
        public static List<Entities.ShopBranchSkuInfo> GetSkusByIds(long shopBranchId, List<string> skuids)
        {
            var list = Service.GetSkusByIds(shopBranchId, skuids);
            return list;
        }
        /// <summary>
        /// 根据商品ID取门店sku信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSkusByProductId(long shopBranchId, long pid)
        {
            var sku = ProductManagerApplication.GetSKUs(pid);
            var shopBranchSkus = Service.GetSkusByIds(shopBranchId, sku.Select(e => e.Id).ToList());
            foreach (var item in sku)
            {
                var branchSku = shopBranchSkus.FirstOrDefault(e => e.SkuId == item.Id);
                if (branchSku != null)
                    item.Stock = branchSku.Stock;
            }
            return sku;
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopBranchManager GetShopBranchManager(long id)
        {
            var managerInfo = Service.GetShopBranchManagersById(id);
          //  AutoMapper.Mapper.CreateMap<ShopBranchManagerInfo, ShopBranchManager>();
           // var manager = AutoMapper.Mapper.Map<ShopBranchManagerInfo, ShopBranchManager>(managerInfo);

            var manager = managerInfo.Map< ShopBranchManager>();

            //管理员类型为门店管理员
            manager.UserType = ManagerType.ShopBranchManager;
            return manager;
        }

        /// <summary>
        /// 根据门店id获取门店管理员
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static ShopBranchManager GetShopBranchManagerByShopBranchId(long branchId)
        {
            var data = Service.GetShopBranchManagers(branchId);
            var manager = data.FirstOrDefault();//门店仅唯一账户
            //if (manager == null)
            //    throw new MessageException(ExceptionMessages.NoFound, "门店管理");
            return manager.Map<ShopBranchManager>();
        }

        /// <summary>
        /// 门店商品查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProducts(ShopBranchProductQuery query)
        {
            var pageModel = Service.SearchProduct(query);
            //补充数据
            foreach (var p in pageModel.Models)
            {
                //补充门店销售数量
                p.SaleCounts = OrderApplication.GetSaleCount(shopBranchId: query.ShopBranchId, productId: p.Id);

            }
            return pageModel;
        }
        /// <summary>
        /// 获取门店销量
        /// </summary>
        /// <param name="branchId">门店ID</param>
        /// <param name="hasVirtual">是否包含虚拟销量</param>
        /// <returns></returns>
        public static long GetShopBranchSaleCount(long branchId, bool hasVirtual = true)
        {
            return GetShopBranchSaleCount(branchId, null, null, hasVirtual);
        }
        public static long GetShopBranchSaleCount(long branchId, DateTime? begin, DateTime? end, bool hasVirtual = true)
        {
            var saleCount = OrderApplication.GetSaleCount(shopBranchId: branchId, startDate: begin, endDate: end);
            if (hasVirtual)
            {
                var virtualCount = Service.GetVirtualSaleCounts(branchId);
                saleCount += virtualCount;
            }
            return saleCount;
        }


        /// <summary>
        /// 根据日期获取门店商品销量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProductsMonth(ShopBranchProductQuery query, DateTime startDate, DateTime endDate)
        {
            var pageModel = Service.SearchProduct(query);
            //var pids = pageModel.Models.Select(p => p.Id).ToList();
            //var saleCount = Service.GetProductSaleCount(query.ShopBranchId.Value, pids, startDate, endDate);
            //pageModel.Models.ForEach(product =>
            //{
            //    product.SaleCounts = saleCount.ContainsKey(product.Id) ? saleCount[product.Id] : 0;
            //});
            return pageModel;
        }

        /// <summary>
        /// 根据日期获取门店产品销售数量
        /// </summary>
        /// <param name="shopBranchId">门店标识</param>
        /// <param name="productId">产品标示</param>
        /// <param name="startDate">启始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public static long GetProductSaleCount(long shopBranchId, long productId, DateTime startDate, DateTime endDate)
        {
            var saleCount = Service.GetProductSaleCount(shopBranchId, new List<long> { productId }, startDate, endDate);
            return saleCount.ContainsKey(productId) ? saleCount[productId] : 0;
        }

        public static bool CheckProductIsExist(long shopBranchId, long productId)
        {
            return Service.CheckProductIsExist(shopBranchId, productId);
        }

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool Exists(ShopBranchQuery query)
        {
            return Service.Exists(query);
        }

        #endregion

        #region 门店管理
        /// <summary>
        /// 新增门店
        /// </summary>
        public static void AddShopBranch(ShopBranch shopBranch, out long shopBranchId)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new MallException("此门店名称已存在，请设置其他名称！");
            }
            var branchManangerInfo = Service.GetShopBranchManagersByName(shopBranch.UserName);
            if (branchManangerInfo != null)
            {
                throw new MallException("此门店管理员账号已存在，请设置其他名称！");
            }
            if (ManagerApplication.CheckUserNameExist(shopBranch.UserName))
            {
                throw new MallException("此门店管理员账号已存在，请设置其他名称！");
            }
           // AutoMapper.Mapper.CreateMap<ShopBranch, Entities.ShopBranchInfo>();
          //  var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, Entities.ShopBranchInfo>(shopBranch);

            var shopBranchInfo = shopBranch.Map< Entities.ShopBranchInfo>();

            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            Service.AddShopBranch(shopBranchInfo);
            shopBranchId = shopBranchInfo.Id;
            var salt = GetSalt();
            var shopBranchManagerInfo = new Entities.ShopBranchManagerInfo
            {
                CreateDate = DateTime.Now,
                UserName = shopBranch.UserName,
                ShopBranchId = shopBranchInfo.Id,
                PasswordSalt = salt,
                Password = GetPasswrodEncode(shopBranch.PasswordOne, salt)
            };
            Service.AddShopBranchManagers(shopBranchManagerInfo);
            shopBranch.Id = shopBranchInfo.Id;
        }
        /// <summary>
        /// 更新门店信息、管理员密码
        /// </summary>
        /// <param name="shopBranch"></param>
        public static void UpdateShopBranch(ShopBranch shopBranch)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
                throw new MallException("门店名称不能重复！");

            //AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            //var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);

            var shopBranchInfo = shopBranch.Map<ShopBranchInfo>();


            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            Service.UpdateShopBranch(shopBranchInfo);

            if (!string.IsNullOrEmpty(shopBranch.PasswordOne))
            {
                if (shopBranch.PasswordOne != shopBranch.PasswordTwo)
                    throw new MessageException("两次密码输入不一致");
                //设置门店管理密码
                SetShopBranchManagerPassword(shopBranchInfo.Id, shopBranch.PasswordOne);
            }
        }

        /// <summary>
        /// 验证密码正确性
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckManagerPassword(long managerId, string password)
        {
            var manager = Service.GetShopBranchManagersById(managerId);
            if (manager == null)
                throw new MessageException(ExceptionMessages.NoFound, "门店管理");
            password = GetPasswrodEncode(password, manager.PasswordSalt);
            return password == manager.Password;
        }
        /// <summary>
        /// 设置门店管理员密码(唯一账户)
        /// </summary>
        /// <param name="branchId">门店ID</param>
        /// <param name="password">密码</param>
        public static void SetShopBranchManagerPassword(long branchId, string password)
        {
            var manager = GetShopBranchManagerByShopBranchId(branchId);
            if (manager == null)
                throw new MessageException(ExceptionMessages.NoFound, "门店管理");
            SetManagerPassword(manager.Id, password);
        }
        /// <summary>
        /// 设置门店管理员密码
        /// </summary>
        /// <param name="managerId">管理员ID</param>
        /// <param name="password">密码</param>
        public static void SetManagerPassword(long managerId, string password)
        {
            var manager = Service.GetShopBranchManagersById(managerId);
            if (string.IsNullOrEmpty(manager.PasswordSalt))
                manager.PasswordSalt = GetSalt();
            password = GetPasswrodEncode(password, manager.PasswordSalt);
            Service.SetShopBranchManagerPassword(manager.Id, password, manager.PasswordSalt);
        }

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="branchId"></param>
        public static void DeleteShopBranch(long branchId)
        {
            //TODO:门店删除逻辑

            Service.DeleteShopBranch(branchId);
        }
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void Freeze(long shopBranchId)
        {
            Service.FreezeShopBranch(shopBranchId);
        }
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void UnFreeze(long shopBranchId)
        {
            Service.UnFreezeShopBranch(shopBranchId);
        }
        #endregion 门店管理

        #region 门店登录
        /// <summary>
        /// 门店登录验证
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ShopBranchManager ShopBranchLogin(string userName, string password)
        {
            var managerInfo = Service.GetShopBranchManagersByName(userName);
            if (managerInfo == null)
                return null;

            password = GetPasswrodEncode(password, managerInfo.PasswordSalt);
            if (!string.Equals(password, managerInfo.Password))
                return null;

           // AutoMapper.Mapper.CreateMap<Entities.ShopBranchManagerInfo, ShopBranchManager>();
          //  var manager = AutoMapper.Mapper.Map<Entities.ShopBranchManagerInfo, ShopBranchManager>(managerInfo);


            var manager = managerInfo.Map<ShopBranchManager>();


            return manager;
        }
        #endregion 门店登录

        #region 门店商品管理
        /// <summary>
        /// 添加SKU，并过滤已添加的
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void AddProductSkus(List<long> pids, long shopBranchId, long shopId)
        {
            var products = ProductManagerApplication.GetProducts(pids, shopId);
            if (products.Count == 0)
                throw new MessageException("未找到商品数据");
            if (products.Any(d => d.IsOpenLadder))
                throw new MessageException("不可添加阶梯价商品");

            //查询已添加的SKU，用于添加时过滤
            var oldskus = Service.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
            var allSkus = ProductManagerApplication.GetSKUByProducts(products.Select(p => p.Id));
            var shopBranchSkus = new List<ShopBranchSkuInfo>();

            var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkuInfo
            {
                ProductId = e.ProductId,
                SkuId = e.Id,
                ShopId = shopId,
                ShopBranchId = shopBranchId,
                Stock = 0,
                CreateDate = DateTime.Now
            });

            shopBranchSkus.AddRange(skus);
            Service.AddSkus(shopBranchSkus);
        }
        /// <summary>
        /// 修正商品sku
        /// <para>0库存添加新的sku</para>
        /// </summary>
        /// <param name="productId"></param>
        public static void CorrectBranchProductSkus(long productId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProduct(productId);
            if (productsInfo == null || productsInfo.ShopId != shopId)
            {
                throw new MallException("未找到商品数据");
            }
            var shopbrids = Service.GetAgentShopBranchIds(productId);
            List<long> pids = new List<long>();
            pids.Add(productId);

            foreach (var shopBranchId in shopbrids)
            {
                //查询已添加的SKU，用于添加时过滤
                var oldskus = Service.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
                var allSkus = ProductManagerApplication.GetSKUByProducts(pids);
                var shopBranchSkus = new List<Entities.ShopBranchSkuInfo> { };

                var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new Entities.ShopBranchSkuInfo
                {
                    ProductId = e.ProductId,
                    SkuId = e.Id,
                    ShopId = shopId,
                    ShopBranchId = shopBranchId,
                    Stock = 0,
                    CreateDate = DateTime.Now
                });
                shopBranchSkus.AddRange(skus);

                Service.AddSkus(shopBranchSkus);
            }
        }


        public static void SetSkuStock(long branch, StockOptionType option, Dictionary<string, int> changes)
        {
            Service.SetStock(branch, changes, option);
        }
        /// <summary>
        /// 修改门店商品库存
        /// </summary>
        /// <param name="branchId"></param>
        /// <param name="products"></param>
        /// <param name="stock"></param>
        /// <param name="option"></param>
        public static void SetProductStock(long branchId, List<long> products, int stock, StockOptionType option)
        {
            foreach (var pro in products)
                Service.SetProductStock(branchId, pro, stock, option);
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void UnSaleProduct(long shopBranchId, List<long> productIds)
        {
            Service.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 下架所有门店的商品
        /// <para></para>
        /// </summary>
        /// <param name="productId"></param>
        public static void UnSaleProduct(long productId)
        {
            Service.SetBranchProductStatus(productId, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 检测商品是否可以上架
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static bool CanOnSaleProduct(IEnumerable<long> productIds)
        {
            bool result = false;
            var products = ProductManagerApplication.GetProductsByIds(productIds);
            if (products.Count() == productIds.Count())
            {
                result = true;
            }
            return result;
        }

        public static bool IsOpenLadderInProducts(IEnumerable<long> productIds)
        {
            bool result = false;
            var products = ProductManagerApplication.GetProductsByIds(productIds);
            if (products.Where(p => p.IsOpenLadder).Count() > 0)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void OnSaleProduct(long shopBranchId, List<long> productIds)
        {
            Service.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.Normal);
        }
        #endregion 门店商品管理

        #region 私有方法
        private static bool isRepeatBranchName(long shopId, long shopBranchId, string branchName)
        {
            var exists = Service.Exists(shopId, shopBranchId, branchName);
            return exists;
        }
        #endregion
        /// <summary>
        /// 取门店商品数量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<Entities.ShopBranchSkuInfo> GetShopBranchProductCount(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var skus = Service.SearchShopBranchSkus(shopBranchId, startDate, endDate);
            return skus;
        }

        #region 周边门店
        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetNearShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = Service.GetNearShopBranchs(query);
            return new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = MallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsFreeMail = e.IsFreeMail,
                    IsRecommend = (e.IsAboveSelf && e.IsRecommend) || (e.IsStoreDelive && e.Distance <= e.ServeRadius ? e.IsRecommend : false),
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).OrderBy(e => e.RecommendSequence).ToList(),
                Total = shopBranchInfos.Total
            };

        }
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>关键字包括对门店名称和门店商品的过滤</returns>
        public static QueryPageModel<ShopBranch> SearchNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = Service.SearchNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = MallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsFreeMail = e.IsFreeMail,
                    IsRecommend = (e.IsAboveSelf && e.IsRecommend) || (e.IsStoreDelive && e.Distance <= e.ServeRadius ? e.IsRecommend : false),
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            //修正距离，数据库计算出来的结果有点偏差
            foreach (var item in shopBranchs.Models)
            {
                if (item.Latitude > 0 || item.Longitude > 0)
                {
                    item.Distance = Service.GetLatLngDistancesFromAPI(search.FromLatLng, item.Latitude + "," + item.Longitude);
                    item.DistanceUnit = item.Distance >= 1 ? item.Distance + "KM" : item.Distance * 1000 + "M";
                }
            }
            return shopBranchs;
        }
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签的过滤</returns>
        public static QueryPageModel<ShopBranch> TagsSearchNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = Service.TagsSearchNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = MallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsFreeMail = e.IsFreeMail,
                    IsRecommend = e.IsRecommend,
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        /// <summary>
        /// 根据商品搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签的过滤</returns>
        public static QueryPageModel<ShopBranch> StoreByProductNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = Service.StoreByProductNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = MallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsFreeMail = e.IsFreeMail,
                    IsRecommend = e.IsRecommend,
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }

        /// <summary>
        /// 组合新地址
        /// </summary>
        /// <param name="addressPath"></param>
        /// <param name="address"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string RenderAddress(string addressPath, string address, int level)
        {
            if (!string.IsNullOrWhiteSpace(addressPath))
            {
                string fullName = RegionApplication.GetRegionName(addressPath);
                string[] arr = fullName.Split(',');//省，市，区，街道
                if (arr.Length > 0)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(arr[i]))
                        {
                            address = address.Replace(arr[i], "");//去掉原详细地址中的省市区街道。(为兼容旧门店数据)
                        }
                    }
                }

                if (level <= arr.Length)
                {
                    for (int i = 0; i < level; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(arr[i]))
                        {
                            fullName = fullName.Replace(arr[i], "");
                        }
                    }
                    address = fullName + address;
                }
            }
            if (!string.IsNullOrWhiteSpace(address))
            {
                address = address.Replace(",", " ");
            }
            return address;
        }
        public static QueryPageModel<ShopBranch> GetShopBranchsAll(ShopBranchQuery query)
        {
            var shopBranchInfos = Service.GetShopBranchsAll(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = TypeHelper.ObjectToInt(e.ServeRadius),
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    AddressPath = e.AddressPath,
                    ContactUser = e.ContactUser
                }).ToList()
            };
            return shopBranchs;
        }

        /// <summary>
        /// 获取门店-不分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetAllShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = Service.GetAllShopBranchs(query);
            return shopBranchInfos.Select(n => new ShopBranch
            {
                Id = n.Id,
                ShopBranchName = n.ShopBranchName,
                AddressDetail = GetShopBranchsFullAddress(n.AddressPath) + n.AddressDetail,
                RecommendSequence = n.RecommendSequence
            }).OrderBy(n => n.RecommendSequence).ToList();
        }

        public static string GetShopBranchsFullAddress(string addressPath)
        {
            var str = string.Empty;
            if (!string.IsNullOrEmpty(addressPath))
            {
                str = RegionApplication.GetRegionName(addressPath);
            }
            return str.Replace(",", "");
        }

        /// <summary>
        /// 推荐门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static bool RecommendShopBranch(long[] ids)
        {
            var flag = Service.RecommendShopBranch(ids);
            return flag;
        }

        /// <summary>
        /// 推荐门店排序
        /// </summary>
        /// <param name="oriShopBranchId">门店ID</param>
        /// <param name="newShopBranchId">门店ID</param>
        /// <returns></returns>
        public static bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            var flag = Service.RecommendChangeSequence(oriShopBranchId, newShopBranchId);
            return flag;
        }

        /// <summary>
        /// 取消推荐门店
        /// </summary>
        /// <param name="shopBranchId">门店ID</param>
        /// <returns></returns>
        public static bool ResetShopBranchRecommend(long shopBranchId)
        {
            var flag = Service.ResetShopBranchRecommend(shopBranchId);
            return flag;
        }

        #endregion
        #region 商家手动分配门店
        /// <summary>
        /// 获取商家下该区域范围内的可选门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetArealShopBranchsAll(int areaId, int shopId, string latAndLng)
        {
            float latitude = 0; float longitude = 0;
            var arrLatAndLng = HttpUtility.UrlDecode(latAndLng).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (arrLatAndLng.Length == 2)
            {
                float.TryParse(arrLatAndLng[0], out latitude);
                float.TryParse(arrLatAndLng[1], out longitude);
            }
            var shopBranchInfos = Service.GetArealShopBranchsAll(areaId, shopId, latitude, longitude);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                }).ToList()
            };
            return shopBranchs;
        }
        #endregion

        #region 门店标签相关
        /// <summary>
        /// 查询所有标签
        /// </summary>
        /// <returns></returns>
        public static List<ShopBranchTagModel> GetAllShopBranchTagInfos()
        {
            var tags = Service.GetAllShopBranchTagInfo();
            var counts = Service.GetShopBranchInTagCount(tags.Select(p => p.Id).ToList());
            return tags.Select(e => new ShopBranchTagModel
            {
                Id = e.Id,
                Title = e.Title,
                ShopBranchCount = counts.ContainsKey(e.Id) ? counts[e.Id] : 0
            }).ToList();
        }
        /// <summary>
        /// 根据ID查询标签
        /// </summary>
        /// <param name="Id">标签标识</param>
        /// <returns></returns>
        public static ShopBranchTagModel GetShopBranchTagInfo(long id, bool isInfo = true)
        {
            var tag = Service.GetShopBranchTagInfo(id);
            if (tag == null)
            {
                if (isInfo)
                    throw new MessageException(ExceptionMessages.NoFound, "标签");
                else
                    return null;
            }
            var count = Service.GetShopBranchInTagCount(id);
            return new ShopBranchTagModel
            {
                Id = tag.Id,
                Title = tag.Title,
                ShopBranchCount = count
            };
        }

        public static int GetShopBranchInTagCount(long branch)
        {
            return Service.GetShopBranchInTagCount(branch);
        }
        /// <summary>
        /// 增加标签
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        public static void AddShopBranchTagInfo(string title)
        {
            if (string.IsNullOrEmpty(title)) throw new Exception("标签名称不可为空");
            Service.AddShopBranchTagInfo(title);
        }
        /// <summary>
        /// 修改标签名称
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        /// <returns></returns>
        public static void UpdateShopBranchTagInfo(long id, string title)
        {
            if (id <= 0) throw new MallException("修改目标标签不可为空");
            if (string.IsNullOrEmpty(title)) throw new MallException("标签名称不可为空");
            Service.UpdateShopBranchTagInfo(id, title);
        }
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static void DeleteShopBranchTagInfo(long Id)
        {
            Service.DeleteShopBranchTagInfo(Id);
        }
        /// <summary>
        /// 批量设置门店标签
        /// </summary>
        /// <param name="brands"></param>
        /// <param name="tags"></param>
        public static void SetShopBrandTagInfos(List<long> brands, List<long> tags)
        {
            Service.SetShopBranchTags(brands, tags);
        }

        #endregion
        /// <summary>
        /// 获取两点间距离
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="latlng"></param>
        /// <returns></returns>
        public static double GetLatLngDistances(string fromLatLng, string latlng)
        {
            return Service.GetLatLngDistancesFromAPI(fromLatLng, latlng);
        }

        /// <summary>
        /// 判断当前用户是否领取优惠卷
        /// </summary>
        /// <param name="couponinfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int CouponIsUse(Entities.CouponInfo couponinfo, long userId)
        {
            var status = 0;
            CouponRecordQuery crQuery = new CouponRecordQuery();
            if (userId > 0)
            {//检验当前会员是否可领
                crQuery.CouponId = couponinfo.Id;
                crQuery.UserId = userId;
                QueryPageModel<Entities.CouponRecordInfo> pageModel = _iCouponService.GetCouponRecordList(crQuery);
                if (couponinfo.PerMax != 0 && pageModel.Total >= couponinfo.PerMax)
                {
                    //达到个人领取最大张数
                    status = 1;
                }
            }
            if (status == 0)
            {//检验优惠券本身是否可领
                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponinfo.Id
                };
                QueryPageModel<Entities.CouponRecordInfo> pageModel = _iCouponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponinfo.Num)
                {
                    //达到领取最大张数
                    status = 2;
                }
            }
            return status;
        }

        public static ShopStoreServiceMark GetServiceMark(long Id)
        {
            return Service.GetServiceMark(Id);
        }
        /// <summary>
        /// 取用户在门店的购物车数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="branchIds"></param>
        /// <returns></returns>
        public static Dictionary<long, int> GetShopBranchCartItemCount(long userId, List<long> branchIds)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            if (userId == 0)
            {
                return result;
            }
            Mall.Entities.ShoppingCartInfo memberCartInfo = new Mall.Entities.ShoppingCartInfo();
            List<DTO.Product.Product> onSaleProducts = new List<DTO.Product.Product>();
            if (userId > 0)
            {//如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(userId);
                if (memberCartInfo != null)
                {
                    onSaleProducts = ProductManagerApplication.GetAllStatusProductByIds(memberCartInfo.Items.Select(e => e.ProductId)).ToList();
                }
            }

            Dictionary<long, long> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, long>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, memberCartInfo.Items.Select(x => x.ProductId));
            }
            var shopBranchSkuList = Service.GetSkusByBranchIds(branchIds, skuids: memberCartInfo.Items.Select(s => s.SkuId).ToList());

            foreach (var id in branchIds)
            {
                //var cartQuantity = memberCartInfo.Items.Where(c => c.ShopBranchId.HasValue && c.ShopBranchId.Value == query.shopBranchId).Sum(c => c.Quantity);
                //过滤购物车 无效商品
                var cartQuantity = memberCartInfo.Items.Where(c =>c.ShopBranchId == id).Select(item =>
                {
                    var product = onSaleProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    var shopbranchsku = shopBranchSkuList.FirstOrDefault(x => x.ShopBranchId == id && x.SkuId == item.SkuId);
                    long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                    if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                        stock = product.MaxBuyCount;
                    if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                    {
                        long buynum = buyedCounts[item.ProductId];
                        stock = stock - buynum;
                    }
                    var status = product.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > stock ? 2 : 0) : 1);//0:正常；1：冻结；2：库存不足
                    if (status == 0)
                    {
                        return item.Quantity;
                    }
                    else
                    {
                        return 0;
                    }
                }).Sum(count => count);
                if (cartQuantity > 0)
                    result.Add(id, cartQuantity);
            }
            return result;

        }

        /// <summary>
        /// 获取启用商家管理的门店
        /// </summary>
        /// <returns></returns>
        public static List<ShopBranchInfo> GetSellerManager(long shop)
        {
            return Service.GetManagerShops(shop);
        }
        /// <summary>
        /// 启用门店管理
        /// </summary>
        public static void EnableManager(long branch)
        {
            Service.SetManagerShop(branch, true);
        }
        /// <summary>
        /// 禁用门店管理
        /// </summary>
        public static void DisableManger(long branch)
        {
            Service.SetManagerShop(branch, false);
        }


        public static List<ShopBranchInTag> GetShopBranchInTag(long branch)
        {
            return Service.GetShopBranchInTagByBranchs(new List<long> { branch });
        }

        public static ShopBranchInfo GetShopBranchInfoById(long shopBranchId)
        {
            return Service.GetShopBranchById(shopBranchId);
        }
    }
}
