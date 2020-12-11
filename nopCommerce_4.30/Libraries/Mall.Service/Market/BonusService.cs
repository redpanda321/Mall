using Mall.Application;
using Mall.CommonModel;
using Mall.CommonModel.WeiXin;
using Mall.Core;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using Mall.Service.Market.Business;
using NetRube.Data;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mall.Service
{
    public class BonusService : ServiceBase, IBonusService
    {
        public bool CanAddBonus()
        {
            int attentionCount = DbFactory.Default.Get<BonusInfo>().Where(p =>
                p.Type == BonusInfo.BonusType.Attention &&  //关注红包
                p.IsInvalid == false &&                                                   //没有失效
                p.EndTime > DateTime.Now).Count();                //还未结束
            if (attentionCount == 1)
            {
                return false;
            }
            return true;

        }

        public void Add(BonusInfo model, string receiveurl)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    model.EndTime = model.EndTime.AddHours(23).AddMinutes(59).AddSeconds(59);
                    model.IsInvalid = false;
                    model.ReceiveCount = 0;
                    model.QRPath = "";
                    model.ReceiveHref = "";
                    var flag = DbFactory.Default.Add(model);
                    if (!flag) return false;

                    string path = GenerateQR(receiveurl+model.Id);
                    flag = DbFactory.Default
                        .Set<BonusInfo>()
                        .Set(n => n.ReceiveHref, receiveurl + model.Id)
                        .Set(n => n.QRPath, path)
                        .Where(n => n.Id == model.Id)
                        .Succeed();
                    if (!flag) return false;

                    Task.Factory.StartNew(() => GenerateBonusDetail(model));
                    return true;
                });
        }

        public void Update(BonusInfo model)
        {
            BonusInfo info = DbFactory.Default.Get<BonusInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            if (null == info) return;
            info.Style = model.Style;
            info.Name = model.Name;
            info.MerchantsName = model.MerchantsName;
            info.Remark = model.Remark;
            info.Blessing = model.Blessing;
            info.StartTime = model.StartTime;
            info.EndTime = (model.EndTime == null ? model.EndTime : DateTime.Parse(model.EndTime.ToString("yyyy-MM-dd") + " 23:59:59"));
            info.ImagePath = model.ImagePath;
            info.Description = model.Description;
            info.IsAttention = model.IsAttention;
            info.IsGuideShare = model.IsGuideShare;
            //info.ReceiveCount = model.ReceiveCount;
            DbFactory.Default.Update(info);
        }

        public void IncrReceiveCount(long id)
        {
            BonusInfo info = DbFactory.Default.Get<BonusInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (null == info) return;
            info.ReceiveCount++;
            DbFactory.Default.Update(info);
        }

        public void Invalid(long id)
        {
            var model = DbFactory.Default.Get<BonusInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (null == model) return;
            model.IsInvalid = true;
            DbFactory.Default.Update(model);
        }

        public QueryPageModel<BonusInfo> Get(BonusQuery query)
        {
            var db = DbFactory.Default.Get<BonusInfo>();
            if (query.Type.HasValue)
                db.Where(p => p.Type == query.Type);
            if (!string.IsNullOrEmpty(query.Name))
                db.Where(p => p.Name.Contains(query.Name));

            if (query.State == 1)
                db = db.Where(p => p.EndTime > DateTime.Now && p.IsInvalid == false);
            else if (query.State == 2)
                db = db.Where(p => p.IsInvalid == true || p.EndTime < DateTime.Now);

            switch (query.Sort.ToLower())
            {
                case "totalprice":
                    if (query.IsAsc) db.OrderBy(p => p.TotalPrice);
                    else db.OrderByDescending(p => p.TotalPrice);
                    break;
                case "receivecount":
                    if (query.IsAsc) db.OrderBy(p => p.ReceiveCount);
                    else db.OrderByDescending(p => p.ReceiveCount);
                    break;
                case "starttime":
                    if (query.IsAsc) db.OrderBy(p => p.StartTime);
                    else db.OrderByDescending(p => p.StartTime);
                    break;
                case "endtime":
                    if (query.IsAsc) db.OrderBy(p => p.EndTime);
                    else db.OrderByDescending(p => p.EndTime);
                    break;
                default:
                    db.OrderByDescending(o => o.StartTime);
                    break;
            }
            var datas = db.ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<BonusInfo>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
        }

        public QueryPageModel<BonusReceiveInfo> GetDetail(long bonusId, int pageIndex = 1, int pageSize = 15)
        {
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            var bonusReceiveContext = DbFactory.Default
                .Get<BonusReceiveInfo>()
                .LeftJoin<BonusInfo>((bri, bi) => bri.BonusId == bi.Id)
                .Where<BonusInfo>(p => p.Id == bonusId);
            var datas = bonusReceiveContext.OrderByDescending(o => o.ReceiveTime).ToPagedList(pageIndex, pageSize);
            var pageModel = new QueryPageModel<BonusReceiveInfo>()
            {
                Models = datas,
                Total = datas.TotalRecordCount
            };
            return pageModel;
        }

        /// <summary>
        /// 现金红包领取（3.3版本因为要兼容小程序领取现金红包，BonusReceiveInfo表中的OpenId改为存储Unionid）
        /// </summary>
        /// <param name="id"></param>
        /// <param name="openId"></param>
        /// <param name="unionId"></param>
        /// <returns></returns>
        public object Receive(long id, string openId,string unionId)
        {
            //领取红包是否要关注
            bool isAttention = DbFactory.Default.Get<BonusInfo>().Where(p => p.Id == id && p.IsAttention == true).Exist();
            bool selfIsAttention = IsAttention(openId);//当前账号是否有关注
            if (isAttention)  //需要关注
            {
                if (!selfIsAttention)
                {
                    //没有关注
                    return new ReceiveModel { State = ReceiveStatus.NotAttention, Price = 0 };
                }
            }


            var receives = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => (p.OpenId.ToLower() == openId.ToLower() || p.OpenId.ToLower() == unionId.ToLower()) && p.BonusId == id);
            int count = receives.Count();
            bool isShare = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => (p.OpenId.ToLower() == openId.ToLower() || p.OpenId.ToLower() == unionId.ToLower()) && p.BonusId == id && p.IsShare == true).Exist();

            if (count > 0)
            {
                if (!isShare && count == 1 || count == 2) //没有分享时，有过一次领取记录，就不能领取。最大领取次数2次
                {
                    //没有领取机会
                    return new ReceiveModel { State = ReceiveStatus.Receive, Price = 0 };
                }
            }

            var query = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => p.BonusId == id && p.OpenId.ExIfNull("") == "");
            var obj = query.FirstOrDefault();
            if (obj != null)
            {
                var bonus = Get(obj.BonusId);
                if (bonus.IsInvalid)  //失效
                {
                    return new ReceiveModel { State = ReceiveStatus.Invalid, Price = 0 };
                }
                
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        //可以领取
                        bonus.ReceiveCount += 1;
                        bonus.ReceivePrice += obj.Price;
                        obj.OpenId = unionId;
                        obj.ReceiveTime = DateTime.Now;
                        obj.IsTransformedDeposit = false;
                        var flag = DbFactory.Default.Update(bonus) > 0;
                        if (!flag) return false;
                        return DbFactory.Default.Update(obj) > 0;
                    });
                //if(selfIsAttention )
                //{
                //    Task.Factory.StartNew( () =>
                //    {
                //        Mall.ServiceProvider.Instance<IWXApiService>.Create.Subscribe( openId );
                //    } );
                //}

                //根据openid判断是否绑定了用户，有则红包存到预存款里处理；
                DepositToMember(openId, obj.Price,unionId);

                if (selfIsAttention)
                {
                    Task.Factory.StartNew(() =>
                    {
                        GetSuccessSendWXMessage(obj, openId);
                    });
                    return new ReceiveModel { State = ReceiveStatus.CanReceive, Price = obj.Price };
                }
                else
                {
                    return new ReceiveModel { State = ReceiveStatus.CanReceiveNotAttention, Price = obj.Price };
                }

            }
            else
            {
                //红包已被领取完
                return new ReceiveModel { State = ReceiveStatus.HaveNot, Price = 0 };
            }
        }

        /// <summary>
        /// 发送微信模板消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="openId"></param>
        public void GetSuccessSendWXMessage(BonusReceiveInfo data, string openId)
        {
            var member = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == data.UserId).FirstOrDefault();
            #region 发送模板消息
            //TODO:DZY[150914]此处功能需要整理，暂时只是实现功能
            var msgdata = new WX_MsgTemplateSendDataModel();
            var bouns = Get(data.BonusId);
            msgdata.first.value = "恭喜红包领取成功！";
            msgdata.first.color = "#000000";
            msgdata.keyword1.value = data.Price.ToString() + "元红包";// user != null ? user.Nick : "微信会员";
            msgdata.keyword1.color = "#000000";
            msgdata.keyword2.value = data.ReceiveTime.Value.ToString("yyyy-MM-dd HH:mm");
            msgdata.keyword2.color = "#FF0000";
            msgdata.keyword3.value = bouns.EndTime.ToString("yyyy-MM-dd HH:mm");
            msgdata.keyword3.color = "#FF0000";
            msgdata.remark.value = "红包已成功领取，请在规定时间内使用，过期作废哦！";
            msgdata.remark.color = "#000000";

            //处理url
            //var _iwxtser = Mall.ServiceProvider.Instance<IWXMsgTemplateService>.Create;
            //string url = _iwxtser.GetMessageTemplateShowUrl(Mall.Core.Plugins.Message.MessageTypeEnum.ReceiveBonus);
            //var wxmsgtmpl = _iwxtser.GetWeiXinMsgTemplate(Mall.Core.Plugins.Message.MessageTypeEnum.ReceiveBonus);

            //var siteSetting = SiteSettingApplication.SiteSettings;
            //if (wxmsgtmpl != null)
            //{
            //    if (!string.IsNullOrWhiteSpace(wxmsgtmpl.TemplateId) && wxmsgtmpl.IsOpen)
            //    {
            //        _iwxtser.SendMessageByTemplate(Core.Plugins.Message.MessageTypeEnum.ReceiveBonus, data.UserId.HasValue ? data.UserId.Value : 0, msgdata, url, openId);
            //        //Mall.ServiceProvider.Instance<IWXApiService>.Create.SendMessageByTemplate(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret, openId, wxmsgtmpl.TemplateId, "#000000", url, msgdata);
            //    }
            //}
            #endregion
        }

        public string Receive(string openId)
        {
            bool IsfirstAttention = DbFactory.Default.Get<OpenIdInfo>().Where(p => p.OpenId == openId).Exist();
            //不是首次关注（说明它已关注过直接返回null，不需执行后面）
            if (IsfirstAttention)
            {
                return null;
            }

            //类型必须为关注红包
            //不能为失效状态
            //还未结束
            //已经开始
            //OpenId为空
            //没有存到预存款
            var model = DbFactory.Default
                .Get<BonusReceiveInfo>()
                .InnerJoin<BonusInfo>((bri, bi) => bi.Id == bri.BonusId)
                .Where<BonusInfo>(p => p.Type == BonusInfo.BonusType.Attention && p.IsInvalid == false &&
                    p.EndTime >= DateTime.Now && p.StartTime <= DateTime.Now)
                .Where(p => p.OpenId.ExIfNull("")=="" && p.IsTransformedDeposit == false);
            
            var receive = model.FirstOrDefault();
            if (receive != null)  //存在符合条件的关注送红包
            {
                var bonus= Get(receive.BonusId);
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        bonus.ReceiveCount += 1;
                        bonus.ReceivePrice += receive.Price;
                        receive.OpenId = openId;
                        receive.ReceiveTime = DateTime.Now;
                        receive.IsTransformedDeposit = false;
                        DbFactory.Default.Update(bonus);
                        DbFactory.Default.Update(receive);
                    });
                Task.Factory.StartNew(() =>
                {
                    DepositToMember(openId, receive.Price);
                    GetSuccessSendWXMessage(receive, openId);
                });
                string content = "";
                //content = string.Format("感谢关注，您已获得预存款{0}元，通过此公众号进入商城可用预存款购买商品或提现", receive.Price);
                return content;
            }
            return null;
        }

        /// <summary>
        /// 生成红包详情
        /// </summary> 
        private void GenerateBonusDetail(Mall.Entities.BonusInfo model)
        {
            GenerateDetailContext Generate = new GenerateDetailContext(model);
            Generate.Generate();
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        private string GenerateQR(string path)
        {
            Bitmap bi = Mall.Core.Helper.QRCodeHelper.Create(path);
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string fileFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Plat", "Bonus");
            string fileFullPath = Path.Combine(fileFolderPath, fileName);
            if (!Directory.Exists(fileFolderPath))
            {
                Directory.CreateDirectory(fileFolderPath);
            }
            bi.Save(fileFullPath);

            return "/Storage/Plat/Bonus/" + fileName;
        }

        public bool IsAttention(string openId)
        {
            var model = DbFactory.Default.Get<OpenIdInfo>().Where(p => p.OpenId == openId).FirstOrDefault();
            //从本地数据里判断是否关注
            if (model != null)
            {
                return model.IsSubscribe;
            }
            return IsAttentionByRPC(openId);
        }

        public void SetShare(long id, string openId)
        {
            var model = DbFactory.Default
                .Set<BonusReceiveInfo>()
                .Set(n => n.IsShare, true)
                .Where(p => p.BonusId == id && p.OpenId == openId)
                .Succeed();
        }

        public void SetShareByUserId(long id, bool isShare, long userId)
        {
            var model = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => p.BonusId == id && p.IsShare == isShare).FirstOrDefault();
            model.IsShare = true;
            model.UserId = userId;
            model.ReceiveTime = DateTime.Now;
            model.IsTransformedDeposit = true;
            DbFactory.Default.Update(model);

            decimal price = GetReceivePriceByUserId(id, userId);
            WeiDepositToMember(userId, price);
        }

        /// <summary>
        /// 访问微信接口查看是否关注
        /// </summary>
        private bool IsAttentionByRPC(string openId)
        {
            var siteSetting = SiteSettingApplication.SiteSettings;
            string accessToken = "";
            if (!string.IsNullOrEmpty(siteSetting.WeixinAppId) || !string.IsNullOrEmpty(siteSetting.WeixinAppSecret))
            {
                accessToken = AccessTokenContainer.TryGetAccessToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
            }
            else
            {
                throw new MallException("未配置微信相关信息");
            }

            var result = UserApi.Info(accessToken, openId);
            if (result.errcode == Senparc.Weixin.ReturnCode.不合法的OpenID || result.subscribe == 0)
            {
                return false;
            }
            else if (result.errcode != 0)
            {
                throw new Exception(result.errmsg);
            }
            return result.subscribe == 1;
        }


        /// <summary>
        /// 将红包金额存到预存款里  (领取红包时执行)
        /// 只会出现一条红包记录
        /// </summary>
        private void DepositToMember(string openId, decimal price, string unionId = "")
        {
            //查看用户、OpenId关联表里是否存在数据，存在则证明已经绑定过OpenId
            MemberOpenIdInfo model = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.OpenId.ToLower() == openId.ToLower()).FirstOrDefault();
            if (model == null && !string.IsNullOrEmpty(unionId))
            {
                model = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UnionId.ToLower() == unionId.ToLower()).FirstOrDefault();
            }
            if (model != null)
            {
                var receive = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => (p.OpenId.ToLower() == openId.ToLower() || p.OpenId.ToLower() == model.UnionId.ToLower()) && p.IsTransformedDeposit == false).FirstOrDefault();
                if (receive == null || receive.Id <= 0)
                    return;//没有未领取红包提前跳出

                receive.IsTransformedDeposit = true;
                receive.UserId = model.UserId;
                DbFactory.Default.Update(receive);

                IMemberCapitalService capitalServicer = Mall.ServiceProvider.Instance<IMemberCapitalService>.Create;
                CapitalDetailModel capita = new CapitalDetailModel
                {
                    UserId = model.UserId,
                    SourceType = Mall.Entities.CapitalDetailInfo.CapitalDetailType.RedPacket,
                    Amount = price,
                    CreateTime = ((DateTime)receive.ReceiveTime).ToString("yyyy-MM-dd HH:mm:ss")
                };
                capitalServicer.AddCapital(capita);
            }
        }

        /// <summary>
        /// 微信活动 将红包金额存到预存款里  (领取红包时执行)
        /// 只会出现一条红包记录
        /// </summary>
        private void WeiDepositToMember(long userId, decimal price)
        {

            IMemberCapitalService capitalServicer = Mall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            CapitalDetailModel capita = new CapitalDetailModel
            {
                UserId = userId,
                SourceType = Mall.Entities.CapitalDetailInfo.CapitalDetailType.RedPacket,
                Amount = price,
                CreateTime = (DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")
            };
            capitalServicer.AddCapital(capita);
        }

        /// <summary>
        /// 获取红包集合
        /// </summary>
        /// <param name="bonusType"></param>
        /// <returns></returns>
        public IEnumerable<BonusInfo> GetBonusByType(BonusInfo.BonusType bonusType)
        {

            return DbFactory.Default
                .Get<BonusInfo>()
                .Where(item => (int)item.Type == (int)bonusType && item.EndTime > DateTime.Now && item.IsInvalid == false)
                .ToList();
        }


        #region  这里需要移到外部  专门用作用户绑定openid时将要执行的一系列操作
        /// <summary>
        /// 将红包金额存到预存款里  (用户注册、绑定微信时执行)
        /// 可能会存在多条红包记录
        /// </summary>
        public void DepositToRegister(long userid)
        {
            DbFactory.Default
                .InTransaction(() =>
                {
                    //查看用户、OpenId关联表里是否存在OpenId，存在则证明已经绑定过OpenId
                    //获取用户所有已经绑定过的OpenId
                    var openInfos = DbFactory.Default.Get<MemberOpenIdInfo>().Where(p => p.UserId == userid && p.OpenId.ExIsNotNull()).ToList();
                    if (openInfos == null || openInfos.Count == 0)
                    {
                        return;
                    }

                    foreach (var o in openInfos)
                    {
                        DepositToRegister(o);
                    }

                    foreach (var o in openInfos)
                    {
                        DepositShopBonus(o);
                    }
                });
        }

        //平台红包存储
        private void DepositToRegister(MemberOpenIdInfo openInfo)
        {
            //获取某个OpenId对应的红包记录(在Mall3.3版本，考虑到小程序首页配置了现金红包跳转到红包页面的情况，将之前的openId改为UnionId，并兼容以前的数据)
            var receives = DbFactory.Default.Get<BonusReceiveInfo>().Where(p => (p.OpenId.ToLower() == openInfo.OpenId.ToLower() || p.OpenId.ToLower() == openInfo.UnionId.ToLower()) && p.IsTransformedDeposit == false);
            var list = receives.ToList();
            List<CapitalDetailModel> capitals = new List<CapitalDetailModel>();
            //存在数据则证明有可用红包，可以存到预存款里
            if (list.Count > 0)
            {
                foreach (var model in list)
                {
                    model.IsTransformedDeposit = true;
                    model.UserId = openInfo.UserId;
                    DbFactory.Default.Update(model);

                    CapitalDetailModel capital = new CapitalDetailModel()
                    {
                        UserId = openInfo.UserId,
                        SourceType = Mall.Entities.CapitalDetailInfo.CapitalDetailType.RedPacket,
                        Amount = model.Price,
                        CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    new MemberCapitalService().AddCapital(capital);
                }
            }
            IMemberCapitalService capitalServicer = Mall.ServiceProvider.Instance<IMemberCapitalService>.Create;
            foreach (var c in capitals)
            {
                capitalServicer.AddCapital(c);
            }
        }

        //商家红包存储
        private void DepositShopBonus(MemberOpenIdInfo openInfo)
        {
            var msg = new Exception();
            var flag = DbFactory.Default
                .InTransaction(() =>
                {
                    var receives = DbFactory.Default.Get<ShopBonusReceiveInfo>()
                        .Where(p => p.OpenId.ToLower() == openInfo.OpenId.ToLower() && (p.UserId == 0 || p.UserId.ExIsNull())).ToList();
                    if (receives.Count() <= 0)
                    {
                        return true;
                    }

                    DateTime now = DateTime.Now;
                    foreach (var r in receives)
                    {
                        r.UserId = openInfo.UserId;
                        r.ReceiveTime = now;
                        DbFactory.Default.Update(r);
                    }
                    return true;
                },
                null,
                (ex) => { msg = ex; });
            if (!flag) Log.Info("商家红包存储出错：", msg);
        }
        #endregion

        public BonusInfo Get(long id)
        {
            return DbFactory.Default.Get<BonusInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public decimal GetReceivePriceByOpendId(long id, string openId)
        {
            return DbFactory.Default
                .Get<BonusReceiveInfo>()
                .Where(p => p.OpenId.ToLower() == openId.ToLower() && p.BonusId == id)
                .Select(n => n.Price)
                .FirstOrDefault<decimal>();
        }
        public decimal GetReceivePriceByUserId(long id, long userId)
        {
            return DbFactory.Default
                .Get<BonusReceiveInfo>()
                .Where(p => p.UserId == userId && p.BonusId == id)
                .OrderByDescending(t => t.ReceiveTime)
                .Select(n => n.Price)
                .FirstOrDefault<decimal>();
        }

        public decimal GetFirstReceivePriceByBonus(long bonus)
        {
            return DbFactory.Default.Get<BonusReceiveInfo>().Where(p => p.BonusId == bonus && p.IsShare == false && p.UserId == null).Select(p => p.Price).FirstOrDefault<decimal>();
        }
        /// <summary>
        /// 获取红包剩余数量
        /// </summary>
        public string GetBonusSurplus(long bonusId)
        {
            return DbFactory.Default
                .Get<BonusReceiveInfo>()
                .Where(p => p.BonusId == bonusId && p.UserId.ExIsNull())
                .Count().ToString();
        }


    }


}
