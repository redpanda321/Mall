using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Mall.Service
{
    public class ShopBonusService : ServiceBase, IShopBonusService
    {
        public QueryPageModel<ShopBonusInfo> Get(long shopid, string name, int state, int pageIndex, int pageSize)
        {
            var query = DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.ShopId == shopid);
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }
            if (state > 0)
            {
                if (state == 1)
                {
                    query = query.Where(p => p.DateEnd > DateTime.Now && p.DateStart < DateTime.Now && p.IsInvalid == false);
                }
                else if(state == 2)
                {
                    query = query.Where(p => p.DateEnd < DateTime.Now || p.IsInvalid == true);
                }
                else if(state == 3)
                {
                    query = query.Where(p => p.DateStart > DateTime.Now && p.IsInvalid == false);
                }
            }
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            var datas = query.OrderByDescending(o => o.BonusDateStart).ToPagedList(pageIndex, pageSize);

            QueryPageModel<ShopBonusInfo> pageModel = new QueryPageModel<ShopBonusInfo>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
            return pageModel;
        }

        public decimal GetUsedPrice(long orderid, long userid)
        {
            var model = DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.UsedOrderId == orderid && p.UserId == userid).FirstOrDefault();
            if (model != null)
            {
                return (decimal)model.Price;
            }
            return 0;
        }

        public ShopBonusInfo GetShopBonus(long id)
        {
            return DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.Id == id).FirstOrDefault();
        }
        public List<ShopBonusInfo> GetShopBonus(List<long> bonus)
        {
            return DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.Id.ExIn(bonus)).ToList();
        }

        public ShopBonusInfo GetByGrantId(long grantid)
        {
            return DbFactory.Default.Get<ShopBonusInfo>().InnerJoin<ShopBonusGrantInfo>((sb, sbg) => sb.Id == sbg.ShopBonusId && sbg.Id == grantid).FirstOrDefault();
        }

        /// <summary>
        /// 获取可用红包
        /// </summary>
        /// <returns></returns>
        public List<ShopBonusReceiveInfo> GetDetailToUse(long shopid, long userid, decimal sumprice)
        {
            var result = DbFactory.Default.Get<ShopBonusReceiveInfo>()
                .InnerJoin<ShopBonusGrantInfo>((sbr, sbg) => sbr.BonusGrantId == sbg.Id)
                .InnerJoin<ShopBonusInfo, ShopBonusGrantInfo>((sb, sbg) => sbg.ShopBonusId == sb.Id)
                .Where(p => p.UserId == userid && p.State == ShopBonusReceiveInfo.ReceiveState.NotUse)
                .Where<ShopBonusInfo>(p => p.ShopId == shopid && p.BonusDateEnd > DateTime.Now && p.BonusDateStart < DateTime.Now && p.UsrStatePrice <= sumprice);

            if (result.Count() <= 0)
                return new List<ShopBonusReceiveInfo>();

            //var grant = GetGrant(result.FirstOrDefault().BonusGrantId);
            //var bonus = GetShopBonu(grant.ShopBonusId);
            //if (bonus.UseState == ShopBonusInfo.UseStateType.FilledSend)
            //{//修改为不需要大于优惠券本身价格 && p.Price < sumprice 
            //    result = result.Where(p => p.ShopBonusGrantInfo.ShopBonusInfo.UsrStatePrice <= sumprice).OrderByDescending(p => p.Price).ToList();
            //}

            return result.OrderByDescending(p => p.Price).ToList();
        }

        public int GetAvailableBonusCountByUser(long userId)
        {
            return DbFactory.Default.Get<ShopBonusReceiveInfo>()
                .LeftJoin<ShopBonusGrantInfo>((sri, sgi) => sri.BonusGrantId == sgi.Id)
                .LeftJoin<ShopBonusInfo, ShopBonusGrantInfo>((sbi, sgi) => sgi.ShopBonusId == sbi.Id)
                .Where(p => p.UserId == userId && p.State == ShopBonusReceiveInfo.ReceiveState.NotUse)
                .Where<ShopBonusInfo>(p => p.BonusDateEnd > DateTime.Now && p.BonusDateStart < DateTime.Now)
                .Count();
        }


        public QueryPageModel<ShopBonusReceiveInfo> GetDetailByQuery(CouponRecordQuery query)
        {
            if (query.PageNo <= 0)
            {
                query.PageNo = 1;
            }

            var bonusReceiveContext = DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.UserId == query.UserId);
            if (query.Status == 0)
            {

                bonusReceiveContext.InnerJoin<ShopBonusGrantInfo>((r, b) => r.BonusGrantId == b.Id && r.State == ShopBonusReceiveInfo.ReceiveState.NotUse)
                    .InnerJoin<ShopBonusInfo, ShopBonusGrantInfo>((s, b) => b.ShopBonusId == s.Id && s.BonusDateEnd > DateTime.Now && s.BonusDateStart < DateTime.Now);
                //bonusReceiveContext.Where(
                //    p => p.ShopBonusGrantInfo.ShopBonusInfo.BonusDateEnd > DateTime.Now &&
                //    p.State == ShopBonusReceiveInfo.ReceiveState.NotUse);
            }
            else if (query.Status == 1)
            {
                bonusReceiveContext.Where(p => p.State == ShopBonusReceiveInfo.ReceiveState.Use);
            }
            else if (query.Status == 2)
            {
                bonusReceiveContext.InnerJoin<ShopBonusGrantInfo>((r, b) => r.BonusGrantId == b.Id)
                   .InnerJoin<ShopBonusInfo, ShopBonusGrantInfo>((s, b) => b.ShopBonusId == s.Id && s.BonusDateEnd < DateTime.Now);
                //bonusReceiveContext.Where(p => p.ShopBonusGrantInfo.ShopBonusInfo.BonusDateEnd < DateTime.Now);
            }

            var datas = bonusReceiveContext.OrderByDescending(o => o.ReceiveTime).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<ShopBonusReceiveInfo> pageModel = new QueryPageModel<ShopBonusReceiveInfo>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
            return pageModel;
        }

        public List<ShopBonusReceiveInfo> GetDetailByUserId(long userid)
        {
            return DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.UserId == userid).ToList();
        }

        public List<ShopBonusReceiveInfo> GetCanUseDetailByUserId(long userid)
        {
            return DbFactory.Default.Get<ShopBonusReceiveInfo>()
                .InnerJoin<ShopBonusGrantInfo>((sbr, sbg) => sbr.BonusGrantId == sbg.Id && sbr.UserId == userid && sbr.State == ShopBonusReceiveInfo.ReceiveState.NotUse)
                .InnerJoin<ShopBonusInfo, ShopBonusGrantInfo>((sb, sbg) => sbg.ShopBonusId == sb.Id && sb.BonusDateEnd > DateTime.Now && sb.BonusDateStart < DateTime.Now).ToList();
        }

        public ShopBonusInfo GetByShopId(long shopid)
        {
            return DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.ShopId == shopid && p.IsInvalid == false && p.DateEnd > DateTime.Now && DateTime.Now > p.DateStart).FirstOrDefault();
        }

        public ShopInfo GetShopByReceive(long id)
        {
            var receive = DbFactory.Default.Get<ShopBonusReceiveInfo>(p => p.Id == id).FirstOrDefault();
            var grant = DbFactory.Default.Get<ShopBonusGrantInfo>(p => p.Id == receive.BonusGrantId).FirstOrDefault();
            var bonus = DbFactory.Default.Get<ShopBonusInfo>(p => p.Id == grant.ShopBonusId).FirstOrDefault();
            var shop = DbFactory.Default.Get<ShopInfo>(p => p.Id == bonus.ShopId).FirstOrDefault();
            return shop;
        }

        public ShopBonusGrantInfo GetGrant(long id)
        {
            return DbFactory.Default.Get<ShopBonusGrantInfo>(p => p.Id == id).FirstOrDefault();
        }
        public List<ShopBonusGrantInfo> GetGrants(List<long> grants)
        {
            return DbFactory.Default.Get<ShopBonusGrantInfo>(p => p.Id.ExIn(grants)).ToList();
        }


        public ShopBonusReceiveInfo GetDetailById(long userid, long id)
        {
            return DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.Id == id && p.UserId == userid).FirstOrDefault();
        }

        public ShopBonusGrantInfo GetGrantByUserOrder(long orderid, long userid)
        {
            var sbrisql = DbFactory.Default
                .Get<ShopBonusReceiveInfo>()
                .Where<ShopBonusGrantInfo>((sbri, sbgi) => sbri.BonusGrantId == sbgi.Id && sbri.UserId == 0);
            var result = DbFactory.Default
                .Get<ShopBonusGrantInfo>()
                .InnerJoin<ShopBonusInfo>((sbgi, sbi) => sbgi.ShopBonusId == sbi.Id)
                .Where(p => p.OrderId == orderid && p.UserId == userid && p.ExExists(sbrisql))
                //p.ShopBonusReceiveInfo.Any(o => o.UserId == null || o.UserId <= 0) &&  //还存在未领取的红包
                .Where<ShopBonusInfo>(p => p.DateEnd > DateTime.Now)
                .FirstOrDefault();  //未超过红包有效期
            return result;
        }

        public List<ShopBonusReceiveInfo> GetDetailByGrantId(long grantid)
        {
            return DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.BonusGrantId == grantid && p.OpenId.ExIfNull("") != "").ToList();
        }

        public QueryPageModel<ShopBonusReceiveInfo> GetDetail(long bonusid, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            int total = 0;
            var bonusReceiveContext = DbFactory.Default.Get<ShopBonusReceiveInfo>()
                .InnerJoin<ShopBonusGrantInfo>((sbr, sbg) => sbr.BonusGrantId == sbg.Id && sbg.ShopBonusId == bonusid);

            var datas = bonusReceiveContext.OrderByDescending(o => o.ReceiveTime).ToPagedList(pageIndex, pageSize);

            QueryPageModel<ShopBonusReceiveInfo> pageModel = new QueryPageModel<ShopBonusReceiveInfo>()
            {
                Models = datas,
                Total = total
            };
            return pageModel;
        }

        public long GetGrantIdByOrderId(long orderid)
        {
            var model = DbFactory.Default.Get<ShopBonusGrantInfo>().Where(p => p.OrderId == orderid).FirstOrDefault();
            if (model != null)
            {
                return model.Id;
            }
            return 0;
        }

        public bool IsAdd(long shopid)
        {
            var bonus = DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.ShopId == shopid && p.IsInvalid == false && p.DateEnd > DateTime.Now && p.DateStart < DateTime.Now).FirstOrDefault();
            if (bonus != null)
            {
                return false;
            }
            return true;
        }

        public void Add(ShopBonusInfo model, long shopid)
        {
            var bonus = DbFactory.Default.Get<ShopBonusInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (bonus != null && bonus.IsInvalid == false && bonus.DateEnd > DateTime.Now && bonus.DateStart < DateTime.Now)
            {
                throw new MallException("一个时间段只能新增一个随机红包");
            }
            model.DateEnd = model.DateEnd.AddHours(23).AddMinutes(59).AddSeconds(59);
            model.BonusDateEnd = model.BonusDateEnd.AddHours(23).AddMinutes(59).AddSeconds(59);
            model.ShopId = shopid;
            model.IsInvalid = false;
            model.ReceiveCount = 0;
            model.QRPath = "";
            if (model.SynchronizeCard)
            {
                //微信卡券同步改为自动过审后有BUG,暂时默认优惠券添加就是同步状态成功的
                model.WXCardState = WXCardLogInfo.AuditStatusEnum.Audited.GetHashCode();
            }


            DbFactory.Default.Add(model);

            #region 同步微信
            if (model.SynchronizeCard == true)
            {
                WXCardLogInfo wxdata = new WXCardLogInfo();
                wxdata.CardColor = model.CardColor;
                wxdata.CardTitle = model.CardTitle;
                wxdata.CardSubTitle = model.CardSubtitle;
                wxdata.CouponType = WXCardLogInfo.CouponTypeEnum.Bonus;
                wxdata.CouponId = model.Id;
                wxdata.ShopId = model.ShopId;
                wxdata.Quantity = 0;  //最大库存数
                wxdata.DefaultDetail = model.RandomAmountStart.ToString("F2") + "元" + model.RandomAmountEnd.ToString("F2") + "元随机优惠券1张";
                wxdata.LeastCost = (model.UseState == ShopBonusInfo.UseStateType.None ? 0 : (int)(model.UsrStatePrice * 100));
                wxdata.BeginTime = model.BonusDateStart.Date;
                wxdata.EndTime = model.BonusDateEnd.AddDays(1).AddMinutes(-1);
                ServiceProvider.Instance<IWXCardService>.Create.Add(wxdata);
            }
            #endregion
        }

        public void Update(ShopBonusInfo model)
        {
            model.DateEnd = model.DateEnd.AddHours(23).AddMinutes(59).AddSeconds(59);
            DbFactory.Default.Update(model);
        }

        public void Invalid(long id)
        {
            DbFactory.Default.Set<ShopBonusInfo>().Where(p => p.Id == id)
                            .Set(p => p.IsInvalid, true).Succeed();
        }

        public bool IsOverDate(DateTime bonusDateEnd, DateTime dateEnd, long shopid)
        {
            var market = GetShopBonusService(shopid);
            var time = MarketApplication.GetServiceEndTime(market.Id);
            if (bonusDateEnd > time || dateEnd > time)
            {
                return true;
            }
            return false;
        }

        public object Receive(long grantid, string openId, string wxhead, string wxname)
        {
            if (DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.OpenId == openId && p.BonusGrantId == grantid).Exist()) //已领取过
            {
                return new ShopReceiveModel { State = ShopReceiveStatus.Receive, Price = 0 };
            }

            var receives = DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.BonusGrantId == grantid && p.OpenId.ExIfNull("") == "").FirstOrDefault();
            if (receives == null)  //已被领完
                return new ShopReceiveModel { State = ShopReceiveStatus.HaveNot, Price = 0 };
            var bonus = DbFactory.Default.Get<ShopBonusInfo>()
                .LeftJoin<ShopBonusGrantInfo>((b, g) => g.ShopBonusId == b.Id)
                .Where<ShopBonusGrantInfo>(p => p.Id == receives.BonusGrantId)
                .FirstOrDefault();

            if (bonus.IsInvalid)  //失效
                return new ShopReceiveModel { State = ShopReceiveStatus.Invalid, Price = 0 };

            MemberOpenIdInfo model = model = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.OpenId.ToLower() == openId.ToLower()).FirstOrDefault();
            if (model != null) //在平台有帐号并且已经绑定openid
            {
                receives.UserId = model.UserId;
            }

            receives.OpenId = openId;
            receives.ReceiveTime = DateTime.Now;
            receives.WXHead = wxhead;
            receives.WXName = wxname;
            DbFactory.Default.Update(receives);
            if (model != null)
            {
                string username = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == model.UserId).Select(p => p.UserName).FirstOrDefault<string>();
                return new ShopReceiveModel { State = ShopReceiveStatus.CanReceive, Price = (decimal)receives.Price, UserName = username, Id = receives.Id };
            }
            return new ShopReceiveModel { State = ShopReceiveStatus.CanReceiveNotUser, Price = (decimal)receives.Price, Id = receives.Id };
        }

        public void SetBonusToUsed(long userid, List<OrderInfo> orders, long rid)
        {
            var model = DbFactory.Default.Get<ShopBonusReceiveInfo>().Where(p => p.UserId == userid && p.Id == rid).FirstOrDefault();

            model.State = ShopBonusReceiveInfo.ReceiveState.Use;
            model.UsedTime = DateTime.Now;
            var grant = GetGrant(model.BonusGrantId);
            var bonus = GetShopBonus(grant.ShopBonusId);
            model.UsedOrderId = orders.FirstOrDefault(p => p.ShopId == bonus.ShopId).Id;

            DbFactory.Default.Update(model);
            //TODO:DZY[150916]同步核销卡券
            Mall.ServiceProvider.Instance<IWXCardService>.Create.Consume(model.Id, WXCardLogInfo.CouponTypeEnum.Bonus);
        }

        public ShopBonusGrantInfo GetByOrderId(long orderid)
        {
            return DbFactory.Default.Get<ShopBonusGrantInfo>().Where(p => p.OrderId == orderid).FirstOrDefault();
        }

        /// <summary>
        /// 生成红包详情
        /// </summary>
        public long GenerateBonusDetail(ShopBonusInfo model, long orderid, string receiveurl)
        {
            if (model.DateEnd <= DateTime.Now || model.IsInvalid)  //过期、失效
            {
                Log.Info("此活动已过期 , shopid = " + model.ShopId);
                return 0;
            }
            else if (model.DateStart > DateTime.Now) // 未开始
            {
                Log.Info("此活动未开始 , shopid = " + model.ShopId);
                return 0;
            }

            ShopBonusGrantInfo grant = null;
            try
            {
                var order = DbFactory.Default.Get<OrderInfo>().Where(r => r.Id == orderid).FirstOrDefault();

                grant = DbFactory.Default.Get<ShopBonusGrantInfo>().Where(p => p.OrderId == orderid).FirstOrDefault();

                //if (Context.ShopBonusGrantInfo.Exist((p => p.OrderId == orderid)))
                if (DbFactory.Default.Get<ShopBonusGrantInfo>().Where(p => p.OrderId == orderid).Exist())
                {
                    Log.Info("此活动已存在防止多次添加 , shopid = " + model.ShopId);
                    return grant.Id;
                }
                DbFactory.Default.InTransaction(() =>
                {
                    grant = new ShopBonusGrantInfo();
                    grant.ShopBonusId = model.Id;
                    grant.UserId = order.UserId;
                    grant.OrderId = orderid;
                    grant.BonusQR = "";
                    DbFactory.Default.Add(grant);

                    string path = GenerateQR(Path.Combine(receiveurl, grant.Id.ToString()));
                    grant.BonusQR = path;
                    DbFactory.Default.Set<ShopBonusGrantInfo>().Set(e => e.BonusQR, path).Where(e => e.Id == grant.Id).Succeed();

                    for (int i = 0; i < model.Count; i++)
                    {
                        decimal randPrice = GenerateRandomAmountPrice(model.RandomAmountStart, model.RandomAmountEnd);
                        ShopBonusReceiveInfo detail = new ShopBonusReceiveInfo
                        {
                            BonusGrantId = grant.Id,
                            Price = randPrice,
                            OpenId = null,
                            State = ShopBonusReceiveInfo.ReceiveState.NotUse
                        };
                        DbFactory.Default.Add(detail);
                    }
                });
            }
            catch (Exception e)
            {
                Log.Info("错误：", e);

                var sql = DbFactory.Default.Get<ShopBonusGrantInfo>()
                    .Where<ShopBonusReceiveInfo>((sbg, sbr) => sbg.Id == sbr.BonusGrantId && sbg.ShopBonusId == model.Id);
                DbFactory.Default.Del<ShopBonusReceiveInfo>().Where(n => n.ExExists(sql)).Succeed();

                DbFactory.Default.Del<ShopBonusGrantInfo>().Where(s => s.ShopBonusId == model.Id).Succeed();
                DbFactory.Default.Del<ShopBonusInfo>().Where(s => s.Id == model.Id).Succeed();
            }
            return grant.Id;
        }

        private Random _random = new Random((int)(DateTime.Now.Ticks & 0xffffffffL) | (int)(DateTime.Now.Ticks >> 32));
        /// <summary>
        /// 生成随机数
        /// </summary>
        private decimal GenerateRandomAmountPrice(decimal start, decimal end)
        {
            if (start == end)
            {
                return start;
            }

            decimal temp = _random.Next((int)start, (int)end);
            string startF = String.Format("{0:N2} ", start);
            string endF = String.Format("{0:N2} ", end);
            startF = startF.Substring(startF.IndexOf('.') + 1, 2);//小数位
            endF = endF.Substring(endF.IndexOf('.') + 1, 2);//小数位
            if (int.Parse(startF) == 0)
            {
                startF = "1";
            }
            if (int.Parse(endF) < int.Parse(startF))
            {
                endF = "100";
            }
            decimal tempF = (decimal)_random.Next(int.Parse(startF), int.Parse(endF)) / 100;

            temp += tempF;
            return temp;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path)
        {
            Bitmap bi = Mall.Core.Helper.QRCodeHelper.Create(path);
            MemoryStream ms = new MemoryStream();
            bi.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            var fileFolderPath = "/temp/ShopBonusShareCode" + fileName;
            MemoryStream me = new MemoryStream(ms.ToArray());
            Core.MallIO.CreateFile(fileFolderPath, me, FileCreateType.Create);
            string newPath = MoveImages(fileFolderPath);
            ms.Dispose();
            me.Dispose();
            return newPath;
        }

        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var ext = image.Substring(image.LastIndexOf("."));
            var name = DateTime.Now.ToString("yyyyMMddhhmmss");
            //转移图片
            string relativeDir = "/Storage/ShopbonusGrant/ShopBonusShareCode/";
            string fileName = "hare" + name + ext;
            if (image.Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                string temp = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(temp, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                var fname = image.Substring(image.LastIndexOf("/") + 1);
                return relativeDir + fname;
            }
        }
        public ActiveMarketServiceInfo GetShopBonusService(long shopId)
        {
            return DbFactory.Default.Get<ActiveMarketServiceInfo>()
                .Where(m => m.ShopId == shopId && m.TypeId == MarketType.RandomlyBonus)
                .FirstOrDefault();
        }
        /// <summary>
        /// 同步微信卡券审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="auditstatus">审核状态</param>
        public void SyncWeixinCardAudit(long id, WXCardLogInfo.AuditStatusEnum auditstatus)
        {
            DbFactory.Default.Set<ShopBonusInfo>()
                .Set(p => p.WXCardState, (int)auditstatus)
                .Where(p => p.Id == id).Succeed();
        }
    }


}
