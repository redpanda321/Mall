using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.Distribution;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mall.Application;
using Mall.Core.Plugins.Message;
using System.Threading.Tasks;

namespace Mall.Service
{
    public class DistributionService : ServiceBase, IDistributionService
    {
        public void AddDistributorGrade(DistributorGradeInfo data)
        {
            DbFactory.Default.Add(data);
        }

        public void AddSpreadProducts(IEnumerable<long> productIds, long shopId)
        {
            int sv = DistributionProductStatus.Normal.GetHashCode();
            if (productIds.Count() < 1)
                throw new MallException("错误的商品编号");
            if (shopId < 0)
                throw new MallException("错误的店铺编号");
            //选出商品id
            var ids = DbFactory.Default.Get<ProductInfo>()
                .Where(p => p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                && p.IsDeleted == false && p.Id.ExIn(productIds) && p.ShopId == shopId)
                .Select(p => p.Id).ToList<long>();
            var normalIds = DbFactory.Default.Get<DistributionProductInfo>().Where(d => d.ProductId.ExIn(ids) && d.ProductStatus == sv)
                .Select(p => p.ProductId).ToList<long>();
            ids = ids.Where(d => !normalIds.Contains(d)).ToList();
            var dbrokerage = GetDefaultBrokerageRate(shopId);
            if (dbrokerage == null)
            {
                throw new MallException("请先设置商家默认分佣比");
            }
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var id in ids)
                {
                    //仅添加与修改
                    var data = DbFactory.Default.Get<DistributionProductInfo>().Where(d => d.ProductId == id).FirstOrDefault();
                    if (data != null)
                    {
                        if (data.ProductStatus != sv)
                        {
                            data.ProductStatus = sv;
                            data.AddDate = DateTime.Now;
                            data.BrokerageRate1 = dbrokerage.BrokerageRate1;
                            data.BrokerageRate2 = dbrokerage.BrokerageRate2;
                            data.BrokerageRate3 = dbrokerage.BrokerageRate3;
                            DbFactory.Default.Update(data);
                        }
                    }
                    else
                    {
                        data = new DistributionProductInfo
                        {
                            ProductId = id,
                            ShopId = shopId,
                            BrokerageRate1 = dbrokerage.BrokerageRate1,
                            BrokerageRate2 = dbrokerage.BrokerageRate2,
                            BrokerageRate3 = dbrokerage.BrokerageRate3,
                            ProductStatus = sv,
                            AddDate = DateTime.Now,
                            SaleAmount = 0,
                            SaleCount = 0,
                            SettlementAmount = 0,
                        };
                        DbFactory.Default.Add(data);
                    }
                }
            });
        }

        public void AddDistributor(DistributorInfo data)
        {
            DbFactory.Default.Add(data);
        }
        public void UpdateDistributor(DistributorInfo data)
        {
            DbFactory.Default.Update(data);
        }
        public int GetDistributorSubNumber(long memberId)
        {
            int result = 0;
            try
            {
                var _tmp = DbFactory.Default.Get<DistributorInfo>()
                    .Where(d => d.SuperiorId == memberId)
                    .Select(d => new { result = d.MemberId.ExCount(false) }).FirstOrDefault<int?>();
                result = _tmp ?? 0;
            }
            catch
            {
                result = 0;
            }
            return result;
        }

        public void SyncDistributorSubNumber(long memberId)
        {
            var subnumber = GetDistributorSubNumber(memberId);
            DbFactory.Default.Set<DistributorInfo>()
                    .Where(d => d.MemberId == memberId)
                    .Set(d => d.SubNumber, subnumber)
                    .Succeed();
        }

        public void DeleteDistributorGrade(long id)
        {
            DbFactory.Default.Del<DistributorGradeInfo>(d => d.Id == id);
            DbFactory.Default.Set<DistributorInfo>()
                .Where(d => d.GradeId == id)
                .Set(d => d.GradeId, v => 0)
                .Succeed();
        }

        /// <summary>
        /// 销售员分销订单集
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>

        public QueryPageModel<OrderInfo> GetDistributorBrokerageOrderList(DistributionBrokerageQuery query)
        {
            var db = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where<DistributionBrokerageInfo>(p => p.BrokerageStatus >= 0)
                .GroupBy(d => d.OrderId);


            //屏蔽不可结算收入

            #region WhereBuilder
            if (query.DistributorId > 0)
            {
                db.Where<DistributionBrokerageInfo>(p => p.SuperiorId1 == query.DistributorId
                            || p.SuperiorId2 == query.DistributorId
                            || p.SuperiorId3 == query.DistributorId);
            }
            #endregion

            db.OrderByDescending(p => p.OrderDate).OrderByDescending(d => d.OrderId);
            db.Select(d => new
            {
                OrderId = d.OrderId,
                MinOrderDate = d.OrderDate.ExMin()
            });
            var datal = db.ToPagedList<Tmp_DistributionOrderGroupModel>(query.PageNo, query.PageSize);
            var result = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id.ExIn(datal.Select(o => o.OrderId).ToList())).ToList();
            return new QueryPageModel<OrderInfo>
            {
                Models = result,
                Total = datal.TotalRecordCount
            };
        }
        /// <summary>
        /// 通过订单项编号获取分佣数据集
        /// </summary>
        /// <param name="orderItemIds"></param>
        /// <returns></returns>
        public List<DistributionBrokerageInfo> GetDistributionBrokerageByOrderItemIds(IEnumerable<long> orderItemIds)
        {
            List<DistributionBrokerageInfo> result = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(d => d.OrderItemId.ExIn(orderItemIds))
                .ToList();
            return result;
        }
        /// <summary>
        /// 通过订单编号获取分佣数据集
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public List<DistributionBrokerageInfo> GetDistributionBrokerageByOrderIds(IEnumerable<long> orderIds)
        {
            List<DistributionBrokerageInfo> result = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(d => d.OrderId.ExIn(orderIds))
                .ToList();
            return result;
        }

        /// <summary>
        /// 获取分销订单业绩查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributionBrokerageInfo> GetBrokerageOrderBuilder(BrokerageOrderQuery query)
        {
            var db = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .LeftJoin<OrderInfo>((d, o) => d.OrderId == o.Id)
                .LeftJoin<ProductInfo>((d, p) => d.ProductId == p.Id)
                .Where(p => p.BrokerageStatus >= 0);//屏蔽不可结算收入

            #region WhereBuilder
            if (query.OrderId > 0)
                db.Where(p => p.OrderId == query.OrderId);

            if (query.ShopId > 0)
                db.Where(p => p.ShopId == query.ShopId);
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                var shops = DbFactory.Default.Get<ShopInfo>().Where(p => p.ShopName.Contains(query.ShopName)).Select(p => p.Id);
                db.Where(p => p.ShopId.ExIn(shops));
            }

            if (query.ProductId > 0)
                db.Where(p => p.ProductId == query.ProductId);
            if (!string.IsNullOrEmpty(query.ProductName))
                db.Where<ProductInfo>(p => p.ProductName.Contains(query.ProductName));

            if (query.OrderStatus.HasValue)
                db.Where<OrderInfo>(p => p.OrderStatus == query.OrderStatus.Value);

            if (query.DistributorId > 0)
            {
                db.Where(p => p.SuperiorId1 == query.DistributorId
                            || p.SuperiorId2 == query.DistributorId
                            || p.SuperiorId3 == query.DistributorId);
            }
            if (!string.IsNullOrWhiteSpace(query.DistributorName))
            {
                var users = DbFactory.Default.Get<MemberInfo>().Where(p => p.UserName.Contains(query.DistributorName)).Select(p => p.Id).ToList<long>();
                db.Where(p => p.SuperiorId1.ExIn(users)
                            || p.SuperiorId2.ExIn(users)
                            || p.SuperiorId3.ExIn(users));
            }
            if (query.SettlementBegin.HasValue)
                db.Where(p => p.SettlementTime >= query.SettlementBegin.Value);
            if (query.SettlementEnd.HasValue)
                db.Where(p => p.SettlementTime < query.SettlementEnd.Value);
            if (query.SettlementStatus.HasValue)
                db.Where(p => p.BrokerageStatus == query.SettlementStatus.Value);
            #endregion

            db.Select(p => new
            {
                p.OrderId,
                p.ShopId,
                p.ProductId,
                p.SettlementTime,
                Status = p.BrokerageStatus,
                p.SuperiorId1,
                p.SuperiorId2,
                p.SuperiorId3,
                Brokerage1 = p.BrokerageRate1 * p.RealPayAmount / 100,
                Brokerage2 = p.BrokerageRate2 * p.RealPayAmount / 100,
                Brokerage3 = p.BrokerageRate3 * p.RealPayAmount / 100
            })
            .Select<OrderInfo>(p => p.OrderStatus)
            .Select<ProductInfo>(p => p.ProductName)
            .OrderByDescending(p => p.Id);
            return db;
        }

        public QueryPageModel<BrokerageOrder> GetBrokerageOrders(BrokerageOrderQuery query)
        {
            var db = GetBrokerageOrderBuilder(query);

            var result = db.ToPagedList<BrokerageOrder>(query.PageNo, query.PageSize);
            return new QueryPageModel<BrokerageOrder>
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        /// <summary>
        /// 分销订单列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<BrokerageOrder> GetBrokerageOrdersAll(BrokerageOrderQuery query)
        {
            var db = GetBrokerageOrderBuilder(query);
            return db.ToList<BrokerageOrder>();
        }

        /// <summary>
        /// 获取分销明细查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributionBrokerageInfo> GetBrokerageProductBuilder(BrokerageProductQuery query)
        {
            var db = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .LeftJoin<DistributionProductInfo>((b, p) => b.ProductId == p.ProductId)
                .LeftJoin<ProductInfo>((b, p) => b.ProductId == p.Id)
                .GroupBy(p => p.ProductId);

            if (query.ShopId > 0)
                db.Where(p => p.ShopId == query.ShopId);

            if (query.Status.HasValue)
                db.Where<DistributionProductInfo>(p => p.ProductStatus == (int)query.Status.Value);

            if (!string.IsNullOrWhiteSpace(query.ProductName))
            {
                db.Where<ProductInfo>(p => p.ProductName.Contains(query.ProductName));
            }

            #region SelectBuilder

            db.Select(p => new
            {
                p.ShopId,
                p.ProductId,
                Quantity = p.Quantity.ExSum(),
                Amount = p.RealPayAmount.ExSum(),
                Settlement = "sum(case when BrokerageStatus = 1 then RealPayAmount*(Mall_DistributionBrokerage.BrokerageRate1+Mall_DistributionBrokerage.BrokerageRate2+Mall_DistributionBrokerage.BrokerageRate3)/100 else 0 end)".ExSql(),
                NoSettlement = "sum(case when BrokerageStatus = 0 then RealPayAmount*(Mall_DistributionBrokerage.BrokerageRate1+Mall_DistributionBrokerage.BrokerageRate2+Mall_DistributionBrokerage.BrokerageRate3)/100 else 0 end)".ExSql()
            })
            .Select<DistributionProductInfo>(p => p.ProductStatus)
            .Select<ProductInfo>(p => p.ProductName);
            #endregion

            #region OrderBuilder
            switch (query.Sort.ToLower())
            {
                case "quantity":
                    if (query.IsAsc) db.OrderBy(p => "quantity");
                    else db.OrderByDescending(p => "quantity");
                    break;
                case "amount":
                    if (query.IsAsc) db.OrderBy(p => "Amount");
                    else db.OrderByDescending(p => "Amount");
                    break;
                case "settlement":
                    if (query.IsAsc) db.OrderBy(p => "Settlement");
                    else db.OrderByDescending(p => "Settlement");
                    break;
                case "nosettlement":
                    if (query.IsAsc) db.OrderBy(p => "NoSettlement");
                    else db.OrderByDescending(p => "NoSettlement");
                    break;
                default:
                    db.OrderByDescending(p => p.ProductId);
                    break;
            }
            #endregion

            return db;
        }

        public QueryPageModel<BrokerageProduct> GetBrokerageProduct(BrokerageProductQuery query)
        {
            var db = GetBrokerageProductBuilder(query);

            var result = db.ToPagedList<BrokerageProduct>(query.PageNo, query.PageSize);
            return new QueryPageModel<BrokerageProduct>
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        /// <summary>
        /// 获取分销明细列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<BrokerageProduct> GetBrokerageProductAll(BrokerageProductQuery query)
        {
            var db = GetBrokerageProductBuilder(query);
            return db.ToList<BrokerageProduct>();
        }

        public DistributorGradeInfo GetDistributorGrade(long id)
        {
            return DbFactory.Default.Get<DistributorGradeInfo>().Where(d => d.Id == id).FirstOrDefault();
        }

        public List<DistributorGradeInfo> GetDistributorGrades(bool IsAvailable = false)
        {
            int dasv = DistributorStatus.Audited.GetHashCode();
            int status = DistributorStatus.NotAvailable.GetHashCode();
            var sql = DbFactory.Default.Get<DistributorGradeInfo>();
            var datas = DbFactory.Default.Get<DistributorInfo>();
            if (IsAvailable)
                datas = datas.Where(d => d.DistributionStatus == dasv || d.DistributionStatus == status);
            else
                datas = datas.Where(d => d.DistributionStatus == dasv);
            var groupdata = datas
                .GroupBy(d => d.GradeId)
                .Select(d => new
                {
                    GradeId = d.GradeId,
                    MemberCount = d.MemberId.ExCount(false)
                }).ToList<Tmp_DistributorGrabeGroupModel>();
            var data = sql.ToList();
            foreach (var item in groupdata)
            {
                var _tmp = data.FirstOrDefault(d => d.Id == item.GradeId);
                if (_tmp != null)
                {
                    _tmp.MemberCount = item.MemberCount;
                }
            }
            data = data.OrderBy(d => d.Quota).ToList();
            return data;
        }
        /// <summary>
        /// 是否己存在同名等级
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ExistDistributorGradesName(string name, long id)
        {
            return DbFactory.Default.Get<DistributorGradeInfo>().Where(d => d.GradeName == name && d.Id != id).Exist();
        }
        /// <summary>
        /// 是否己存在同条件等级
        /// </summary>
        /// <param name="quota"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ExistDistributorGradesQuota(decimal quota, long id)
        {
            return DbFactory.Default.Get<DistributorGradeInfo>().Where(d => d.Quota == quota && d.Id != id).Exist();
        }

        public DistributorInfo GetDistributor(long memberId)
        {
            var dbsql = DbFactory.Default.Get<DistributorInfo>()
                .LeftJoin<MemberInfo>((dm, m) => dm.MemberId == m.Id)
                .LeftJoin<DistributorGradeInfo>((dm, dg) => dm.GradeId == dg.Id)
                .Where(d => d.MemberId == memberId);
            dbsql.Select();
            dbsql.Select<MemberInfo>(d => new
            {
                MemberName = d.UserName,
                RegDate = d.CreateDate
            });
            dbsql.Select<DistributorGradeInfo>(d => new
            {
                GradeName = d.GradeName
            });
            var result = dbsql.FirstOrDefault();
            //补数据
            if (result != null)
            {
                var _sm = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id == result.SuperiorId).FirstOrDefault();
                result.SuperiorMemberName = "平台";
                if (_sm != null)
                {
                    result.SuperiorMemberName = _sm.UserName;
                }

                result.SubNumber2 = 0;
                try
                {
                    var _tmp = DbFactory.Default.Get<DistributorInfo>()
                        .Where(d => d.SuperiorId == result.MemberId)
                        .Select(d => d.SubNumber.ExSum())
                        .FirstOrDefault<int?>();
                    result.SubNumber2 = _tmp ?? 0;
                }
                catch
                {
                    result.SubNumber2 = 0;
                }
                result.SubNumber3 = 0;
                try
                {
                    var sm2 = DbFactory.Default.Get<DistributorInfo>()
                        .Where(d => d.SuperiorId == result.MemberId)
                        .Select(d => d.MemberId);
                    var _tmp = DbFactory.Default.Get<DistributorInfo>()
                        .Where(d => d.SuperiorId.ExIn(sm2))
                        .Select(d => d.SubNumber.ExSum())
                        .FirstOrDefault<int?>();
                    result.SubNumber3 = _tmp ?? 0;
                }
                catch
                {
                    result.SubNumber3 = 0;
                }
            }

            return result;
        }


        public DistributorInfo GetDistributorBase(long member)
        {
            return DbFactory.Default.Get<DistributorInfo>(p => p.MemberId == member).FirstOrDefault();
        }

        public decimal GetNoSettlementAmount(long member)
        {
            var noSettlementSum = string.Format("Round((case "
                        + "when SuperiorId1 = {0} then BrokerageRate1 "
                        + "when SuperiorId2 = {0} then BrokerageRate2 "
                        + "when SuperiorId3 = {0} then BrokerageRate3 "
                        + "else 0 end) * RealPayAmount / 100,2)", member);

            return DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(p => p.BrokerageStatus == DistributionBrokerageStatus.NotSettled)
                .Where(p => p.SuperiorId1 == member || p.SuperiorId2 == member || p.SuperiorId3 == member)
               .Sum<decimal>(p => noSettlementSum.ExSql());
        }


        /// <summary>
        /// 获取响应层级下属
        /// </summary>
        /// <param name="member"></param>
        /// <param name="level"></param>
        /// <returns><level,[members]></returns>
        public Dictionary<int, List<long>> GetSubordinate(long member, int level)
        {
            var result = new Dictionary<int, List<long>>();
            if (level >= 1)
            {
                var first = DbFactory.Default.Get<DistributorInfo>(p => p.SuperiorId == member).Select(p => p.MemberId).ToList<long>();
                result.Add(1, first);
            }
            if (level >= 2)
            {
                var second = DbFactory.Default.Get<DistributorInfo>(p => p.SuperiorId.ExIn(result[1])).Select(p => p.MemberId).ToList<long>();
                result.Add(2, second);
            }
            if (level >= 3)
            {
                var third = DbFactory.Default.Get<DistributorInfo>(p => p.SuperiorId.ExIn(result[2])).Select(p => p.MemberId).ToList<long>();
                result.Add(3, third);
            }
            return result;
        }
        /// <summary>
        /// 获取下级业绩
        /// </summary>
        /// <returns></returns>
        public DistributionAchievement GetSubAchievement(List<long> subs)
        {
            return DbFactory.Default.Get<DistributionBrokerageInfo>(p => p.MemberId.ExIn(subs))
                .Select(p => new
                {
                    TotalCount = p.Quantity.ExSum(),
                    TotalAmount = p.RealPayAmount.ExSum()
                }).FirstOrDefault<DistributionAchievement>();
        }
        /// <summary>
        /// 获取下级业绩
        /// </summary>
        /// <returns></returns>
        public DistributionAchievement GetSubAchievement(long memberId)
        {
            return DbFactory.Default.Get<DistributionBrokerageInfo>(p => (
            (p.SuperiorId1== memberId && p.BrokerageRate1>0)
            || (p.SuperiorId2 == memberId && p.BrokerageRate2 > 0)
            || (p.SuperiorId3 == memberId && p.BrokerageRate3 > 0))
            && p.BrokerageStatus!= DistributionBrokerageStatus.NotAvailable
            )
                .Select(p => new
                {
                    TotalCount = p.Quantity.ExSum(),
                    TotalAmount = p.RealPayAmount.ExSum()
                }).FirstOrDefault<DistributionAchievement>();
        }

        /// <summary>
        /// 获取会员业绩
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public List<DistributionAchievement> GetAchievement(List<long> members)
        {
            return DbFactory.Default.Get<DistributionBrokerageInfo>(p => p.MemberId.ExIn(members))
                .Where(d => d.BrokerageStatus != DistributionBrokerageStatus.NotAvailable)
                .GroupBy(p => p.MemberId)
                .Select(p => new
                {
                    MemberId = p.MemberId,
                    TotalCount = p.Quantity.ExSum(),
                    TotalAmount = p.RealPayAmount.ExSum()
                }).ToList<DistributionAchievement>();
        }
        /// <summary>
        /// 获得最后一次生成的报表
        /// </summary>
        /// <returns></returns>
        public DistributionRankingBatchInfo GetLastRankingBatch()
        {
            return DbFactory.Default.Get<DistributionRankingBatchInfo>().OrderByDescending(p => p.CreateTime).FirstOrDefault();
        }

        private class GenerateItem
        {
            /// <summary>
            /// 销售员
            /// </summary>
            public long MemberId { get; set; }
            /// <summary>
            /// 成交数量
            /// </summary>
            public long Quantity { get; set; }
            /// <summary>
            /// 成交金额
            /// </summary>
            public decimal Amount { get; set; }
            /// <summary>
            /// 佣金
            /// </summary>
            public decimal Settlement { get; set; }
            public decimal NoSettlement { get; set; }
        }
        public void GenerateRanking(DateTime beginDate, DateTime endDate)
        {
            var batch = new DistributionRankingBatchInfo
            {
                BeginTime = beginDate.Date,
                EndTime = endDate.Date,
                CreateTime = DateTime.Now
            };
            DbFactory.Default.Add(batch);//插入批次

            var begin = beginDate.Date;
            var end = endDate.Date.AddDays(1);

            //查询三级分销佣金
            var level1 = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(p => p.OrderDate >= begin && p.OrderDate < end && p.SuperiorId1 > 0 && p.BrokerageStatus >= 0 && p.BrokerageRate1 > 0)
                .GroupBy(p => p.SuperiorId1)
                .Select(p => new
                {
                    MemberId = p.SuperiorId1,
                    Quantity = p.Quantity.ExSum(),
                    Amount = p.RealPayAmount.ExSum(),
                    NoSettlement = "sum(case when BrokerageStatus = 0 then ROUND(RealPayAmount*BrokerageRate1/100,2) else 0 end)".ExSql(),
                    Settlement = "sum(case when BrokerageStatus = 1 then ROUND(RealPayAmount*BrokerageRate1/100,2) else 0 end)".ExSql()
                }).ToList<GenerateItem>();

            var level2 = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(p => p.OrderDate >= begin && p.OrderDate < end && p.SuperiorId2 > 0 && p.BrokerageStatus >= 0 && p.BrokerageRate2 > 0)
                .GroupBy(p => p.SuperiorId2)
                .Select(p => new
                {
                    MemberId = p.SuperiorId2,
                    Quantity = p.Quantity.ExSum(),
                    Amount = p.RealPayAmount.ExSum(),
                    NoSettlement = "sum(case when BrokerageStatus = 0 then ROUND(RealPayAmount*BrokerageRate2/100,2) else 0 end)".ExSql(),
                    Settlement = "sum(case when BrokerageStatus = 1 then ROUND(RealPayAmount*BrokerageRate2/100,2) else 0 end)".ExSql()
                }).ToList<GenerateItem>();

            var level3 = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(p => p.OrderDate >= begin && p.OrderDate < end && p.SuperiorId3 > 0 && p.BrokerageStatus >= 0 && p.BrokerageRate3 > 0)
                .GroupBy(p => p.SuperiorId3)
                .Select(p => new
                {
                    MemberId = p.SuperiorId3,
                    Quantity = p.Quantity.ExSum(),
                    Amount = p.RealPayAmount.ExSum(),
                    NoSettlement = "sum(case when BrokerageStatus = 0 then ROUND(RealPayAmount*BrokerageRate3/100,2) else 0 end)".ExSql(),
                    Settlement = "sum(case when BrokerageStatus = 1 then ROUND(RealPayAmount*BrokerageRate3/100,2) else 0 end)".ExSql()
                }).ToList<GenerateItem>();
            foreach(var item in level1)
            {
                item.Settlement= decimal.Parse(item.Settlement.ToString("f2"));
                item.NoSettlement = decimal.Parse(item.NoSettlement.ToString("f2"));
            }
            foreach (var item in level2)
            {
                item.Settlement = decimal.Parse(item.Settlement.ToString("f2"));
                item.NoSettlement = decimal.Parse(item.NoSettlement.ToString("f2"));
            }
            foreach (var item in level3)
            {
                item.Settlement = decimal.Parse(item.Settlement.ToString("f2"));
                item.NoSettlement = decimal.Parse(item.NoSettlement.ToString("f2"));
            }
            //合并三级分销佣金
            var list = new List<GenerateItem>();
            list.AddRange(level1);
            list.AddRange(level2);
            list.AddRange(level3);
            var result = list.GroupBy(p => p.MemberId).Select(item => new DistributionRankingInfo
            {
                MemberId = item.Key,
                BatchId = batch.Id,
                Amount = item.Sum(p => p.Amount),
                Quantity = (int)item.Sum(p => p.Quantity),
                Settlement = item.Sum(p => p.Settlement),
                NoSettlement = item.Sum(p => p.NoSettlement)
            }).ToList();

            DbFactory.Default.AddRange(result);
        }
        public void RemoveRankingBatch(List<long> batchs)
        {
            DbFactory.Default.Del<DistributionRankingBatchInfo>(p => p.Id.ExIn(batchs));
            DbFactory.Default.Del<DistributionRankingInfo>(p => p.BatchId.ExIn(batchs));
        }

        public List<DistributionRankingBatchInfo> GetRankingBatchs()
        {
            return DbFactory.Default.Get<DistributionRankingBatchInfo>().ToList();
        }

        /// <summary>
        /// 获取销售员排行查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributionRankingInfo> GetRankingsBuilder(DistributorRankingQuery query)
        {
            var db = DbFactory.Default.Get<DistributionRankingInfo>()
                .Where(p => p.BatchId == query.BatchId);

            switch (query.Sort.ToLower())
            {
                case "quantity":
                    if (query.IsAsc) db.OrderBy(p => p.Quantity);
                    else db.OrderByDescending(p => p.Quantity);
                    break;
                case "amount":
                    if (query.IsAsc) db.OrderBy(p => p.Amount);
                    else db.OrderByDescending(p => p.Amount);
                    break;
                case "settlement":
                    if (query.IsAsc) db.OrderBy(p => p.Settlement);
                    else db.OrderByDescending(p => p.Settlement);
                    break;
                case "nosettlement":
                    if (query.IsAsc) db.OrderBy(p => p.NoSettlement);
                    else db.OrderByDescending(p => p.NoSettlement);
                    break;
                default:
                    db.OrderByDescending(p => p.Amount);
                    break;
            }

            return db;
        }

        public QueryPageModel<DistributionRankingInfo> GetRankings(DistributorRankingQuery query)
        {

            var db = GetRankingsBuilder(query);

            var data = db.ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<DistributionRankingInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        /// <summary>
        /// 获取排行数据（忽略分页）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DistributionRankingInfo> GetRankingsAll(DistributorRankingQuery query)
        {
            var db = GetRankingsBuilder(query);

            return db.ToList();
        }


        /// <summary>
        /// 获取销售员查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributorInfo> GetDistributorsBuilder(DistributorQuery query)
        {
            var dbsql = DbFactory.Default.Get<DistributorInfo>()
                .LeftJoin<MemberInfo>((dm, m) => dm.MemberId == m.Id)
                .LeftJoin<DistributorGradeInfo>((dm, dg) => dm.GradeId == dg.Id);
            if (!query.IsIncludeMember)
            {
                var _s = DistributorStatus.UnApply.GetHashCode();
                dbsql.Where(d => d.DistributionStatus > _s);
            }
            if (query.SuperiorMemberId.HasValue)
            {
                if (query.Level.HasValue)
                {
                    var secondmemids = DbFactory.Default.Get<DistributorInfo>()
                        .Where(d => d.SuperiorId == query.SuperiorMemberId.Value)
                        .Select(d => d.MemberId);
                    var thirdmemids = DbFactory.Default.Get<DistributorInfo>()
                        .Where(d => d.SuperiorId.ExIn(secondmemids))
                        .Select(d => d.MemberId);
                    switch (query.Level)
                    {
                        case 1:
                            dbsql.Where(d => d.SuperiorId == query.SuperiorMemberId.Value);
                            break;
                        case 2:
                            dbsql.Where(d => d.SuperiorId.ExIn(secondmemids));
                            break;
                        case 3:
                            dbsql.Where(d => d.SuperiorId.ExIn(thirdmemids));
                            break;
                    }
                }
            }
            if (query.StartTime.HasValue)
            {
                dbsql.Where(d => d.ApplyTime >= query.StartTime.Value);
            }
            if (query.EndTime.HasValue)
            {
                dbsql.Where(d => d.ApplyTime <= query.EndTime.Value);
            }
            if (query.GradeId.HasValue)
            {
                dbsql.Where(d => d.GradeId == query.GradeId.Value);
            }
            if (query.Status.HasValue)
            {
                var _s = query.Status.Value.GetHashCode();
                dbsql.Where(d => d.DistributionStatus == _s);
            }
            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                dbsql.Where(d => d.ShopName.Contains(query.ShopName));
            }
            if (!string.IsNullOrWhiteSpace(query.MemberName))
            {
                dbsql.Where<MemberInfo>(d => d.UserName.Contains(query.MemberName));
            }
            if (query.ExcludeMemberIds != null && query.ExcludeMemberIds.Count > 0)
            {
                dbsql.Where(d => d.MemberId.ExNotIn(query.ExcludeMemberIds));
            }
            if (!string.IsNullOrWhiteSpace(query.SuperiorMemberName))
            {
                var ssql = new List<long>();
                if (query.SuperiorMemberName.Equals("平台"))
                    ssql.Add(0);
                else
                    ssql = DbFactory.Default.Get<MemberInfo>().Where<MemberInfo>(d => d.UserName.Contains(query.SuperiorMemberName)).Select(d => d.Id).ToList<long>();
                dbsql.Where(d => d.SuperiorId.ExIn(ssql));
            }
            dbsql.Select();
            dbsql.Select<MemberInfo>(d => new
            {
                MemberName = d.UserName,
                RegDate = d.CreateDate
            });
            dbsql.Select<DistributorGradeInfo>(d => new
            {
                GradeName = d.GradeName
            });

            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                string sort = query.Sort;
                switch (query.Sort)
                {
                    case "SettlementAmount":
                        if (query.IsAsc)
                        {
                            dbsql.OrderBy(o => "SettlementAmount");
                            dbsql.OrderByDescending(o => o.ApplyTime);
                        }
                        else
                        {
                            dbsql.OrderByDescending(o => "SettlementAmount");
                            dbsql.OrderByDescending(o => o.ApplyTime);
                        }
                        break;
                    case "ApplyTime":
                        if (query.IsAsc)
                        {
                            dbsql.OrderBy(o => "ApplyTime");
                        }
                        else
                        {
                            dbsql.OrderByDescending(o => "ApplyTime");
                        }
                        break;
                }
            }
            else
            {
                dbsql.OrderByDescending(o => o.ApplyTime);
            }

            return dbsql;
        }

        /// <summary>
        /// 营销员上级销售员、下架发展等数据提充处理
        /// </summary>
        /// <param name="result"></param>
        private void SetDistributorsInfo(List<DistributorInfo> result)
        {
            List<long> smids = result.Select(d => d.SuperiorId).ToList();
            var allsuper = DbFactory.Default.Get<MemberInfo>().Where(d => d.Id.ExIn(smids)).ToList();
            List<Tmp_DistributorSubNumberGroupModel> mcount = new List<Tmp_DistributorSubNumberGroupModel>();

            List<long> mids = result.Select(d => d.MemberId).ToList();
            if (mids != null && mids.Count > 0)
            {
                var csql = @"select MemberId,d.SuperiorId as SuperiorId,d.SubNumber as SubNumber,g.SubNumber2 as SubNumber2 from Mall_Distributor as d
                        LEFT JOIN(
                        SELECT SuperiorId, sum(SubNumber) SubNumber2
                        FROM Mall_Distributor
                        WHERE SuperiorId IN (SELECT MemberId FROM `Mall_Distributor` WHERE SuperiorId in ({sids}))
                        GROUP BY SuperiorId
                        ) as g
                        on d.MemberId = g.SuperiorId
                        WHERE d.SuperiorId > 0";
                csql = csql.Replace("{sids}", string.Join(",", mids));
                mcount = DbFactory.Default.Query<Tmp_DistributorSubNumberGroupModel>(csql).ToList();
            }
            foreach (var item in result)
            {
                var _sm = allsuper.FirstOrDefault(d => d.Id == item.SuperiorId);
                item.SuperiorMemberName = "平台";
                if (_sm != null)
                {
                    item.SuperiorMemberName = _sm.UserName;
                }

                item.SubNumber2 = 0;
                try
                {
                    item.SubNumber2 = mcount.Where(d => d.SuperiorId == item.MemberId).Sum(d => d.SubNumber);
                }
                catch
                {
                    item.SubNumber2 = 0;
                }
                item.SubNumber3 = 0;
                try
                {
                    item.SubNumber3 = mcount.Where(d => d.SuperiorId == item.MemberId).Sum(d => d.SubNumber2);
                }
                catch
                {
                    item.SubNumber3 = 0;
                }
            }
        }

        public QueryPageModel<DistributorInfo> GetDistributors(DistributorQuery query)
        {
            var dbsql = GetDistributorsBuilder(query);

            var result = dbsql.ToPagedList<DistributorInfo>(query.PageNo, query.PageSize);
            SetDistributorsInfo(result);//设置提充值

            return new QueryPageModel<DistributorInfo>()
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        /// <summary>
        /// 获取销售员列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DistributorInfo> GetDistributorsAll(DistributorQuery query)
        {
            var dbsql = GetDistributorsBuilder(query);

            var result = dbsql.ToList();
            SetDistributorsInfo(result);//设置提充值

            return result;
        }

        public List<DistributorInfo> GetDistributors(List<long> members)
        {
            return DbFactory.Default.Get<DistributorInfo>().Where(p => p.MemberId.ExIn(members)).ToList();
        }


        /// <summary>
        /// 获取佣金管理列表查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributorInfo> GetNewDistributorsBuilder(DistributorSubQuery query)
        {
            var db = DbFactory.Default.Get<DistributorInfo>();

            if (!query.IsAll)
                db.Where(p => p.DistributionStatus == (int)DistributorStatus.Audited);

            if (!string.IsNullOrEmpty(query.MemberName))
            {
                var members = DbFactory.Default.Get<MemberInfo>(p => p.UserName.Contains(query.MemberName)).Select(p => p.Id);
                db.Where(p => p.MemberId.ExIn(members));
            }
            if (!string.IsNullOrEmpty(query.ShopName))
                db.Where(p => p.ShopName.Contains(query.ShopName));

            if (query.Members != null)
                db.Where(p => p.MemberId.ExIn(query.Members));

            //复杂属性，显性包含
            if (query.IncludeNoSettlementAmount)
            {
                var noSettlementSql = "(select sum(ROUND(case when SuperiorId1 = Mall_Distributor.MemberId then brokeragerate1 when SuperiorId2 = Mall_Distributor.MemberId then brokeragerate2 when SuperiorId3 = Mall_Distributor.MemberId then brokeragerate3  else 0 end * RealPayAmount/100,2)) from Mall_DistributionBrokerage where BrokerageStatus = 0)";
                db.Select(p => new { NoSettlementAmount = noSettlementSql.ExSql() });
            }

            db.Select();
            switch (query.Sort.ToLower())
            {
                case "withdrawalsamount":
                    if (query.IsAsc) db.OrderBy(p => p.WithdrawalsAmount);
                    else db.OrderByDescending(p => p.WithdrawalsAmount);
                    break;
                case "freezeamount":
                    if (query.IsAsc) db.OrderBy(p => p.FreezeAmount);
                    else db.OrderByDescending(p => p.FreezeAmount);
                    break;
                case "balance":
                    if (query.IsAsc) db.OrderBy(p => p.Balance);
                    else db.OrderByDescending(p => p.Balance);
                    break;
                case "settlementamount":
                    if (query.IsAsc) db.OrderBy(p => p.SettlementAmount);
                    else db.OrderByDescending(p => p.SettlementAmount);
                    break;
                case "nosettlementamount":
                    if (query.IsAsc) db.OrderBy(p => "NoSettlementAmount");
                    else db.OrderByDescending(p => "NoSettlementAmount");
                    break;
                default:
                    db.OrderByDescending(p => p.MemberId);
                    break;
            }

            return db;
        }

        public QueryPageModel<DistributorInfo> GetNewDistributors(DistributorSubQuery query)
        {
            var db = GetNewDistributorsBuilder(query);

            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<DistributorInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        /// <summary>
        /// 获取佣金管理列表(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DistributorInfo> GetNewDistributorsAll(DistributorSubQuery query)
        {
            return GetNewDistributorsBuilder(query).ToList();
        }

        public DistributionProduct GetProduct(long productId)
        {

            var db = DbFactory.Default.Get<DistributionProductInfo>()
                .LeftJoin<ProductInfo>((pb, p) => pb.ProductId == p.Id)
                .LeftJoin<ShopInfo>((pb, s) => pb.ShopId == s.Id)
                .Where<ProductInfo>(p => p.IsDeleted == false && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited && p.Id == productId);

            db.Select(d => new
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ShopId = d.ShopId,
                ProductStatus = d.ProductStatus,
                BrokerageRate1 = d.BrokerageRate1,
                BrokerageRate2 = d.BrokerageRate2,
                BrokerageRate3 = d.BrokerageRate3,
                SaleCount = d.SaleCount,
                SaleAmount = d.SaleAmount,
                SettlementAmount = d.SettlementAmount
            });
            db.Select<ProductInfo>(d => new
            {
                ProductName = d.ProductName,
                MinSalePrice = d.MinSalePrice,
                ImagePath = d.ImagePath,
                ShortDescription = d.ShortDescription
            });
            db.Select<ShopInfo>(d => new
            {
                ShopName = d.ShopName
            });

            var result = db.FirstOrDefault<DistributionProduct>();
            if (result == null)
                return null;

            #region 补数据
            var notSettlementAmount = DbFactory.Default
                .Get<DistributionBrokerageInfo>()
                .Where(d => d.ProductId == productId && d.BrokerageStatus == DistributionBrokerageStatus.NotSettled)
                .Select(d => ((d.BrokerageRate1 + d.BrokerageRate2 + d.BrokerageRate3) * d.RealPayAmount).ExSum())
                .FirstOrDefault<decimal?>();
            var maxprice = DbFactory.Default.Get<SKUInfo>()
                .Where(d => d.ProductId == productId)
                .Select(d => d.SalePrice.ExMax())
                .FirstOrDefault<decimal?>();
            var lmaxprice = DbFactory.Default.Get<ProductLadderPriceInfo>()
                .Where(d => d.ProductId == productId)
                .Select(d => d.Price.ExMax())
                .FirstOrDefault<decimal?>();
            result.NoSettlementAmount = GetDivide100Number(notSettlementAmount ?? 0);
            result.MaxSalePrice = maxprice ?? 0;
            if (lmaxprice.HasValue && lmaxprice > result.MaxSalePrice)
            {
                result.MaxSalePrice = lmaxprice.Value;
            }
            result.MaxDistributionLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel;
            #endregion

            return result;
        }

        /// <summary>
        /// 获取分销明细查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributionProductInfo> GetProductsBuilder(DistributionProductQuery query)
        {
            var notSettlementAmountSql = DbFactory.Default
                .Get<DistributionBrokerageInfo>()
                .Where<DistributionProductInfo>((dbi, dpi) => dbi.ProductId == dpi.ProductId && dbi.BrokerageStatus == DistributionBrokerageStatus.NotSettled)
                .Select(d => new { NoResult = ((d.BrokerageRate1 + d.BrokerageRate2 + d.BrokerageRate3) * d.RealPayAmount / 100).ExSum() });

            var db = DbFactory.Default.Get<DistributionProductInfo>()
                .LeftJoin<ProductInfo>((pb, p) => pb.ProductId == p.Id)
                .LeftJoin<ShopInfo>((pb, s) => pb.ShopId == s.Id)
                .Where<ProductInfo>(p => p.IsDeleted == false && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale
                && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited);
            //搜索条件
            if (query.Status.HasValue)
            {
                int sv = query.Status.Value.GetHashCode();
                db.Where(p => p.ProductStatus == sv);
            }

            if (query.ShopId.HasValue)
            {
                db.Where(p => p.ShopId == query.ShopId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.ProductName))
            {
                db.Where<ProductInfo>(p => p.ProductName.Contains(query.ProductName));
            }

            if (!string.IsNullOrWhiteSpace(query.ShopName))
            {
                db.Where<ShopInfo>(p => p.ShopName.Contains(query.ShopName));
            }


            if (!string.IsNullOrEmpty(query.ProductIds))
            {
                var products = query.ProductIds.Split(',').Select(e => long.Parse(e)).ToList();
                db.Where(p => p.ProductId.ExIn(products));
            }

            if (query.CategoryId.HasValue && query.CategoryId != 0)
            {
                if (!query.isShopCategory)
                {
                    if (query.Categories != null && query.Categories.Count > 0)
                    {
                        db.Where<ProductInfo>(p => p.CategoryId.ExIn(query.Categories));
                    }
                }
                else
                {
                    var shopCategoryId = query.CategoryId.Value;
                    var shopCategorys = DbFactory.Default.Get<ShopCategoryInfo>().Where(p => p.Id == shopCategoryId || p.ParentCategoryId == shopCategoryId).Select(p => p.Id).ToList<long>();
                    var products = DbFactory.Default.Get<ProductShopCategoryInfo>().Where(p => p.ShopCategoryId.ExIn(shopCategorys)).Select(p => p.ProductId).ToList<long>();
                    db.Where(p => p.ProductId.ExIn(products));
                }
            }

            db.Select(d => new
            {
                Id = d.Id,
                ProductId = d.ProductId,
                ShopId = d.ShopId,
                ProductStatus = d.ProductStatus,
                BrokerageRate1 = d.BrokerageRate1,
                BrokerageRate2 = d.BrokerageRate2,
                BrokerageRate3 = d.BrokerageRate3,
                SaleCount = d.SaleCount,
                SaleAmount = d.SaleAmount,
                SettlementAmount = d.SettlementAmount,
                NoSettlementAmount = @"IFNULL((SELECT SUM(
ROUND(`Mall_DistributionBrokerage`.`BrokerageRate1` * `Mall_DistributionBrokerage`.`RealPayAmount` / 100,2)
+ROUND(`Mall_DistributionBrokerage`.`BrokerageRate2` * `Mall_DistributionBrokerage`.`RealPayAmount` / 100,2)
+ROUND(`Mall_DistributionBrokerage`.`BrokerageRate3` * `Mall_DistributionBrokerage`.`RealPayAmount` / 100,2)) AS NoResult
            FROM
				`Mall_DistributionBrokerage`
			WHERE
                (
						`Mall_DistributionBrokerage`.`ProductId` = `Mall_DistributionProduct`.`ProductId`  
						AND `Mall_DistributionBrokerage`.`BrokerageStatus` = 0
				)
		),0)".ExSql(),
            });
            db.Select<ProductInfo>(d => new
            {
                ProductName = d.ProductName,
                MinSalePrice = d.MinSalePrice,
                ImagePath = d.ImagePath,
                ShortDescription = d.ShortDescription
            });
            db.Select<ShopInfo>(d => new
            {
                ShopName = d.ShopName
            });
            if (!string.IsNullOrWhiteSpace(query.Sort))
            {
                string sort = query.Sort.ToLower();
                switch (sort)
                {
                    case "salecount":
                        if (query.IsAsc)
                        {
                            db.OrderBy(o => "SaleCount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        else
                        {
                            db.OrderByDescending(o => "SaleCount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        break;
                    case "saleamount":
                        if (query.IsAsc)
                        {
                            db.OrderBy(o => "SaleAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        else
                        {
                            db.OrderByDescending(o => "SaleAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        break;
                    case "settlementamount":
                        if (query.IsAsc)
                        {
                            db.OrderBy(o => "SettlementAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        else
                        {
                            db.OrderByDescending(o => "SettlementAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        break;
                    case "nosettlementamount":
                        if (query.IsAsc)
                        {
                            db.OrderBy(o => "NoSettlementAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        else
                        {
                            db.OrderByDescending(o => "NoSettlementAmount");
                            db.OrderByDescending(o => o.AddDate);
                        }
                        break;
                    case "quantity":
                        if (query.IsAsc) db.OrderBy(p => "SaleCount");
                        else db.OrderByDescending(p => "SaleCount");
                        break;
                    case "amount":
                        if (query.IsAsc) db.OrderBy(p => "SaleAmount");
                        else db.OrderByDescending(p => "SaleAmount");
                        break;
                    case "settlement":
                        if (query.IsAsc) db.OrderBy(p => "SettlementAmount");
                        else db.OrderByDescending(p => "SettlementAmount");
                        break;
                    case "nosettlement":
                        if (query.IsAsc) db.OrderBy(p => "NoSettlementAmount");
                        else db.OrderByDescending(p => "NoSettlementAmount");
                        break;
                    default:
                        db.OrderByDescending(p => p.ProductId);
                        break;
                }
            }
            else
            {
                db.OrderByDescending(o => o.AddDate);
            }

            return db;
        }

        public QueryPageModel<DistributionProduct> GetProducts(DistributionProductQuery query)
        {
            var db = GetProductsBuilder(query);

            var result = db.ToPagedList<DistributionProduct>(query.PageNo, query.PageSize);

            #region 补数据
            var proids = result.Select(d => d.ProductId).ToList();
            var skugroup = DbFactory.Default.Get<SKUInfo>()
                .Where(d => d.ProductId.ExIn(proids))
                .GroupBy(d => d.ProductId)
                .Select(d => new
                {
                    ProductId = d.ProductId,
                    Stock = d.Stock.ExSum(),
                    MaxSalePrice = d.SalePrice.ExMax()
                })
                .ToList<Tmp_ProductSkuGroupModel>();
            var laddergroup = DbFactory.Default.Get<ProductLadderPriceInfo>()
                .Where(d => d.ProductId.ExIn(proids))
                .GroupBy(d => d.ProductId)
                .Select(d => new
                {
                    ProductId = d.ProductId,
                    MaxSalePrice = d.Price.ExMax()
                })
                .ToList<Tmp_ProductSkuGroupModel>();
            foreach (var item in result)
            {
                var skug = skugroup.FirstOrDefault(d => d.ProductId == item.ProductId);
                var ladderg = laddergroup.FirstOrDefault(d => d.ProductId == item.ProductId);
                if (skug != null)
                {
                    item.MaxSalePrice = skug.MaxSalePrice;
                    item.Stock = skug.Stock;
                }
                if (ladderg != null)
                {
                    if (ladderg.MaxSalePrice > item.MaxSalePrice)
                    {
                        item.MaxSalePrice = ladderg.MaxSalePrice;
                    }
                }
                item.MaxDistributionLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel;
            }
            #endregion
            return new QueryPageModel<DistributionProduct>()
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        /// <summary>
        /// 分销商品(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DistributionProduct> GetProductsAll(DistributionProductQuery query)
        {
            var db = GetProductsBuilder(query);
            var result = db.ToList<DistributionProduct>();
            foreach (var item in result)
            {
                item.MaxDistributionLevel = SiteSettingApplication.SiteSettings.DistributionMaxLevel;
            }
            return result;
        }

        /// <summary>
        /// 获取有分销商品的一级分类
        /// </summary>
        /// <returns></returns>
        public List<CategoryInfo> GetHaveDistributionProductTopCategory()
        {
            var sv = DistributionProductStatus.Normal.GetHashCode();
            var psql = DbFactory.Default.Get<DistributionProductInfo>()
                .Where(d => d.ProductStatus == sv)
                .Select(d => d.ProductId);
            var cpathlist = DbFactory.Default.Get<ProductInfo>()
                .Where(d => d.Id.ExIn(psql))
                .Select(d => d.CategoryPath)
                .ToList<string>();
            string _tmp = string.Join("|", cpathlist);
            var allcid = _tmp.Split('|').Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => long.Parse(d)).ToList();
            List<CategoryInfo> result = DbFactory.Default.Get<CategoryInfo>()
                .Where(d => d.Id.ExIn(allcid) && d.ParentCategoryId == 0)
                .ToList();
            return result;
        }
        public List<long> GetAllDistributionProductIds(long shopId)
        {
            int sv = DistributionProductStatus.Normal.GetHashCode();
            return DbFactory.Default.Get<DistributionProductInfo>()
                .Where(d => d.ShopId == shopId && d.ProductStatus == sv)
                .Select(d => d.ProductId)
                .ToList<long>();
        }

        public void RecoverDistributor(IEnumerable<long> memberIds)
        {
            int rs = DistributorStatus.Audited.GetHashCode();
            var list = DbFactory.Default.Set<DistributorInfo>().Where(p => p.MemberId.ExIn(memberIds));
            list.Set(p => p.DistributionStatus, rs).Succeed();
        }

        public void RefuseDistributor(IEnumerable<long> memberIds, string remark)
        {
            int rs = DistributorStatus.Refused.GetHashCode();
            int cs = DistributorStatus.UnAudit.GetHashCode();
            var list = DbFactory.Default.Set<DistributorInfo>()
                .Where(p => p.MemberId.ExIn(memberIds) && p.DistributionStatus == cs);
            list
                .Set(p => p.DistributionStatus, rs)
                .Set(p => p.Remark, remark)
                .Succeed();
        }
        public void AgreeDistributor(IEnumerable<long> memberIds, string remark)
        {
            int rs = DistributorStatus.Audited.GetHashCode();
            int cs = DistributorStatus.UnAudit.GetHashCode();
            var list = DbFactory.Default.Set<DistributorInfo>()
                .Where(p => p.MemberId.ExIn(memberIds) && p.DistributionStatus == cs);
            list
                .Set(p => p.DistributionStatus, rs)
                .Set(p => p.Remark, remark)
                .Succeed();
        }

        public void RemoveDistributor(IEnumerable<long> memberIds)
        {
            int rs = DistributorStatus.NotAvailable.GetHashCode();
            var list = DbFactory.Default.Set<DistributorInfo>().Where(p => p.MemberId.ExIn(memberIds));
            list.Set(p => p.DistributionStatus, rs).Succeed();
        }

        public void RemoveSpreadProducts(IEnumerable<long> productIds, long? shopId = null)
        {
            int rs = DistributionProductStatus.Removed.GetHashCode();
            var list = DbFactory.Default.Set<DistributionProductInfo>().Where(p => p.ProductId.ExIn(productIds));
            if (shopId.HasValue)
                list.Where(p => p.ShopId == shopId);
            list.Set(p => p.ProductStatus, rs).Succeed();
        }

        public void SetProductBrokerageRate(long productId, decimal BrokerageRate1, decimal BrokerageRate2 = 0, decimal BrokerageRate3 = 0)
        {
            var pro = DbFactory.Default.Get<DistributionProductInfo>().Where(p => p.ProductId == productId).FirstOrDefault();
            pro.BrokerageRate1 = BrokerageRate1;
            pro.BrokerageRate2 = BrokerageRate2;
            pro.BrokerageRate3 = BrokerageRate3;
            DbFactory.Default.Save(pro);
        }

        /// <summary>
        /// 订单分佣状态处理
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="orderItemId"></param>
        public void SetBrokerageStatus(long orderId, DistributionBrokerageStatus status, long? orderItemId = null)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (order == null || order.OrderStatus <= OrderInfo.OrderOperateStatus.WaitPay) { return; }
            List<OrderItemInfo> orderitems = new List<OrderItemInfo>();
            if (orderItemId.HasValue)
            {
                var orditem = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == orderId && d.Id == orderItemId.Value).FirstOrDefault();
                orderitems.Add(orditem);
            }
            else
            {
                orderitems = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == orderId).ToList();
            }
            if (orderitems.Count < 1) { return; }
            foreach (var item in orderitems)
            {
                decimal realPay = item.RealTotalPrice - item.FullDiscount - item.CouponDiscount - item.RefundPrice;
                var ditem = DbFactory.Default.Get<DistributionBrokerageInfo>().Where(d => d.OrderId == orderId && d.OrderItemId == item.Id).FirstOrDefault();
                if (ditem != null)
                {
                    ditem.BrokerageStatus = status;
                    if (status == DistributionBrokerageStatus.Settled)
                    {
                        ditem.SettlementTime = DateTime.Now;
                    }
                    DbFactory.Default.Update(ditem);
                }
            }
        }
        /// <summary>
        /// 移除分佣
        /// <para>订单关闭</para>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderItemId"></param>
        public void RemoveBrokerageByOrder(long orderId, long? orderItemId = null)
        {
            var sql = DbFactory.Default.Del<DistributionBrokerageInfo>().Where(d => d.OrderId == orderId);
            if (orderItemId.HasValue)
            {
                sql.Where(d => d.OrderItemId == orderItemId.Value);
            }
            sql.Succeed();
        }

        /// <summary>
        /// 订单分佣比处理（待付款）
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="canSelfBuy"></param>
        /// <param name="maxLevel"></param>
        public void TreatedOrderDistribution(long orderId, bool canSelfBuy, int maxLevel)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (order == null
                || ((order.OrderStatus != OrderInfo.OrderOperateStatus.WaitPay && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery) && order.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery)
                || (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.PaymentType == OrderInfo.PaymentTypes.CashOnDelivery))
            {
                return;
            }
            var orderitems = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == orderId).ToList();
            if (orderitems.Count < 1) { return; }
            foreach (var item in orderitems)
            {
                var bk = GetOrderItemDistributionBrokerage(order.UserId, item.ProductId, canSelfBuy, maxLevel);
                if (bk != null)
                {
                    bk.OrderId = order.Id;
                    bk.MemberId = order.UserId;
                    bk.OrderItemId = item.Id;
                    bk.BrokerageStatus = DistributionBrokerageStatus.NotAvailable;
                    bk.ProductId = item.ProductId;
                    bk.OrderDate = order.OrderDate;
                    bk.Quantity = item.Quantity;
                    bk.ShopId = order.ShopId;
                    DbFactory.Default.Add(bk);
                }
            }
        }

        /// <summary>
        /// 处理订单分佣记录（收款）
        /// <para>付款、货到付款完成订单、售后</para>
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="needOperaDistributor">是否处理销售员清退情况</param>
        /// <param name="status">处理后分佣条目的状态</param>
        /// <param name="orderItemId"></param>

        public void TreatedOrderDistributionBrokerage(long orderId, bool needOperaDistributor, long? orderItemId = null)
        {
            var order = DbFactory.Default.Get<OrderInfo>().Where(d => d.Id == orderId).FirstOrDefault();
            if (order == null || order.OrderStatus <= OrderInfo.OrderOperateStatus.WaitPay) { return; }
            List<OrderItemInfo> orderitems = new List<OrderItemInfo>();
            if (orderItemId.HasValue)
            {
                var orditem = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == orderId && d.Id == orderItemId.Value).FirstOrDefault();
                orderitems.Add(orditem);
            }
            else
            {
                orderitems = DbFactory.Default.Get<OrderItemInfo>().Where(d => d.OrderId == orderId).ToList();
            }
            if (orderitems.Count < 1) { return; }
            foreach (var item in orderitems)
            {
                decimal realPay = item.RealTotalPrice - item.FullDiscount - item.CouponDiscount - item.RefundPrice;
                var ditem = DbFactory.Default.Get<DistributionBrokerageInfo>().Where(d => d.OrderId == orderId && d.OrderItemId == item.Id).FirstOrDefault();
                if (ditem != null)
                {
                    decimal oldPayAmount = ditem.RealPayAmount;
                    long oldQuantity = ditem.Quantity;
                    var oldStatus = ditem.BrokerageStatus;
                    ditem.RealPayAmount = realPay;
                    ditem.Quantity = item.Quantity - item.ReturnQuantity;

                    if (needOperaDistributor)
                    {
                        #region 处理清退情况
                        DistributorInfo duobj = null;
                        int dasv = DistributorStatus.Audited.GetHashCode();
                        if (ditem.SuperiorId1 > 0)
                        {
                            duobj = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId1).FirstOrDefault();
                            if (duobj != null && duobj.DistributionStatus != dasv)
                            {
                                ditem.BrokerageRate1 = 0;
                            }
                        }
                        if (ditem.SuperiorId2 > 0)
                        {
                            duobj = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId2).FirstOrDefault();
                            if (duobj != null && duobj.DistributionStatus != dasv)
                            {
                                ditem.BrokerageRate2 = 0;
                            }
                        }
                        if (ditem.SuperiorId3 > 0)
                        {
                            duobj = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId3).FirstOrDefault();
                            if (duobj != null && duobj.DistributionStatus != dasv)
                            {
                                ditem.BrokerageRate3 = 0;
                            }
                        }
                        #endregion
                    }
                    #region 处理成交量、成交金额
                    var dpro = DbFactory.Default.Get<DistributionProductInfo>().Where(d => d.ProductId == ditem.ProductId).FirstOrDefault();
                    DistributorInfo buyuobj = null, duobj1 = null, duobj2 = null, duobj3 = null;
                    buyuobj = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.MemberId).FirstOrDefault();
                    if (ditem.SuperiorId1 > 0 && ditem.BrokerageRate1 > 0)
                    {
                        duobj1 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId1).FirstOrDefault();
                    }
                    if (ditem.SuperiorId2 > 0 && ditem.BrokerageRate2 > 0)
                    {
                        duobj2 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId2).FirstOrDefault();
                    }
                    if (ditem.SuperiorId3 > 0 && ditem.BrokerageRate3 > 0)
                    {
                        duobj3 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == ditem.SuperiorId3).FirstOrDefault();
                    }
                    if (oldStatus == DistributionBrokerageStatus.NotAvailable)
                    {
                        //增加
                        if (dpro != null)
                        {
                            dpro.SaleCount += (int)ditem.Quantity;
                            dpro.SaleAmount += ditem.RealPayAmount;
                        }
                        if (buyuobj != null)
                        {
                            buyuobj.ProductCount += (int)ditem.Quantity;
                            buyuobj.SaleAmount += ditem.RealPayAmount;
                        }
                        if (duobj1 != null)
                        {
                            duobj1.SubProductCount += (int)ditem.Quantity;
                            duobj1.SubSaleAmount += ditem.RealPayAmount;
                        }
                        if (duobj2 != null)
                        {
                            duobj2.SubProductCount += (int)ditem.Quantity;
                            duobj2.SubSaleAmount += ditem.RealPayAmount;
                        }
                        if (duobj3 != null)
                        {
                            duobj3.SubProductCount += (int)ditem.Quantity;
                            duobj3.SubSaleAmount += ditem.RealPayAmount;
                        }
                    }

                    if (oldStatus == DistributionBrokerageStatus.NotSettled)
                    {
                        //增量
                        if (dpro != null)
                        {
                            dpro.SaleCount -= (int)(oldQuantity - ditem.Quantity);
                            dpro.SaleAmount -= oldPayAmount - ditem.RealPayAmount;
                        }
                        if (buyuobj != null)
                        {
                            buyuobj.ProductCount -= (int)(oldQuantity - ditem.Quantity);
                            buyuobj.SaleAmount -= oldPayAmount - ditem.RealPayAmount;
                        }
                        if (duobj1 != null)
                        {
                            duobj1.SubProductCount -= (int)(oldQuantity - ditem.Quantity);
                            duobj1.SubSaleAmount -= oldPayAmount - ditem.RealPayAmount;
                        }
                        if (duobj2 != null)
                        {
                            duobj2.SubProductCount -= (int)(oldQuantity - ditem.Quantity);
                            duobj2.SubSaleAmount -= oldPayAmount - ditem.RealPayAmount;
                        }
                        if (duobj3 != null)
                        {
                            duobj3.SubProductCount -= (int)(oldQuantity - ditem.Quantity);
                            duobj3.SubSaleAmount -= oldPayAmount - ditem.RealPayAmount;
                        }
                    }
                    if (dpro != null)
                    {
                        if (dpro.SaleCount < 0)
                        {
                            dpro.SaleCount = 0;
                        }
                        if (dpro.SaleAmount < 0)
                        {
                            dpro.SaleAmount = 0;
                        }
                        DbFactory.Default.Update(dpro);
                    }
                    #endregion

                    ditem.BrokerageStatus = DistributionBrokerageStatus.NotSettled;

                    DbFactory.Default.Update(ditem);
                    if (buyuobj != null)
                    {
                        DbFactory.Default.Update(buyuobj);
                    }
                    if (duobj1 != null)
                    {
                        DbFactory.Default.Update(duobj1);
                    }
                    if (duobj2 != null)
                    {
                        DbFactory.Default.Update(duobj2);
                    }
                    if (duobj3 != null)
                    {
                        DbFactory.Default.Update(duobj3);
                    }
                }
            }
        }


        public void UpdateDistributorGrade(DistributorGradeInfo data)
        {
            var old = DbFactory.Default.Get<DistributorGradeInfo>().Where(d => d.Id == data.Id).FirstOrDefault();
            DbFactory.Default.Update(data);
            if (old != null && (old.GradeName != data.GradeName || old.Quota != data.Quota))
            {
                DbFactory.Default.Set<DistributorInfo>()
                    .Where(d => d.GradeId == data.Id)
                    .Set(d => d.GradeId, v => 0)
                    .Succeed();
            }
        }

        public DistributionShopRateConfigInfo GetDefaultBrokerageRate(long shopId)
        {
            return DbFactory.Default.Get<DistributionShopRateConfigInfo>()
                .Where(d => d.ShopId == shopId)
                .FirstOrDefault();
        }

        public void UpdateDefaultBrokerageRate(long shopId, DistributionShopRateConfigInfo data)
        {
            var d = GetDefaultBrokerageRate(shopId);
            if (d == null)
            {
                d = new DistributionShopRateConfigInfo();
                d.ShopId = shopId;
                d.BrokerageRate1 = data.BrokerageRate1;
                d.BrokerageRate2 = data.BrokerageRate2;
                d.BrokerageRate3 = data.BrokerageRate3;
                DbFactory.Default.Add(d);
            }
            else
            {
                d.ShopId = shopId;
                d.BrokerageRate1 = data.BrokerageRate1;
                d.BrokerageRate2 = data.BrokerageRate2;
                d.BrokerageRate3 = data.BrokerageRate3;
                DbFactory.Default.Update(d);
            }
        }
        /// <summary>
        /// 重置非开放等级分佣比
        /// </summary>
        /// <param name="maxLevel"></param>
        public void ResetDefaultBrokerageRate(int maxLevel)
        {
            if (maxLevel < 3)
            {
                DbFactory.Default.Set<DistributionShopRateConfigInfo>().Set(d => d.BrokerageRate3, 0).Succeed();
            }
            if (maxLevel < 2)
            {
                DbFactory.Default.Set<DistributionShopRateConfigInfo>().Set(d => d.BrokerageRate2, 0).Succeed();
            }
        }

        #region 分佣关系

        /// <summary>
        /// 获取订单项用户分佣
        /// </summary>
        /// <param name="buyMemberId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        private DistributionBrokerageInfo GetOrderItemDistributionBrokerage(long buyMemberId, long productId, bool canSelfBuy, int maxLevel)
        {
            DistributionBrokerageInfo result = null;
            var ulinks = GetBrokerageUserLink(buyMemberId, productId, canSelfBuy, maxLevel);
            var lc = ulinks.Count;
            if (ulinks != null && lc > 0 && ulinks.Any(d => d.Item2 > 0))
            {
                result = new DistributionBrokerageInfo
                {
                    ProductId = productId,
                    MemberId = buyMemberId
                };
                var _tmp = ulinks[0];
                result.SuperiorId1 = _tmp.Item1;
                result.BrokerageRate1 = _tmp.Item2;
                if (lc > 1)
                {
                    _tmp = ulinks[1];
                    result.SuperiorId2 = _tmp.Item1;
                    result.BrokerageRate2 = _tmp.Item2;
                }
                if (lc > 2)
                {
                    _tmp = ulinks[2];
                    result.SuperiorId3 = _tmp.Item1;
                    result.BrokerageRate3 = _tmp.Item2;
                }
            }
            return result;
        }
        /// <summary>
        /// 获取分佣关系
        /// <para>多级</para>
        /// </summary>
        /// <param name="buyMemberId"></param>
        /// <param name="productId"></param>
        /// <param name="canBuySelf"></param>
        private List<Tuple<long, decimal>> GetBrokerageUserLink(long buyMemberId, long productId, bool canBuySelf, int maxLevel)
        {
            List<Tuple<long, decimal>> result = new List<Tuple<long, decimal>>();
            var pnsv = DistributionProductStatus.Normal.GetHashCode();
            int dasv = (int)DistributorStatus.Audited;
            int dnasv = (int)DistributorStatus.NotAvailable;
            int curlevel = 0;
            decimal _curRate = 0m;
            var pro = DbFactory.Default.Get<DistributionProductInfo>().Where(d => d.ProductId == productId && d.ProductStatus == pnsv).FirstOrDefault();
            var buydobj = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == buyMemberId).FirstOrDefault();
            if (maxLevel < 1 || maxLevel > 3 || pro == null || buydobj == null)
            {
                return result;
            }

            //自购
            if (canBuySelf && buydobj.DistributionStatus == dasv)
            {
                curlevel++;
                _curRate = GetLevelRate(pro, curlevel);
                if (buydobj.DistributionStatus == dasv || buydobj.DistributionStatus == dnasv)
                {
                    if (buydobj.DistributionStatus == dnasv)
                    {
                        _curRate = 0;
                    }
                    result.Add(new Tuple<long, decimal>(buydobj.MemberId, _curRate));
                }
            }
            if (buydobj.SuperiorId == 0)
            {
                return result;
            }
            if (curlevel >= maxLevel)
            {
                return result;
            }
            //上级
            var pdobj1 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == buydobj.SuperiorId).FirstOrDefault();
            if (pdobj1 == null)
            {
                return result;
            }
            curlevel++;
            _curRate = GetLevelRate(pro, curlevel);
            if (pdobj1.DistributionStatus == dasv || pdobj1.DistributionStatus == dnasv)
            {
                if (pdobj1.DistributionStatus == dnasv)
                {
                    _curRate = 0;
                }
                result.Add(new Tuple<long, decimal>(pdobj1.MemberId, _curRate));
            }
            if (pdobj1.SuperiorId == 0)
            {
                return result;
            }
            if (curlevel >= maxLevel)
            {
                return result;
            }
            //上上级
            var pdobj2 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == pdobj1.SuperiorId).FirstOrDefault();
            if (pdobj2 != null)
            {
                curlevel++;
                _curRate = GetLevelRate(pro, curlevel);
                if (pdobj2.DistributionStatus == dasv || pdobj2.DistributionStatus == dnasv)
                {
                    if (pdobj2.DistributionStatus == dnasv)
                    {
                        _curRate = 0;
                    }
                    result.Add(new Tuple<long, decimal>(pdobj2.MemberId, _curRate));
                }
            }
            if (pdobj2.SuperiorId == 0)
            {
                return result;
            }
            if (curlevel >= maxLevel)
            {
                return result;
            }
            //上上上级
            var pdobj3 = DbFactory.Default.Get<DistributorInfo>().Where(d => d.MemberId == pdobj2.SuperiorId).FirstOrDefault();
            if (pdobj3 != null)
            {
                curlevel++;
                _curRate = GetLevelRate(pro, curlevel);
                if (pdobj3.DistributionStatus == dasv || pdobj3.DistributionStatus == dnasv)
                {
                    if (pdobj3.DistributionStatus == dnasv)
                    {
                        _curRate = 0;
                    }
                    result.Add(new Tuple<long, decimal>(pdobj3.MemberId, _curRate));
                }
            }
            if (pdobj3.SuperiorId == 0)
            {
                return result;
            }
            if (curlevel >= maxLevel)
            {
                return result;
            }
            return result;
        }
        /// <summary>
        /// 获取分佣等级对应商品分佣比
        /// </summary>
        /// <param name="pro"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private decimal GetLevelRate(DistributionProductInfo pro, int level)
        {
            decimal result = 0m;
            switch (level)
            {
                case 1:
                    result = pro.BrokerageRate1;
                    break;
                case 2:
                    result = pro.BrokerageRate2;
                    break;
                case 3:
                    result = pro.BrokerageRate3;
                    break;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 获取除以100保留两位小数的结果
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private decimal GetDivide100Number(decimal data)
        {
            return decimal.Parse((data / (decimal)100).ToString("f2"));
        }

        public QueryPageModel<DistributorRecordInfo> GetRecords(DistributorRecordQuery query)
        {
            var db = DbFactory.Default.Get<DistributorRecordInfo>();

            if (query.MemberId > 0)
                db.Where(p => p.MemberId == query.MemberId);

            db.OrderByDescending(p => p.Id);
            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<DistributorRecordInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        #region 佣金提现流程

        private Random random = new Random();
        private long GenerateNewNumber()
        {
            var no = long.Parse(DateTime.Now.ToString("yyMMddHHmmdd") + random.Next(1000, 9999));
            if (DbFactory.Default.Get<DistributionWithdrawInfo>(p => p.Id == no).Exist())
                return GenerateNewNumber();
            return no;
        }
        public DistributionWithdrawInfo GetWithdraw(long id)
        {
            return DbFactory.Default.Get<DistributionWithdrawInfo>(p => p.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// 获取提现记录查询构架器
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private GetBuilder<DistributionWithdrawInfo> GetWithdrawsBuilder(DistributionWithdrawQuery query)
        {
            var db = DbFactory.Default.Get<DistributionWithdrawInfo>();

            if (query.MemberId > 0)
                db.Where(p => p.MemberId == query.MemberId);
            if (!string.IsNullOrEmpty(query.MemberName))
            {
                var members = DbFactory.Default.Get<MemberInfo>(p => p.UserName.Contains(query.MemberName)).Select(p => p.Id);
                db.Where(p => p.MemberId.ExIn(members));
            }
            if (query.WithdrawId > 0)
                db.Where(p => p.Id == query.WithdrawId);

            if (query.Status.HasValue)
                db.Where(p => p.WithdrawStatus == query.Status);

            if (query.Type.HasValue)
                db.Where(p => p.WithdrawType == query.Type.Value);

            if (query.Begin.HasValue)
                db.Where(p => p.ApplyTime >= query.Begin.Value);

            if (query.End.HasValue)
            {
                DateTime edt = query.End.Value.AddDays(1);
                db.Where(p => p.ApplyTime < edt);
            }                

            db.OrderByDescending(p => p.ApplyTime);

            return db;
        }

        /// <summary>
        /// 获取提现记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<DistributionWithdrawInfo> GetWithdraws(DistributionWithdrawQuery query)
        {
            var db = GetWithdrawsBuilder(query);
            var data = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<DistributionWithdrawInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }

        /// <summary>
        /// 获取提现记录(忽略分页)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DistributionWithdrawInfo> GetWithdrawsAll(DistributionWithdrawQuery query)
        {
            return GetWithdrawsBuilder(query).ToList();
        }

        public void ApplyWithdraw(DistributionWithdrawInfo model)
        {
            model.Id = GenerateNewNumber();
            model.ApplyTime = DateTime.Now;
            model.WithdrawStatus = DistributionWithdrawStatus.UnAudit;
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Add(model);
                //变更余额与冻结金额
                DbFactory.Default.Set<DistributorInfo>()
                .Where(p => p.MemberId == model.MemberId)
                .Set(p => p.Balance, n => n.Balance - model.Amount)
                .Set(p => p.FreezeAmount, n => n.FreezeAmount + model.Amount)
                .Succeed();
            });
        }

        public void AuditingWithdraw(long withdrawId, string operatorName, string remark)
        {
            DbFactory.Default.Set<DistributionWithdrawInfo>()
                .Where(p => p.Id == withdrawId && p.WithdrawStatus == DistributionWithdrawStatus.UnAudit)
                .Set(p => p.Operator, operatorName)
                .Set(p => p.Remark, remark)
                .Set(p => p.ConfirmTime, DateTime.Now)
                .Set(p => p.WithdrawStatus, DistributionWithdrawStatus.WaitPayment)
                .Succeed();
        }
        public void FailWithdraw(long withdrawId, string remark)
        {
            DbFactory.Default.Set<DistributionWithdrawInfo>()
                .Where(p => p.Id == withdrawId && p.WithdrawStatus == DistributionWithdrawStatus.WaitPayment)
                .Set(p => p.WithdrawStatus, DistributionWithdrawStatus.UnAudit)
                .Set(p => p.Remark, remark)
                .Succeed();
        }
        public void SuccessWithdraw(long withdrawId, string payNo)
        {
            var model = DbFactory.Default.Get<DistributionWithdrawInfo>(p => p.Id == withdrawId && p.WithdrawStatus == DistributionWithdrawStatus.WaitPayment).FirstOrDefault();
            model.PayNo = payNo;
            model.PayTime = DateTime.Now;
            model.WithdrawStatus = DistributionWithdrawStatus.Completed;

            var distributor = DbFactory.Default.Get<DistributorInfo>(p => p.MemberId == model.MemberId).FirstOrDefault();
            var record = new DistributorRecordInfo
            {
                CreateTime = DateTime.Now,
                Amount = -model.Amount,//提现为负
                MemberId = model.MemberId,
                Remark = string.Format("提现单:{0}", model.Id),
                Type = DistributorRecordType.Withdraw,
                Balance = distributor.Balance,
            };

            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Update(model);
                DbFactory.Default.Set<DistributorInfo>()
                .Where(p => p.MemberId == model.MemberId)
                //变更冻结金额
                .Set(p => p.FreezeAmount, n => n.FreezeAmount - model.Amount)
                //变更已提现金额
                .Set(p => p.WithdrawalsAmount, n => n.WithdrawalsAmount + model.Amount)
                .Succeed();
                //插入账户流水
                DbFactory.Default.Add(record);
            });


        }


        public void RefusedWithdraw(long withdrawId, string operatorName, string remark)
        {
            var model = DbFactory.Default.Get<DistributionWithdrawInfo>(p => p.Id == withdrawId && p.WithdrawStatus == DistributionWithdrawStatus.UnAudit).FirstOrDefault();
            model.Operator = operatorName;
            model.Remark = remark;
            model.ConfirmTime = DateTime.Now;
            model.WithdrawStatus = DistributionWithdrawStatus.Refused;
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Update(model);
                //还原冻结金额
                DbFactory.Default.Set<DistributorInfo>()
                .Where(p => p.MemberId == model.MemberId)
                .Set(p => p.Balance, n => n.Balance + model.Amount)
                .Set(p => p.FreezeAmount, n => n.FreezeAmount - model.Amount)
                .Succeed();
            });
            //发送消息
            var member = MemberApplication.GetMember(model.MemberId);
            var message = new MessageWithDrawInfo();
            message.UserName = member != null ? member.UserName : "";
            message.Amount = model.Amount;
            message.ApplyType = model.WithdrawType.GetHashCode();
            message.ApplyTime = model.ApplyTime;
            message.Remark = model.Remark;
            message.SiteName = SiteSettingApplication.SiteSettings.SiteName;
            Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnMemberWithDrawFail(model.MemberId, message));
        }
        #endregion

        public decimal GetDistributionBrokerageAmount(long orderid)
        {
            var distributionBrokerage = DbFactory.Default.Get<DistributionBrokerageInfo>()
                .Where(p => p.OrderId == orderid).ToList();
            var totalBrokerage = distributionBrokerage.Sum(e => GetDivide100Number(e.BrokerageRate1 * e.RealPayAmount)
                      + GetDivide100Number(e.BrokerageRate2 * e.RealPayAmount)
                      + GetDivide100Number(e.BrokerageRate3 * e.RealPayAmount));

            return totalBrokerage;
        }
    }

    public class Tmp_DistributorSubNumberGroupModel
    {
        public long MemberId { get; set; }
        public long SuperiorId { get; set; }
        public int SubNumber { get; set; }
        public int SubNumber2 { get; set; }
    }
    public class Tmp_DistributorGrabeGroupModel
    {
        public long GradeId { get; internal set; }
        public long MemberCount { get; internal set; }
    }
    public class Tmp_ProductSkuGroupModel
    {
        public long ProductId { get; internal set; }
        public long Stock { get; internal set; }
        /// <summary>
        /// 最高售价
        /// </summary>
        public decimal MaxSalePrice { get; set; }
    }
    public class Tmp_DistributionOrderGroupModel
    {
        public long OrderId { get; internal set; }
        public DateTime MinOrderDate { get; internal set; }
    }
}
