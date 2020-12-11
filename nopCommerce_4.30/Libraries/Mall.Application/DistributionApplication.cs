using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.Distribution;
using Mall.DTO.QueryModel;
using Mall.IServices;
using System.Collections.Generic;
using System.Linq;
using System;
using Mall.Entities;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mall.Core.Plugins.Payment;
using Mall.Core.Plugins;
using Mall.Core.Plugins.Message;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    /// <summary>
    /// 商品类别
    /// </summary>
    public class DistributionApplication : BaseApplicaion<IDistributionService>
    {
        //private static IDistributionService _iDistributionService =  EngineContext.Current.Resolve<IDistributionService>();
      //  private static IOrderService _iOrderService =  EngineContext.Current.Resolve<IOrderService>();
    //    private static IShopService _iShopService =  EngineContext.Current.Resolve<IShopService>();
    //    private static IRefundService _iRefundService =  EngineContext.Current.Resolve<IRefundService>();


        private static IDistributionService _iDistributionService =  EngineContext.Current.Resolve<IDistributionService>();
        private static IOrderService _iOrderService =  EngineContext.Current.Resolve<IOrderService>();
        private static IShopService _iShopService =  EngineContext.Current.Resolve<IShopService>();
        private static IRefundService _iRefundService =  EngineContext.Current.Resolve<IRefundService>();
        /// <summary>
        /// 获取分销商品集
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<DistributionProduct> GetProducts(DistributionProductQuery query)
        {
            if (query.CategoryId.HasValue)
            {
                var categories = GetService<ICategoryService>().GetAllCategoryByParent(query.CategoryId.Value);
                query.Categories = categories.Select(p => p.Id).ToList();
                query.Categories.Add(query.CategoryId.Value);
            }
            var result = _iDistributionService.GetProducts(query);
            foreach (var item in result.Models)
            {
                item.DefaultImage = MallIO.GetProductSizeImage(item.ImagePath, 1, (int)ImageSize.Size_500);
                item.NoSettlementAmount = Math.Round(item.NoSettlementAmount, 2, MidpointRounding.AwayFromZero);
            }
            return result;
        }

        /// <summary>
        /// 分销商品(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<DistributionProduct> GetProductsAll(DistributionProductQuery query)
        {
            if (query.CategoryId.HasValue)
            {
                var categories = GetService<ICategoryService>().GetAllCategoryByParent(query.CategoryId.Value);
                query.Categories = categories.Select(p => p.Id).ToList();
                query.Categories.Add(query.CategoryId.Value);
            }
            var result = _iDistributionService.GetProductsAll(query);
            return result;
        }

        /// <summary>
        /// 获取有分销商品的一级分类
        /// </summary>
        /// <returns></returns>
        public static List<CategoryInfo> GetHaveDistributionProductTopCategory()
        {
            return _iDistributionService.GetHaveDistributionProductTopCategory();
        }
        /// <summary>
        /// 获取分销商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static DistributionProduct GetProduct(long productId)
        {
            DistributionProduct result = _iDistributionService.GetProduct(productId);
            return result;
        }
        /// <summary>
        /// 获取分销中的商品
        /// </summary>
        /// <param name="curshopid"></param>
        /// <returns></returns>
        public static List<long> GetAllDistributionProductIds(long shopId)
        {
            return _iDistributionService.GetAllDistributionProductIds(shopId);
        }
        /// <summary>
        /// 推广分销商品
        /// </summary>
        /// <param name="productIds"></param>
        public static void AddSpreadProducts(IEnumerable<long> productIds, long shopId)
        {
            _iDistributionService.AddSpreadProducts(productIds, shopId);
        }
        /// <summary>
        /// 取消推广分销商品
        /// </summary>
        /// <param name="productIds"></param>
        public static void RemoveSpreadProducts(IEnumerable<long> productIds, long? shopId = null)
        {
            _iDistributionService.RemoveSpreadProducts(productIds, shopId);
        }
        /// <summary>
        /// 修改商品分佣比例
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="rate"></param>
        /// <param name="level"></param>
        public static void SetProductBrokerageRate(long productId, decimal rate, decimal? rate2 = null, decimal? rate3 = null)
        {
            var config = SiteSettingApplication.SiteSettings;
            var pro = _iDistributionService.GetProduct(productId);
            if (pro == null)
                throw new MallException("分销商品不存在！");
            pro.BrokerageRate1 = rate;
            pro.BrokerageRate2 = rate2 ?? pro.BrokerageRate2;
            pro.BrokerageRate3 = rate3 ?? pro.BrokerageRate3;
            var sumrate = rate;
            if (rate < 0.1m || rate > 100)
            {
                throw new MallException("错误的分佣比例！");
            }
            if (config.DistributionMaxLevel > 1)
            {
                if (!rate2.HasValue)
                {
                    throw new MallException("错误的分佣比例！");
                }
                if (rate2 < 0.1m || rate2 > 100)
                {
                    throw new MallException("错误的分佣比例！");
                }
                sumrate += rate2.Value;
            }
            if (config.DistributionMaxLevel > 2)
            {
                if (!rate3.HasValue)
                {
                    throw new MallException("错误的分佣比例！");
                }
                if (rate3 < 0.1m || rate3 > 100)
                {
                    throw new MallException("错误的分佣比例！");
                }
                sumrate += rate3.Value;
            }
            if (sumrate > config.DistributionMaxBrokerageRate)
            {
                throw new MallException("多级比例总和需在 0.1% ~ " + config.DistributionMaxBrokerageRate + "% 之间");
            }
            _iDistributionService.SetProductBrokerageRate(pro.ProductId, pro.BrokerageRate1, pro.BrokerageRate2, pro.BrokerageRate3);
        }
        /// <summary>
        /// 获取默认分佣比
        /// </summary>
        /// <param name="shopId"></param>
        public static DistributionShopRateConfigInfo GetDefaultBrokerageRate(long shopId)
        {
            var result = _iDistributionService.GetDefaultBrokerageRate(shopId);
            if (result == null)
            {
                result = new DistributionShopRateConfigInfo()
                {
                    ShopId = shopId,
                    BrokerageRate1 = 0,
                    BrokerageRate2 = 0,
                    BrokerageRate3 = 0,
                };
                _iDistributionService.UpdateDefaultBrokerageRate(shopId, result);
                result = _iDistributionService.GetDefaultBrokerageRate(shopId);
            }
            return result;
        }


        public static QueryPageModel<BrokerageOrder> GetBrokerageOrders(BrokerageOrderQuery query)
        {
            var data = Service.GetBrokerageOrders(query);

            //填充 门店名 与 会员名
            var shops = GetService<IShopService>().GetShops(data.Models.Select(p => p.ShopId));
            var memberIds = new List<long>();
            memberIds.AddRange(data.Models.Select(p => p.SuperiorId1));
            memberIds.AddRange(data.Models.Select(p => p.SuperiorId2));
            memberIds.AddRange(data.Models.Select(p => p.SuperiorId3));
            var members = GetService<IMemberService>().GetMembers(memberIds);
            data.Models.ForEach(item =>
            {
                item.ShopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty;
                item.SuperiorName1 = members.FirstOrDefault(p => p.Id == item.SuperiorId1)?.UserName ?? string.Empty;
                item.SuperiorName2 = members.FirstOrDefault(p => p.Id == item.SuperiorId2)?.UserName ?? string.Empty;
                item.SuperiorName3 = members.FirstOrDefault(p => p.Id == item.SuperiorId3)?.UserName ?? string.Empty;
            });
            return data;
        }

        /// <summary>
        /// 分销订单列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isShopName">是否填充商铺名称</param>
        /// <param name="isShopName">是否填分销员(会员)</param>
        /// <returns></returns>
        public static List<BrokerageOrder> GetBrokerageOrdersAll(BrokerageOrderQuery query, bool isShopName = false, bool isSupperior = false)
        {
            var data = Service.GetBrokerageOrdersAll(query);

            if (isShopName)
            {
                //填充 门店名
                var shops = GetService<IShopService>().GetShops(data.Select(p => p.ShopId));
                if (isSupperior)//填充分销员
                {
                    var memberIds = new List<long>();
                    memberIds.AddRange(data.Select(p => p.SuperiorId1));
                    memberIds.AddRange(data.Select(p => p.SuperiorId2));
                    memberIds.AddRange(data.Select(p => p.SuperiorId3));
                    var members = GetService<IMemberService>().GetMembers(memberIds);
                    data.ForEach(item =>
                    {
                        item.ShopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty;
                        item.SuperiorName1 = members.FirstOrDefault(p => p.Id == item.SuperiorId1)?.UserName ?? string.Empty;
                        item.SuperiorName2 = members.FirstOrDefault(p => p.Id == item.SuperiorId2)?.UserName ?? string.Empty;
                        item.SuperiorName3 = members.FirstOrDefault(p => p.Id == item.SuperiorId3)?.UserName ?? string.Empty;
                    });
                }
                else
                {
                    data.ForEach(item =>
                    {
                        item.ShopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty;
                    });
                }
            }
            return data;
        }

        public static QueryPageModel<BrokerageProduct> GetBrokerageProduct(BrokerageProductQuery query)
        {
            return Service.GetBrokerageProduct(query);
        }

        /// <summary>
        /// 获取分销明细列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <param name="isShopName">是否填充商铺名称</param>
        /// <returns></returns>
        public static List<BrokerageProduct> GetBrokerageProductAll(BrokerageProductQuery query, bool isShopName = false)
        {
            var data = Service.GetBrokerageProductAll(query);

            if (isShopName)
            {
                //填充 门店名
                var shops = GetService<IShopService>().GetShops(data.Select(p => p.ShopId));
                data.ForEach(item =>
                {
                    item.ShopName = shops.FirstOrDefault(p => p.Id == item.ShopId)?.ShopName ?? string.Empty;
                });
            }
            return Service.GetBrokerageProductAll(query);
        }

        /// <summary>
        /// 修改默认分佣比
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        public static void UpdateDefaultBrokerageRate(long shopId, DistributionShopRateConfigInfo data)
        {
            _iDistributionService.UpdateDefaultBrokerageRate(shopId, data);
        }
        /// <summary>
        /// 重置非开放等级分佣比
        /// </summary>
        /// <param name="maxLevel"></param>
        public static void ResetDefaultBrokerageRate(int maxLevel)
        {
            _iDistributionService.ResetDefaultBrokerageRate(maxLevel);
        }

        public static QueryPageModel<DistributorRecordInfo> GetRecords(DistributorRecordQuery query)
        {
            return Service.GetRecords(query);
        }
        /// <summary>
        /// 获取销售员
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public static DistributorInfo GetDistributor(long memberId)
        {
            return _iDistributionService.GetDistributor(memberId);
        }

        public static Distributor GetDistributorDTO(long member)
        {
            var model = Service.GetDistributorBase(member);
            var distributor = model.Map<Distributor>();
            distributor.Member = GetService<IMemberService>().GetMember(member);
            distributor.Grade = GetDistributorGrade(distributor.GradeId) ?? new DistributorGradeInfo();
            return distributor;
        }

        /// <summary>
        /// 获取未结算佣金
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static decimal GetNoSettlementAmount(long member)
        {
            return Service.GetNoSettlementAmount(member);
        }
        /// <summary>
        /// 获得分销业绩
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static DistributionAchievement GetAchievement(long member)
        {
            var settings = SiteSettingApplication.SiteSettings;
            var result=Service.GetSubAchievement(member);
            if (result == null)
            {
                result = new DistributionAchievement
                {
                    MemberId = member
                };
            }
            return result;
        }

        /// <summary>
        /// 获取销售员列表
        /// </summary>
        public static QueryPageModel<DistributorListDTO> GetDistributors(DistributorQuery query)
        {
            var result = new QueryPageModel<DistributorListDTO>();
            var data = _iDistributionService.GetDistributors(query);
            result.Models = data.Models.Map<List<DistributorListDTO>>();
            result.Total = data.Total;
            return result;
        }

        /// <summary>
        /// 获取销售员列表(忽略分页)
        /// </summary>
        public static List<DistributorListDTO> GetDistributorsAll(DistributorQuery query)
        {
            var result = new List<DistributorListDTO>();
            var data = _iDistributionService.GetDistributorsAll(query);
            result = data.Map<List<DistributorListDTO>>();
            return result;
        }

        public static Dictionary<int, List<long>> GetSubordinate(long superior)
        {
            return Service.GetSubordinate(superior, SiteSettingApplication.SiteSettings.DistributionMaxLevel);
        }

        /// <summary>
        /// 获取佣金管理列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<Distributor> GetDistributors(DistributorSubQuery query)
        {
            //获得所有下级
            if (query.SuperiorId > 0)
            {
                var subordinate = Service.GetSubordinate(query.SuperiorId, query.Level);
                query.Members = subordinate[query.Level];
            }
            var data = Service.GetNewDistributors(query);
            var result = data.Models.Map<List<Distributor>>();
            var memberids = data.Models.Select(p => p.MemberId).ToList();
            var members = GetService<IMemberService>().GetMembers(memberids);
            var achievements = Service.GetAchievement(memberids);
            result.ForEach(item =>
            {
                item.Member = members.FirstOrDefault(p => p.Id == item.MemberId);
                item.Achievement = achievements.FirstOrDefault(p => p.MemberId == item.MemberId);
            });
            return new QueryPageModel<Distributor>
            {
                Models = result,
                Total = data.Total
            };
        }

        /// <summary>
        /// 获取佣金管理列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<Distributor> GetDistributorsAll(DistributorSubQuery query)
        {
            //获得所有下级
            if (query.SuperiorId > 0)
            {
                var subordinate = Service.GetSubordinate(query.SuperiorId, query.Level);
                query.Members = subordinate[query.Level];
            }
            var data = Service.GetNewDistributorsAll(query);
            var result = data.Map<List<Distributor>>();
            var memberids = data.Select(p => p.MemberId).ToList();
            var members = GetService<IMemberService>().GetMembers(memberids);
            var achievements = Service.GetAchievement(memberids);
            result.ForEach(item =>
            {
                item.Member = members.FirstOrDefault(p => p.Id == item.MemberId);
                item.Achievement = achievements.FirstOrDefault(p => p.MemberId == item.MemberId);
            });

            return result;
        }


        /// <summary>
        /// 申请成为销售员
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopLogo"></param>
        /// <param name="shopName"></param>
        /// <returns></returns>
        public static DistributorInfo ApplyDistributor(long memberId, string shopLogo, string shopName)
        {
            DistributorInfo result = _iDistributionService.GetDistributor(memberId);
            bool isadd = false;
            if (result == null)
            {
                result = new DistributorInfo();
                isadd = true;
                result.ProductCount = 0;
                result.OrderCount = 0;
                result.SettlementAmount = 0;
            }
            result.MemberId = memberId;
            result.ShopLogo = shopLogo;
            result.IsShowShopLogo = true;
            result.ShopName = shopName;
            result.ApplyTime = DateTime.Now;
            result.DistributionStatus = (int)DistributorStatus.UnAudit;
            if (!SiteSettingApplication.SiteSettings.DistributorNeedAudit)
            {
                result.DistributionStatus = (int)DistributorStatus.Audited;
            }
            var gradeId = GetDistributorGrades().OrderByDescending(d => d.Quota).FirstOrDefault(d => d.Quota <= result.SettlementAmount)?.Id;
            result.GradeId = gradeId ?? 0;

            if (isadd)
            {
                _iDistributionService.AddDistributor(result);
            }
            else
            {
                _iDistributionService.UpdateDistributor(result);
            }
            var uobj = MemberApplication.GetMember(result.MemberId);
            //发送短信通知
            Task.Factory.StartNew(() =>
            {
                MessageApplication.SendMessageOnDistributorApply(result.MemberId, uobj.UserName);
                if (result.DistributionStatus == (int)DistributorStatus.Audited)
                {
                    MessageApplication.SendMessageOnDistributorAuditSuccess(result.MemberId, uobj.UserName);
                }
            });
            return result;
        }
        /// <summary>
        /// 小店设置
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopLogo"></param>
        /// <param name="shopName"></param>
        /// <returns></returns>
        public static void UpdateDistributorConfig(long memberId, string shopLogo, string shopName, bool isShowLogo)
        {
            DistributorInfo result = _iDistributionService.GetDistributor(memberId);
            if (result == null)
            {
                throw new MallException("错误的编号");
            }
            result.ShopLogo = shopLogo;
            result.ShopName = shopName;
            result.IsShowShopLogo = isShowLogo;
            _iDistributionService.UpdateDistributor(result);
        }
        /// <summary>
        /// 添加等级
        /// </summary>
        /// <param name="data"></param>
        public static void AddDistributorGrade(DistributorGradeInfo data)
        {
            _iDistributionService.AddDistributorGrade(data);
        }
        /// <summary>
        /// 修改等级
        /// </summary>
        /// <param name="data"></param>
        public static void UpdateDistributorGrade(DistributorGradeInfo data)
        {
            _iDistributionService.UpdateDistributorGrade(data);
        }
        /// <summary>
        /// 获取销售员等级
        /// </summary>
        /// <returns></returns>
        public static DistributorGradeInfo GetDistributorGrade(long id)
        {
            return _iDistributionService.GetDistributorGrade(id);
        }
        /// <summary>
        /// 获取销售员等级列表
        /// </summary>
        /// <returns></returns>
        public static List<DistributorGradeInfo> GetDistributorGrades(bool IsAvailable = false)
        {
            return _iDistributionService.GetDistributorGrades(IsAvailable);
        }
        /// <summary>
        /// 是否己存在同名等级
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id">0表示新增检测</param>
        /// <returns></returns>
        public static bool ExistDistributorGradesName(string name, long id)
        {
            return _iDistributionService.ExistDistributorGradesName(name, id);
        }
        /// <summary>
        /// 是否己存在同条件等级
        /// </summary>
        /// <param name="quota"></param>
        /// <param name="id">0表示新增检测</param>
        /// <returns></returns>
        public static bool ExistDistributorGradesQuota(decimal quota, long id)
        {
            return _iDistributionService.ExistDistributorGradesQuota(quota, id);
        }
        /// <summary>
        /// 清退销售员
        /// </summary>
        /// <param name="memberIds"></param>
        public static void RemoveDistributor(IEnumerable<long> memberIds)
        {
            _iDistributionService.RemoveDistributor(memberIds);
        }
        /// <summary>
        /// 恢复销售员
        /// </summary>
        /// <param name="memberIds"></param>
        public static void RecoverDistributor(IEnumerable<long> memberIds)
        {
            _iDistributionService.RecoverDistributor(memberIds);
        }
        /// <summary>
        /// 拒绝销售员申请
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="remark"></param>
        public static void RefuseDistributor(IEnumerable<long> memberIds, string remark)
        {
            _iDistributionService.RefuseDistributor(memberIds, remark);
            //发送短信通知
            Task.Factory.StartNew(() =>
            {
                foreach (var item in memberIds)
                {
                    var uobj = MemberApplication.GetMember(item);
                    var duobj = _iDistributionService.GetDistributor(item);
                    MessageApplication.SendMessageOnDistributorAuditFail(item, uobj.UserName, remark, duobj.ApplyTime);
                }
            });
        }
        /// <summary>
        /// 同意销售员申请
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="remark"></param>
        public static void AgreeDistributor(IEnumerable<long> memberIds, string remark)
        {
            _iDistributionService.AgreeDistributor(memberIds, remark);
            //发送短信通知
            Task.Factory.StartNew(() =>
            {
                foreach (var item in memberIds)
                {
                    var uobj = MemberApplication.GetMember(item);
                    MessageApplication.SendMessageOnDistributorAuditSuccess(item, uobj.UserName);
                }
            });
        }
        /// <summary>
        /// 调整上级
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="superMemberId"></param>
        public static void UpdateDistributorSuperId(long MemberId, long superMemberId)
        {
            long oldsuperid = 0;
            var cur = _iDistributionService.GetDistributor(MemberId);
            if (cur == null)
            {
                throw new MallException("错误的参数");
            }
            oldsuperid = cur.SuperiorId;
            cur.SuperiorId = superMemberId;
            _iDistributionService.UpdateDistributor(cur);
            //维护下级数量
            _iDistributionService.SyncDistributorSubNumber(MemberId);
            _iDistributionService.SyncDistributorSubNumber(superMemberId);
            if (oldsuperid != superMemberId)
            {
                _iDistributionService.SyncDistributorSubNumber(oldsuperid);
            }
        }
        /// <summary>
        /// 删除销售员等级
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteDistributorGrade(long id)
        {
            Service.DeleteDistributorGrade(id);
        }
        /// <summary>
        /// 获取销售员小店订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<DistributorBrokerageOrder> GetDistributorBrokerageOrderList(DistributionBrokerageQuery query)
        {
            var orders = _iDistributionService.GetDistributorBrokerageOrderList(query);
            var orderids = orders.Models.Select(d => d.Id).ToList();
            var itemBrokerages = _iDistributionService.GetDistributionBrokerageByOrderIds(orderids);
            var shopids = itemBrokerages.Select(d => d.ShopId).Distinct().ToList();
            var orderItems = _iOrderService.GetOrderItemsByOrderId(orderids);
            var shops = _iShopService.GetShops(shopids);
            var refunds = _iRefundService.GetAllOrderRefunds(new RefundQuery
            {
                MoreOrderId = orderids
            });
            QueryPageModel<DistributorBrokerageOrder> result = new QueryPageModel<DistributorBrokerageOrder>
            {
                Models = new List<DistributorBrokerageOrder>(),
                Total = orders.Total
            };
            foreach (var item in orders.Models)
            {
                var bitems = itemBrokerages.Where(d => d.OrderId == item.Id).ToList();
                var oitems = orderItems.Where(d => d.OrderId == item.Id).ToList();
                if (bitems == null || bitems.Count == 0)
                {
                    continue;
                }
                var first = bitems.FirstOrDefault();
                var shdatas = refunds.Where(d => d.OrderId == item.Id);
                DistributorBrokerageOrder odata = new DistributorBrokerageOrder
                {
                    OrderId = first.OrderId,
                    OrderStatus = item.OrderStatus,
                    Status = first.BrokerageStatus,
                    SettlementTime = first.SettlementTime,
                    Items = new List<DistributorBrokerageOrderItem>(),
                    OrderAmount = item.OrderAmount,
                    OrderDate = item.OrderDate,
                    IsRefundCloseOrder = (item.OrderStatus == OrderInfo.OrderOperateStatus.Close && shdatas.Any(d => d.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed))
                };
                int SuperiorLevel = 1;
                decimal BrokerageRate = 0, BrokerageAmount = 0;
                foreach (var oitem in oitems)
                {
                    string[] _skus = (new string[] { oitem.Color, oitem.Size, oitem.Version }).Where(d => !string.IsNullOrWhiteSpace(d)).ToArray();
                    var _shop = shops.FirstOrDefault(d => d.Id == oitem.ShopId);
                    DistributorBrokerageOrderItem idata = new DistributorBrokerageOrderItem
                    {
                        ProductId = oitem.ProductId,
                        ProductName = oitem.ProductName,
                        ProductDefaultImage = MallIO.GetRomoteProductSizeImage(oitem.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                        Sku = string.Join("、", _skus),
                        ShopId = oitem.ShopId,
                        OrderItemId = oitem.Id,
                        Quantity = oitem.Quantity,
                        ShopName = _shop.ShopName,
                        IsHasRefund = item.OrderStatus != OrderInfo.OrderOperateStatus.Close && refunds.Any(d => d.OrderItemId == oitem.Id
                         && d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund
                         && d.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.Confirmed)
                    };
                    var bitem = itemBrokerages.FirstOrDefault(d => d.OrderItemId == oitem.Id);
                    if (bitem != null)
                    {
                        idata.SuperiorId1 = bitem.SuperiorId1;
                        idata.BrokerageRate1 = bitem.BrokerageRate1;
                        idata.SuperiorId2 = bitem.SuperiorId2;
                        idata.BrokerageRate2 = bitem.BrokerageRate2;
                        idata.SuperiorId3 = bitem.SuperiorId3;
                        idata.BrokerageRate3 = bitem.BrokerageRate3;
                        idata.RealPayAmount = bitem.RealPayAmount;
                        idata.SettlementTime = bitem.SettlementTime;
                        idata.Status = bitem.BrokerageStatus;
                        if (idata.SuperiorId1 == query.DistributorId)
                        {
                            SuperiorLevel = 1;
                            BrokerageRate = idata.BrokerageRate1;
                        }
                        if (idata.SuperiorId2 == query.DistributorId)
                        {
                            SuperiorLevel = 2;
                            BrokerageRate = idata.BrokerageRate2;
                        }
                        if (idata.SuperiorId3 == query.DistributorId)
                        {
                            SuperiorLevel = 3;
                            BrokerageRate = idata.BrokerageRate3;
                        }
                        BrokerageAmount += GetDivide100Number(idata.RealPayAmount * BrokerageRate);
                    }
                    odata.Items.Add(idata);
                }
                odata.SuperiorLevel = SuperiorLevel;
                odata.BrokerageAmount = BrokerageAmount;
                odata.QuantitySum = odata.Items.Sum(d => d.Quantity);
                result.Models.Add(odata);
            }
            result.Models = result.Models.OrderByDescending(d => d.OrderDate).ThenByDescending(d=>d.OrderId).ToList();
            return result;
        }

        #region 销量排行


        public static QueryPageModel<BrokerageRanking> GetRankings(DistributorRankingQuery query)
        {
            var data = Service.GetRankings(query);

            var memberids = data.Models.Select(p => p.MemberId).ToList();
            var members = GetService<IMemberService>().GetMembers(memberids);
            var distributors = Service.GetDistributors(memberids);
            var rankingIndex = (query.PageNo - 1) * query.PageSize;

            var rankings = data.Models.Select(item =>
            {
                var distributor = distributors.FirstOrDefault(p => p.MemberId == item.MemberId);
                return new BrokerageRanking
                {
                    Rank = ++rankingIndex,
                    Amount = item.Amount,
                    NoSettlement = item.NoSettlement,
                    Quantity = item.Quantity,
                    Settlement = item.Settlement,
                    Member = members.FirstOrDefault(p => p.Id == item.MemberId),
                    Distributor = distributor,
                    Grade = GetDistributorGrade(distributor.GradeId),
                };
            }).ToList();

            return new QueryPageModel<BrokerageRanking>
            {
                Models = rankings,
                Total = data.Total
            };
        }

        /// <summary>
        /// 获取排行数据（忽略分页）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<BrokerageRanking> GetRankingsAll(DistributorRankingQuery query)
        {
            var data = Service.GetRankingsAll(query);

            var memberids = data.Select(p => p.MemberId).ToList();
            var members = GetService<IMemberService>().GetMembers(memberids);
            var distributors = Service.GetDistributors(memberids);
            var rankingIndex = (query.PageNo - 1) * query.PageSize;

            var rankings = data.Select(item =>
            {
                var distributor = distributors.FirstOrDefault(p => p.MemberId == item.MemberId);
                return new BrokerageRanking
                {
                    Rank = ++rankingIndex,
                    Amount = item.Amount,
                    NoSettlement = item.NoSettlement,
                    Quantity = item.Quantity,
                    Settlement = item.Settlement,
                    Member = members.FirstOrDefault(p => p.Id == item.MemberId),
                    Distributor = distributor,
                    Grade = GetDistributorGrade(distributor.GradeId),
                };
            }).ToList();

            return rankings;
        }

        /// <summary>
        /// 生成报表
        /// </summary>
        public static void GenerateRankingAsync(DateTime begin, DateTime end)
        {
            if (CheckGenerating())
                throw new MallException("上次报表生成进行中...");
            Task.Factory.StartNew(() =>
            {
                #region 仅保留一次排名报表,移除之前所有报表数据
                var batchs = Service.GetRankingBatchs();
                if (batchs.Count > 0)
                    Service.RemoveRankingBatch(batchs.Select(p => p.Id).ToList());
                #endregion

                //生成报表
                var batch = new DistributionRankingBatchInfo
                {
                    BeginTime = begin,
                    EndTime = end,
                    CreateTime = DateTime.Now,
                };
                Cache.Insert(CacheKeyCollection.GenerateDistributionRankingAsync, batch);
                try
                {
                    Service.GenerateRanking(begin, end);
                }
                catch (Exception ex)
                {
                    Log.Error("分销业绩排行报表生成出错:", ex);
                }
                finally
                {
                    Cache.Remove(CacheKeyCollection.GenerateDistributionRankingAsync);
                }
            });
        }
        /// <summary>
        /// 检查是否生成中
        /// </summary>
        /// <returns></returns>
        public static bool CheckGenerating()
        {
            return Cache.Exists(CacheKeyCollection.GenerateDistributionRankingAsync);
        }
        public static DistributionRankingBatchInfo GetLastRankingBatch()
        {
            return Service.GetLastRankingBatch();
        }
        #endregion

        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<DistributionWithdraw> GetWithdraws(DistributionWithdrawQuery query)
        {
            var data = Service.GetWithdraws(query);
            var list = data.Models.Map<List<DistributionWithdraw>>();
            var members = GetService<IMemberService>().GetMembers(list.Select(p => p.MemberId).ToList());
            list.ForEach(item =>
            {
                item.Member = members.FirstOrDefault(p => p.Id == item.MemberId);
            });

            return new QueryPageModel<DistributionWithdraw>
            {
                Models = list,
                Total = data.Total
            };
        }

        /// <summary>
        /// 获取提现记录(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<DistributionWithdraw> GetWithdrawsAll(DistributionWithdrawQuery query)
        {
            var data = Service.GetWithdrawsAll(query);
            var list = data.Map<List<DistributionWithdraw>>();
            var members = GetService<IMemberService>().GetMembers(list.Select(p => p.MemberId).ToList());
            list.ForEach(item =>
            {
                item.Member = members.FirstOrDefault(p => p.Id == item.MemberId);
            });

            return list;
        }

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="apply"></param>
        public static void ApplyWithdraw(DistributionApplyWithdraw apply)
        {
            if (!MemberApplication.VerificationPayPwd(apply.MemberId, apply.Password))
                throw new MallException("交易密码错误");

            if (apply.Amount > SiteSettingApplication.SiteSettings.DistributorWithdrawMaxLimit)
                throw new MallException("超过最大提现额限");

            if (apply.Amount < SiteSettingApplication.SiteSettings.DistributorWithdrawMinLimit)
                throw new MallException("小于最低提现额限");

            var distributor = Service.GetDistributor(apply.MemberId);
            if (apply.Amount > distributor.Balance)
                throw new MallException("超过最多提现金额");

            var settings = SiteSettingApplication.SiteSettings;

            if (apply.Type == DistributionWithdrawType.Alipay)
            {
                if (!settings.DistributorWithdrawTypes.ToLower().Contains("alipay"))
                    throw new MallException("暂不支持支付宝提现");
                if (string.IsNullOrEmpty(apply.WithdrawAccount))
                    throw new MallException("支付宝账户不可为空");
                if (string.IsNullOrEmpty(apply.WithdrawName))
                    throw new MallException("真实姓名不可为空");
            }
            else if (apply.Type == DistributionWithdrawType.WeChat)
            {
                if (!settings.DistributorWithdrawTypes.ToLower().Contains("wechat"))
                    throw new MallException("暂不支持微信提现");
                if (string.IsNullOrEmpty(apply.WithdrawAccount))
                    throw new MallException("尚未绑定微信,请先绑定微信账户");
            }

            var info = new DistributionWithdrawInfo
            {
                Amount = apply.Amount,
                WithdrawType = apply.Type,
                MemberId = apply.MemberId,
                WithdrawAccount = apply.WithdrawAccount,
                WithdrawName = apply.WithdrawName
            };
            Service.ApplyWithdraw(info);

            //发送消息
            var member = MemberApplication.GetMember(apply.MemberId);
            var message = new MessageWithDrawInfo();
            message.UserName = member != null ? member.UserName : "";
            message.Amount = info.Amount;
            message.ApplyType = info.WithdrawType.GetHashCode();
            message.ApplyTime = info.ApplyTime;
            message.Remark = info.Remark;
            message.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            Task.Factory.StartNew(() => MessageApplication.SendMessageOnDistributionMemberWithDrawApply(apply.MemberId, message));

            //预付款提现,自动审核
            if (info.WithdrawType == DistributionWithdrawType.Capital)
                AuditingWithdraw(info.Id, "System", "预存款提现,自动审核");

        }

        /// <summary>
        /// 提现审核
        /// </summary>
        public static void AuditingWithdraw(long id, string operatorName, string remark)
        {
            //审核通过
            Service.AuditingWithdraw(id, operatorName, remark);
            //审核通过,自动启动支付流程
            PaymentWithdraw(id);
        }

        /// <summary>
        /// 提现拒绝
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operatorName"></param>
        /// <param name="remark"></param>
        public static void RefusedWithdraw(long withdrawId, string operatorName, string remark)
        {
            Service.RefusedWithdraw(withdrawId, operatorName, remark);
        }
        /// <summary>
        /// 支付提现
        /// </summary>
        public static void PaymentWithdraw(long withdrawId)
        {
            var model = Service.GetWithdraw(withdrawId);
            try
            {
                switch (model.WithdrawType)
                {
                    case DistributionWithdrawType.Alipay:
                        var result = Payment(DistributionWithdrawType.Alipay, model.WithdrawAccount, model.Amount, $"(单号 {withdrawId})", model.Id.ToString(), model.WithdrawName);
                        Service.SuccessWithdraw(withdrawId, result.TradNo.ToString());
                        break;
                    case DistributionWithdrawType.Capital:
                        var no = MemberCapitalApplication.BrokerageTransfer(model.MemberId, model.Amount, $"(单号 {withdrawId})", model.Id.ToString());
                        Service.SuccessWithdraw(withdrawId, no.ToString());
                        break;
                    case DistributionWithdrawType.WeChat:
                        var resultWechat = Payment(DistributionWithdrawType.WeChat, model.WithdrawAccount, model.Amount, $"(单号 {withdrawId})", model.Id.ToString());
                        Service.SuccessWithdraw(withdrawId, resultWechat.TradNo.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                //支付失败(回滚提现状态)
                Service.FailWithdraw(withdrawId, ex.Message);
                throw ex;
            }
            //发送消息
            var member = MemberApplication.GetMember(model.MemberId);
            var message = new MessageWithDrawInfo();
            message.UserName = member != null ? member.UserName : "";
            message.Amount = model.Amount;
            message.ApplyType = model.WithdrawType.GetHashCode();
            message.ApplyTime = model.ApplyTime;
            message.Remark = model.Remark;
            message.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnDistributionMemberWithDrawSuccess(model.MemberId, message));
        }
        /// <summary>
        /// 调用第三方支付
        /// </summary>
        /// <param name="type"></param>
        /// <param name="account"></param>
        /// <param name="amount"></param>
        /// <param name="desc"></param>
        /// <param name="no"></param>
        /// <returns></returns>
        private static PaymentInfo Payment(DistributionWithdrawType type, string account, decimal amount, string desc, string no,string withdrawName="")
        {
            Plugin<IPaymentPlugin> plugin = null;
            /// 支付宝真实姓名验证逻辑
            bool isCheckName = false;
            switch (type)
            {
                case DistributionWithdrawType.Alipay:
                    plugin = PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(e => e.PluginInfo.PluginId == "Mall.Plugin.Payment.Alipay");
                    isCheckName = true;
                    break;
                case DistributionWithdrawType.WeChat:
                    plugin = PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(e => e.PluginInfo.PluginId.ToLower().Contains("weixin"));
                    break;
                default:
                    throw new MallException("不支持的支付类型");
            }
            if (plugin == null)
                throw new MallException("未找到支付插件");

            var pay = new EnterprisePayPara()
            {
                amount = amount,
                openid = account,
                out_trade_no = no,
                check_name = isCheckName,
                re_user_name = withdrawName,
                desc = desc
            };
            try
            {
                return plugin.Biz.EnterprisePay(pay);
            }
            catch (PluginException pex)
            {
                //插件异常，直接返回错误信息
                Log.Error("调用付款接口异常：" + pex.Message);
                throw new MallException("调用企业付款接口异常:" + pex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("付款异常：" + ex.Message);
                throw new MallException("企业付款异常:" + ex.Message);
            }
        }


        /// <summary>
        /// 获取除以100保留两位小数的结果
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static decimal GetDivide100Number(decimal data)
        {
            return decimal.Parse((data / (decimal)100).ToString("f2"));
        }
    }
}
