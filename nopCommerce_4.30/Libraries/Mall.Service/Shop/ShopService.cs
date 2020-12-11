using Mall.CommonModel;
using Mall.Core;
using Mall.Core.Plugins.Message;
using Mall.DTO;
using Mall.IServices;
using Mall.DTO.QueryModel;
using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mall.Application;
using Microsoft.AspNetCore.Hosting;
using Nop.Core.Infrastructure;

namespace Mall.Service
{
    public class ShopService : ServiceBase, IShopService
    {


        private readonly IWebHostEnvironment _hostingEnvironment  = EngineContext.Current.Resolve<IWebHostEnvironment>();

        public ShopService(/*IWebHostEnvironment hostingEnvironment*/)
        {
          //  _hostingEnvironment = hostingEnvironment;
        }




        #region 实现店铺服务
        public QueryPageModel<ShopInfo> GetSellers(SellerQuery query)
        {
            var db = DbFactory.Default.Get<ShopInfo>();
            if (!string.IsNullOrEmpty(query.ShopName))
                db.Where(item => item.ShopName.Contains(query.ShopName));
            if (query.RegionId.HasValue)
                db.Where(item => item.CompanyRegionId >= query.RegionId.Value);
            if (query.NextRegionId.HasValue)
                db.Where(item => item.CompanyRegionId < query.NextRegionId.Value);
            if (query.Ids != null && query.Ids.Count() > 0)
                db = db.Where(item => item.Id.ExIn(query.Ids));
            if (query.ShopId.HasValue)
                db = db.Where(item => item.Id != query.ShopId.Value);

            db = db.Where(item => item.ShopStatus == ShopInfo.ShopAuditStatus.Open && item.Stage == ShopInfo.ShopStage.Finish);

            var models = db.OrderByDescending(item => item.Id).ToPagedList(query.PageNo, query.PageSize);
            QueryPageModel<ShopInfo> pageModel = new QueryPageModel<ShopInfo>()
            {
                Models = models,
                Total = models.TotalRecordCount
            };
            return pageModel;
        }

        public QueryPageModel<ShopInfo> GetShops(ShopQuery query)
        {
            var db = WhereBuilder(query);
            switch (query.Sort.ToLower())
            {
                case "enddate":
                    if (query.IsAsc) db.OrderBy(p => p.EndDate);
                    else db.OrderByDescending(p => p.EndDate);
                    break;
                case "balance":
                    db.LeftJoin<ShopAccountInfo>((s, a) => s.Id == a.ShopId);
                    if (query.IsAsc) db.OrderBy<ShopAccountInfo>(p => p.Balance);
                    else db.OrderByDescending<ShopAccountInfo>(p => p.Balance);
                    break;
                case "ordercount":
                    //var orders = DbFactory.Default.Get<OrderInfo>().Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.Finish)
                    //    .Where<ShopInfo>((oi, si) => si.Id == oi.ShopId).GroupBy(p => p.ShopId).Select<OrderInfo>(p => p.Id.ExCount(false));
                    //db.Select().Select(p => new { OrderCount = orders.ExResolve<int>() }).OrderByDescending(p => "OrderCount");

                    var orderIds = DbFactory.Default.Get<OrderInfo>().Where(p => p.OrderStatus == OrderInfo.OrderOperateStatus.Finish).Select(a => a.Id);
                    var orderItems = DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).Where<ShopInfo>((oi, si) => si.Id == oi.ShopId)
                        .GroupBy(p => p.ShopId).Select<OrderItemInfo>(p => (p.Quantity - p.ReturnQuantity).ExSum());

                    //var productIds = DbFactory.Default.Get<ProductInfo>().Where(p => p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && p.AuditStatus == ProductInfo.ProductAuditStatus.Audited).Select(a=>a.Id);
                    var productIds = DbFactory.Default.Get<ProductInfo>().Select(a => a.Id);
                    var products = DbFactory.Default.Get<ProductInfo>().Where(p => p.Id.ExIn(productIds))
                        .Where<ShopInfo>((oi, si) => si.Id == oi.ShopId)
                        .GroupBy(p => p.ShopId)
                        .Select<ProductInfo>(p => p.VirtualSaleCounts.ExSum());

                    db.Select().Select(p => new { OrderCount = orderItems.ExResolve<int>() + products.ExResolve<long>() }).OrderByDescending(p => "OrderCount");
                    break;
                default:
                    db.OrderByDescending(o => o.ShopStatus);
                    break;
            }

            var models = db.ToPagedList(query.PageNo, query.PageSize);
            var shops = models.Select(p => p.Id).ToList();
            var managers = DbFactory.Default.Get<ManagerInfo>().Where(p => p.ShopId.ExIn(shops)).ToList();
            foreach (var item in models)
            {
                var manager = managers.Where(p => p.ShopId == item.Id && p.RoleId == 0).FirstOrDefault();
                item.ShopAccount = manager == null ? "" : manager.UserName;
            }
            return new QueryPageModel<ShopInfo>()
            {
                Models = models,
                Total = models.TotalRecordCount
            };
        }

        public int GetShopCount(ShopQuery query)
        {
            var db = WhereBuilder(query);
            return db.Count();
        }

        public GetBuilder<ShopInfo> WhereBuilder(ShopQuery query)
        {
            var db = DbFactory.Default.Get<ShopInfo>().Where(item => item.GradeId > 0);
            if (query.ShopGradeId > 0)
                db.Where(item => item.GradeId == (query.ShopGradeId.Value));
            if (query.BrandId > 0)
            {
                var brands = DbFactory.Default.Get<ShopBrandInfo>().Where(a => a.BrandId == query.BrandId).Select(p => p.ShopId).ToList<long>();
                if (brands.Count > 0) db.Where(item => item.Id.ExIn(brands));
            }
            if (query.CategoryId > 0)
            {
                var products = DbFactory.Default.Get<ProductInfo>().Where(a => a.CategoryId == query.CategoryId).Select(p => p.ShopId).ToList<long>();
                if (products.Count > 0) db.Where(item => item.Id.ExIn(products));
            }

            if (!string.IsNullOrWhiteSpace(query.ShopName))
                db.Where(item => item.ShopName.Contains(query.ShopName));

            if (!string.IsNullOrWhiteSpace(query.ShopAccount))
            {
                var ms = DbFactory.Default.Get<ManagerInfo>().Where(m => m.UserName == query.ShopAccount && m.ShopId != 0 && m.RoleId == 0).Select(p => p.ShopId).ToList<long>();
                if (ms.Count > 0) db.Where(item => item.Id.ExIn(ms));
            }

            if (query.Stage.HasValue)
                db.Where(p => p.Stage == query.Stage.Value);

            if (query.CreateDateBegin.HasValue)
                db.Where(p => p.CreateDate >= query.CreateDateBegin.Value);
            if (query.CreateDateEnd.HasValue)
                db.Where(p => p.CreateDate < query.CreateDateEnd.Value);

            if (query.ExpiredDateEnd.HasValue)
                db.Where(p => p.EndDate < query.ExpiredDateEnd.Value);

            if (query.Status.HasValue && query.Status.Value != ShopInfo.ShopAuditStatus.Default)
            {
                var _stwhere = PredicateExtensions.False<ShopInfo>();
                var _status = query.Status.Value;
                var edv = DateTime.Now.Date.AddSeconds(-1);
                switch (_status)
                {
                    case ShopInfo.ShopAuditStatus.Open:
                        db.Where(d => d.ShopStatus == ShopInfo.ShopAuditStatus.Open && d.EndDate > edv && d.Stage == ShopInfo.ShopStage.Finish);
                        break;
                    case ShopInfo.ShopAuditStatus.HasExpired:
                        db.Where(d => (d.ShopStatus == ShopInfo.ShopAuditStatus.Open && d.EndDate < edv) || (d.ShopStatus == ShopInfo.ShopAuditStatus.HasExpired));
                        break;
                    default:
                        _stwhere = _stwhere.Or(d => d.ShopStatus == _status);
                        if (query.MoreStatus != null && query.MoreStatus.Count > 0)
                        {
                            foreach (var qsitem in query.MoreStatus)
                            {
                                _stwhere = _stwhere.Or(d => d.ShopStatus == qsitem);
                            }
                        }
                        db.Where(_stwhere);
                        break;
                }
            }
            return db;
        }
        /// <summary>
        /// 获取所有已审核门店
        /// </summary>
        /// <returns></returns>
        public List<ShopInfo> GetAllShops()
        {
            var shops = DbFactory.Default
                .Get<ShopInfo>()
                .Where(x => x.ShopStatus != ShopInfo.ShopAuditStatus.WaitAudit && x.ShopStatus != ShopInfo.ShopAuditStatus.Refuse &&
                    x.ShopStatus != ShopInfo.ShopAuditStatus.Unusable && x.ShopStatus != ShopInfo.ShopAuditStatus.WaitConfirm &&
                    x.ShopStatus != ShopInfo.ShopAuditStatus.HasExpired);
            return shops.ToList();
        }

        /// <summary>
        /// 检测并初始店铺模板
        /// </summary>
        /// <param name="shopId"></param>
        public void CheckInitTemplate(long shopId)
        {
            CopyTemplate(shopId);
        }

        //TODO:ZJT 获取店铺有效的订单总销量
        public long GetSales(long id)
        {
            var orderIds = DbFactory.Default.Get<OrderInfo>().Where(p => p.ShopId == id && p.OrderStatus == OrderInfo.OrderOperateStatus.Finish).Select(a => a.Id);
            if (orderIds.Count() > 0)
                return DbFactory.Default.Get<OrderItemInfo>().Where(p => p.OrderId.ExIn(orderIds)).Sum(a => a.Quantity - a.ReturnQuantity);
            return 0;
        }

        public List<ShopBrand> GetShopBrands(List<long> shops)
        {
            return DbFactory.Default.Get<ShopBrandInfo>()
                .LeftJoin<BrandInfo>((sb, b) => sb.BrandId == b.Id)
                .Where(p => p.ShopId.ExIn(shops))
                .Select().Select<BrandInfo>(p => new { BrandName = p.Name })
                .ToList<ShopBrand>();
        }

        public ShopInfo GetShop(long id, bool businessCategoryOn = false)
        {
            var shop = Cache.Get<ShopInfo>(CacheKeyCollection.CACHE_SHOP(id, businessCategoryOn));
            if (shop != null) return shop;

            var manager = DbFactory.Default.Get<ManagerInfo>().Where<ShopInfo>((mi, si) => mi.ShopId == si.Id).Where(p => p.RoleId == 0)
                    .Select(n => n.UserName.ExIfNull("")).Take(1);
            shop = DbFactory.Default.Get<ShopInfo>()
                .Where(p => p.Id == id)
                .Select()
                .Select(n => new { ShopAccount = manager.ExResolve<string>() })
                .FirstOrDefault();
            if (null == shop) return null;

            if (businessCategoryOn)
            {
                shop.BusinessCategory = new Dictionary<long, decimal>();
                var businessCategory = GetBusinessCategory(id);
                foreach (var item in businessCategory)
                {
                    if (!shop.BusinessCategory.ContainsKey(item.CategoryId))
                    {
                        shop.BusinessCategory.Add(item.CategoryId, item.CommisRate);
                    }
                }
            }
            Cache.Insert(CacheKeyCollection.CACHE_SHOP(id, businessCategoryOn), shop, 600);
            return shop;
        }

        /// <summary>
        /// 根据id获取门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ShopInfo> GetShops(IEnumerable<long> ids)
        {
            return DbFactory.Default.Get<ShopInfo>().Where(p => p.Id.ExIn(ids)).ToList();
        }

        public ShopInfo GetShopBasicInfo(long id)
        {
            return DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public void DeleteShop(long id)
        {
            DbFactory.Default.Del<ShopInfo>(p => p.Id == id);
        }

        public void UpdateShop(ShopInfo shop)
        {
            //TODO:FG 此方法影响范围过大,逻辑不明确 待移除
            //说明：
            //     店铺的更新只会影响如下字段
            //     1. ShopName     (店铺名称)
            //     2. ShopGradeId  (店铺等级)
            //     3. EndDate      (有效期)
            //     4. ShopStatus   (店铺状态)

            //var actual = Context.ShopInfo.FirstOrDefault(p => p.Id == shop.Id);
            var actual = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shop.Id).FirstOrDefault();
            //名称如改变，则修改其他几个表商铺名称
            if (!string.IsNullOrEmpty(shop.ShopName) && !shop.ShopName.Equals(actual.ShopName))
            {
                ShopApplication.UpdateOtherTableShopName(shop.Id, shop.ShopName);
            }
            actual.ShopName = shop.ShopName ?? actual.ShopName;
            actual.GradeId = shop.GradeId == 0 ? actual.GradeId : shop.GradeId;
            actual.EndDate = shop.EndDate;
            actual.ShopStatus = shop.ShopStatus == Entities.ShopInfo.ShopAuditStatus.Default ? actual.ShopStatus : shop.ShopStatus;
            actual.Stage = shop.Stage;
            actual.BankAccountName = shop.BankAccountName ?? actual.BankAccountName;
            actual.BankAccountNumber = shop.BankAccountNumber ?? actual.BankAccountNumber;
            actual.BankCode = shop.BankCode ?? actual.BankCode;
            actual.BankName = shop.BankName ?? actual.BankName;
            //    actual.BankPhoto = shop.BankPhoto ?? actual.BankPhoto; //开户银行许可证电子版
            actual.BankRegionId = shop.BankRegionId == 0 ? actual.BankRegionId : shop.BankRegionId;
            actual.CompanyRegionId = shop.CompanyRegionId == 0 ? actual.CompanyRegionId : shop.CompanyRegionId;
            actual.CompanyRegisteredCapital = shop.CompanyRegisteredCapital == 0 ? actual.CompanyRegisteredCapital : shop.CompanyRegisteredCapital;
            actual.BusinessLicenceEnd = shop.BusinessLicenceEnd ?? actual.BusinessLicenceEnd;
            actual.BusinessLicenceStart = shop.BusinessLicenceStart ?? actual.BusinessLicenceStart;
            actual.legalPerson = shop.legalPerson ?? actual.legalPerson;

            if (shop.CompanyFoundingDate.HasValue)
                actual.CompanyFoundingDate = shop.CompanyFoundingDate.HasValue ? shop.CompanyFoundingDate.Value : actual.CompanyFoundingDate.Value;
            actual.Logo = shop.Logo ?? actual.Logo;
            actual.SubDomains = shop.SubDomains ?? actual.SubDomains;
            actual.Theme = shop.Theme ?? actual.Theme;
            actual.BusinessLicenceRegionId = shop.BusinessLicenceRegionId == 0 ? actual.BusinessLicenceRegionId : shop.BusinessLicenceRegionId;
            actual.CompanyEmployeeCount = shop.CompanyEmployeeCount == 0 ? actual.CompanyEmployeeCount : shop.CompanyEmployeeCount;
            actual.BusinessLicenceNumber = shop.BusinessLicenceNumber ?? actual.BusinessLicenceNumber;
            // actual.BusinessLicenceNumberPhoto = shop.BusinessLicenceNumberPhoto ?? actual.BusinessLicenceNumberPhoto;//营业执照
            actual.BusinessSphere = shop.BusinessSphere ?? actual.BusinessSphere;
            actual.CompanyAddress = shop.CompanyAddress ?? actual.CompanyAddress;
            actual.CompanyName = shop.CompanyName ?? actual.CompanyName;
            actual.CompanyPhone = shop.CompanyPhone ?? actual.CompanyPhone;

            actual.ContactsEmail = shop.ContactsEmail ?? actual.ContactsEmail;
            actual.ContactsName = shop.ContactsName ?? actual.ContactsName;
            actual.ContactsPhone = shop.ContactsPhone ?? actual.ContactsPhone;
            actual.OrganizationCode = shop.OrganizationCode ?? actual.OrganizationCode;
            //  actual.OrganizationCodePhoto = shop.OrganizationCodePhoto ?? actual.OrganizationCodePhoto;//组织机构代码证
            // actual.GeneralTaxpayerPhot = shop.GeneralTaxpayerPhot ?? actual.GeneralTaxpayerPhot;//一般纳税人证明
            actual.TaxpayerId = shop.TaxpayerId ?? actual.TaxpayerId;
            actual.TaxRegistrationCertificate = shop.TaxRegistrationCertificate ?? actual.TaxRegistrationCertificate;
            //   actual.TaxRegistrationCertificatePhoto = shop.TaxRegistrationCertificatePhoto ?? actual.TaxRegistrationCertificatePhoto; 
            //actual.PayPhoto = shop.PayPhoto ?? actual.PayPhoto;
            actual.PayRemark = shop.PayRemark ?? actual.PayRemark;
            actual.SenderAddress = shop.SenderAddress ?? actual.SenderAddress;
            actual.SenderName = shop.SenderName ?? actual.SenderName;
            actual.SenderPhone = shop.SenderPhone ?? actual.SenderPhone;

            //个人认证
            actual.BusinessType = shop.BusinessType;
            actual.IDCard = shop.IDCard ?? actual.IDCard;
            actual.WeiXinNickName = shop.WeiXinNickName ?? actual.WeiXinNickName;
            actual.WeiXinSex = shop.WeiXinSex ?? actual.WeiXinSex;
            actual.WeiXinAddress = shop.WeiXinAddress ?? actual.WeiXinAddress;
            actual.WeiXinTrueName = shop.WeiXinTrueName ?? actual.WeiXinTrueName;
            actual.WeiXinOpenId = shop.WeiXinOpenId ?? actual.WeiXinOpenId;
            actual.WeiXinImg = shop.WeiXinImg ?? shop.WeiXinImg;
            actual.AutoAllotOrder = shop.AutoAllotOrder;//商家是否开启订单自动分配门店
            //actual.ProvideInvoice = shop.ProvideInvoice;//商家是否提供发票

            //TODO:DZY[160225]  防联动修改提前保存值
            var BusinessLicenseCert = shop.BusinessLicenseCert;
            var ProductCert = shop.ProductCert;
            var OtherCert = shop.OtherCert;

            //TODO:DZY[160225]  此处如果传进来的shop不是一个new对象，而是从ef获取数据就会形成联动修改。

            if (!string.IsNullOrEmpty(shop.BusinessLicenseCert))
                actual.BusinessLicenseCert = "/Storage/Shop/" + shop.Id + "/Cert/BusinessLicenseCert";//经营许可类证书
            if (!string.IsNullOrEmpty(shop.ProductCert))
                actual.ProductCert = "/Storage/Shop/" + shop.Id + "/Cert/ProductCert";//产品类证书
            if (!string.IsNullOrEmpty(shop.OtherCert))
                actual.OtherCert = "/Storage/Shop/" + shop.Id + "/Cert/OtherCert";//其它证书

            if (!string.IsNullOrEmpty(shop.TaxRegistrationCertificatePhoto))
                actual.TaxRegistrationCertificatePhoto = MoveImages(shop.Id, shop.TaxRegistrationCertificatePhoto, "TaxRegistrationCertificatePhoto", 1); //税务登记证
            if (!string.IsNullOrEmpty(shop.BusinessLicenceNumberPhoto))
                actual.BusinessLicenceNumberPhoto = MoveImages(shop.Id, shop.BusinessLicenceNumberPhoto, "BusinessLicenceNumberPhoto", 1);//营业执照
            if (!string.IsNullOrEmpty(shop.OrganizationCodePhoto))
                actual.OrganizationCodePhoto = MoveImages(shop.Id, shop.OrganizationCodePhoto, "OrganizationCodePhoto", 1);//组织机构代码证
            if (!string.IsNullOrEmpty(shop.GeneralTaxpayerPhot))
                actual.GeneralTaxpayerPhot = MoveImages(shop.Id, shop.GeneralTaxpayerPhot, "GeneralTaxpayerPhoto", 1); //一般纳税人证明
            if (!string.IsNullOrEmpty(shop.BankPhoto))
                actual.BankPhoto = MoveImages(shop.Id, shop.BankPhoto, "BankPhoto", 1); //一般纳税人证明
            if (!string.IsNullOrEmpty(shop.PayPhoto))
                actual.PayPhoto = MoveImages(shop.Id, shop.PayPhoto, "PayPhoto", 1); //付款凭证

            if (!string.IsNullOrEmpty(shop.IDCardUrl))
                actual.IDCardUrl = MoveImages(shop.Id, shop.IDCardUrl, "IDCardUrl", 1);
            if (!string.IsNullOrEmpty(shop.IDCardUrl2))
                actual.IDCardUrl2 = MoveImages(shop.Id, shop.IDCardUrl2, "IDCardUrl", 2);

            //Context.SaveChanges();
            DbFactory.Default.Update(actual);
            //过期店铺下架所有商品
            if (actual.EndDate < DateTime.Now.AddDays(1).Date)
            {
                SaleOffAllProduct(actual.Id);
                CloseAllMarketingAction(actual.Id);
            }

            if (!string.IsNullOrEmpty(BusinessLicenseCert))
            {
                var arr = BusinessLicenseCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "BusinessLicenseCert", index);
                }

            }
            if (!string.IsNullOrEmpty(ProductCert))
            {
                var arr = ProductCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "ProductCert", index);
                }
            }
            if (!string.IsNullOrEmpty(OtherCert))
            {
                var arr = OtherCert.Split(',');
                var index = 0;
                foreach (var i in arr)
                {
                    index++;
                    MoveImages(shop.Id, i, "OtherCert", index);
                }
            }

            var msg = ServiceProvider.Instance<IMessageService>.Create;
            var email = PluginsManagement.GetPlugins<IEmailPlugin>().FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(actual.ContactsEmail))
            {
                msg.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = actual.ContactsEmail,
                    ServiceProvider = email.PluginInfo.PluginId,
                    UserId = actual.Id,
                    UserType = Entities.MemberContactInfo.UserTypes.ShopManager
                });
            }

            var sms = PluginsManagement.GetPlugins<ISMSPlugin>().FirstOrDefault();
            if (sms != null && !String.IsNullOrWhiteSpace(actual.ContactsPhone))
            {
                msg.UpdateMemberContacts(new Entities.MemberContactInfo()
                {
                    Contact = actual.ContactsPhone,
                    ServiceProvider = sms.PluginInfo.PluginId,
                    UserId = actual.Id,
                    UserType = MemberContactInfo.UserTypes.ShopManager
                });
            }
            ClearShopCache(shop.Id);
        }
        public void ClearShopCache(long id)
        {
            //更新操作要处理相关缓存
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOP(id, false));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(id, true));
            Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(id, false));
        }

        public void SetProvideInvoice(ShopInvoiceConfigInfo info)
        {
            var config = DbFactory.Default.Get<ShopInvoiceConfigInfo>().Where(p => p.ShopId == info.ShopId).FirstOrDefault();
            if(config != null)
            {
                config.IsInvoice = info.IsInvoice;
                config.IsPlainInvoice = info.IsPlainInvoice;
                config.IsElectronicInvoice = info.IsElectronicInvoice;
                config.IsVatInvoice = info.IsVatInvoice;
                config.PlainInvoiceRate = info.PlainInvoiceRate;
                config.VatInvoiceDay = info.VatInvoiceDay;
                config.VatInvoiceRate = info.VatInvoiceRate;
                DbFactory.Default.Update<ShopInvoiceConfigInfo>(config);
            }
            else
            {
                DbFactory.Default.Add<ShopInvoiceConfigInfo>(info);
            }
            //DbFactory.Default.Set<ShopInvoiceConfigInfo>().Where(p => p.ShopId == id)
            //    .Set(p => p.ProvideInvoice, enable)
            //    .Succeed();
        }

        public void SetAutoAllotOrder(long id, bool enable)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == id)
              .Set(p => p.AutoAllotOrder, enable)
              .Succeed();
        }
        public void SetBusinessType(long id, ShopBusinessType type)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == id)
              .Set(p => p.BusinessType, type)
              .Succeed();
        }
        public void SetBankAccount(BankAccount account)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == account.ShopId)
                .Set(p => p.BankAccountName, account.BankAccountName)
                .Set(p => p.BankAccountNumber, account.BankAccountNumber)
                .Set(p => p.BankCode, account.BankCode ?? "")
                .Set(p => p.BankName, account.BankName)
                .Set(p => p.BankRegionId, account.BankRegionId)
                .Succeed();
        }
        public void UpdateBankAccount(BankAccount account)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == account.ShopId)
                .Set(p => p.BankAccountName, account.BankAccountName)
                .Set(p => p.BankAccountNumber, account.BankAccountNumber)
                .Set(p => p.BankName, account.BankName)
                .Succeed();
        }

        public void SetWeChatAccount(WeChatAccount model)
        {
            var sex = model.Sex == "男" ? 1 : 0;
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == model.ShopId)
                .Set(p => p.WeiXinOpenId, model.WeiXinOpenId)
                .Set(p => p.WeiXinTrueName, model.WeiXinRealName)
                .Set(p => p.WeiXinNickName, model.WeiXinNickName)
                .Set(p => p.WeiXinAddress, model.Address)
                .Set(p => p.WeiXinImg, model.Logo)
                .Set(p => p.WeiXinSex, sex)
                .Succeed();
        }

        public void SetLicenseCert(ShopLicenseCert model)
        {
            var db = DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == model.ShopId);

            if (!string.IsNullOrEmpty(model.BusinessLicenseCert))
            {
                var arr = model.BusinessLicenseCert.Split(',');
                for (int i = 0; i < arr.Length; i++)
                    MoveImages(model.ShopId, arr[i], "BusinessLicenseCert", i);

                var val = string.Format("/Storage/Shop/{0}/Cert/BusinessLicenseCert", model.ShopId);
                db.Set(p => p.BusinessLicenseCert, val);
            }

            if (!string.IsNullOrEmpty(model.ProductCert))
            {
                var arr = model.ProductCert.Split(',');
                for (int i = 0; i < arr.Length; i++)
                    MoveImages(model.ShopId, arr[i], "ProductCert", i);

                var val = string.Format("/Storage/Shop/{0}/Cert/ProductCert", model.ShopId);
                db.Set(p => p.ProductCert, val);
            }
            if (!string.IsNullOrEmpty(model.OtherCert))
            {
                var arr = model.OtherCert.Split(',');
                for (int i = 0; i < arr.Length; i++)
                    MoveImages(model.ShopId, arr[i], "OtherCert", i);

                var val = string.Format("/Storage/Shop/{0}/Cert/OtherCert", model.ShopId);
                db.Set(p => p.OtherCert, val);
            }
            db.Succeed();
        }
        string MoveImages(long shopId, string image, string name, int index = 1)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "";
            }
            string OriUrl = image;
            var ext = ".png";
            string ImageDir = string.Empty;

            var path = "/Storage/Shop/" + shopId + "/Cert/";
            //转移图片

            var fileName = name + index + ext;

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var img = image.Substring(image.LastIndexOf("/temp"));
                Core.MallIO.CopyFile(img, path + fileName, true);
            }  //目标地址
            return path + fileName;
        }

        public void UpdateShop(ShopInfo shop, List<long> categoryIds)
        {
            var categorys = GetThirdLevelCategories(categoryIds);//获取所有三级分类，分佣精确到三级分类
            var flag = DbFactory.Default.InTransaction(() =>
            {
                shop.ShopStatus = ShopInfo.ShopAuditStatus.WaitAudit;
                UpdateShop(shop);
                UpdateBusinessCategory(shop.Id, categorys);
                return true;
            });
            ClearShopCache(shop.Id);
        }

        List<long> GetThirdLevelCategories(List<long> categoryIds)
        {
            var categoryService = ServiceProvider.Instance<ICategoryService>.Create;
            var allCateogries = categoryService.GetCategories();
            List<long> validCategoryIds = new List<long>();
            foreach (var categoryId in categoryIds.ToList())
            {
                var category = allCateogries.FirstOrDefault(item => item.Id == categoryId);
                if (category.Depth == 1)
                {
                    var secondCategoryIds = allCateogries.Where(item => item.ParentCategoryId == category.Id).Select(item => item.Id);
                    validCategoryIds.AddRange(allCateogries.Where(item => secondCategoryIds.Contains(item.ParentCategoryId)).Select(item => item.Id));
                }
                else if (category.Depth == 2)
                    validCategoryIds.AddRange(allCateogries.Where(item => item.ParentCategoryId == category.Id).Select(item => item.Id));
                else
                    validCategoryIds.Add(categoryId);
            }
            return validCategoryIds;
        }

        private Dictionary<long, string> GetCategoryNamePath(List<long> ids)
        {
            var source = DbFactory.Default.Get<CategoryInfo>();
            var categories = source.Where(p => p.Id.ExIn(ids)).ToList();
            var result = new Dictionary<long, string>();
            foreach (var item in categories)
                result.Add(item.Id, FullCategoryNamePath(source.ToList(), item.Id));
            return result;

        }
        private string FullCategoryNamePath(List<CategoryInfo> source, long id)
        {
            var category = source.FirstOrDefault(p => p.Id == id);
            if (category == null) return string.Empty;
            if (category.ParentCategoryId == 0 || category.Depth == 1)
                return category.Name;
            return FullCategoryNamePath(source, category.ParentCategoryId) + " > " + category.Name;
        }



        public List<BusinessCategoryInfo> GetBusinessCategory(long shop)
        {

            var categories = DbFactory.Default.Get<BusinessCategoryInfo>().Where(b => b.ShopId == shop).ToList();
            var map = GetCategoryNamePath(categories.Select(p => p.CategoryId).ToList());
            categories.ForEach(item =>
            {
                if (map.ContainsKey(item.CategoryId))
                    item.CategoryName = map[item.CategoryId];
            });
            return categories;
        }

        public QueryPageModel<BusinessCategoryInfo> GetBusinessCategory(long shop, int pageNo, int pageSize)
        {
            var categoryIds = DbFactory.Default.Get<CategoryInfo>().Where(a => a.IsDeleted == false).Select(p => p.Id).ToList<long>();

            var categories = DbFactory.Default.Get<BusinessCategoryInfo>().Where(b => b.ShopId == shop && b.CategoryId.ExIn(categoryIds)).ToPagedList(pageNo, pageSize);
            var map = GetCategoryNamePath(categories.Select(p => p.CategoryId).ToList());
            categories.ForEach(item =>
            {
                if (map.ContainsKey(item.CategoryId))
                    item.CategoryName = map[item.CategoryId];
            });
            return new QueryPageModel<BusinessCategoryInfo>
            {
                Models = categories,
                Total = categories.TotalRecordCount
            };
        }

        private void UpdateBusinessCategory(long shopId, List<long> categoryIds)
        {
            DbFactory.Default.Del<BusinessCategoryInfo>(b => b.ShopId == shopId);
            var categories = DbFactory.Default.Get<CategoryInfo>().Where(item => item.Id.ExIn(categoryIds.Distinct())).ToList();
            var businessCategoies = categoryIds.Select(item =>
            {
                var category = categories.FirstOrDefault(t => t.Id == item);
                return new BusinessCategoryInfo()
                {
                    CategoryId = item,
                    ShopId = shopId,
                    CommisRate = category.CommisRate
                };
            });

            DbFactory.Default.Add(businessCategoies);
        }

        /// <summary>
        /// 是否可以删除经营类目
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="bCategoryId"></param>
        /// <returns></returns>
        public bool CanDeleteBusinessCategory(long shopId, long bCategoryId)
        {
            bool result = true;
            var bcategory = DbFactory.Default.Get<BusinessCategoryInfo>().Where(b => b.ShopId == shopId && b.CategoryId == bCategoryId).FirstOrDefault();
            if (bcategory != null)
            {
                var pids = DbFactory.Default.Get<ProductInfo>().Where(p => p.CategoryId == bCategoryId)
                    .Where<OrderItemInfo>((pi, oi) => oi.ProductId == pi.Id);
                result = !DbFactory.Default.Get<OrderItemInfo>().Where(d => d.ShopId == shopId && d.ExExists(pids)).Exist();
            }
            return result;
        }


        public void SaveBusinessCategory(long id, Dictionary<long, decimal> bCategoryList)
        {
            DbFactory.Default.Del<BusinessCategoryInfo>(b => b.ShopId.Equals(id));
            foreach (var item in bCategoryList)
            {
                DbFactory.Default.Add(new BusinessCategoryInfo
                {
                    CategoryId = item.Key,
                    CommisRate = item.Value,
                    ShopId = id
                });
            }
        }

        private void CopyFolder(string from, string to)
        {
            if (!Directory.Exists(to))
                Directory.CreateDirectory(to);

            // 子文件夹
            foreach (string sub in Directory.GetDirectories(from))
                CopyFolder(sub + "\\", to + Path.GetFileName(sub) + "\\");

            // 文件
            foreach (string file in Directory.GetFiles(from))
                File.Copy(file, to + Path.GetFileName(file), true);
        }

        private void CopyTemplate(long shopId)
        {
            string templatePath = _hostingEnvironment.ContentRootPath + string.Format("\\Areas\\SellerAdmin\\Templates\\vshop\\{0}\\", shopId);

            bool isExist = Directory.Exists(templatePath);
            if (!isExist)
            {
                Directory.CreateDirectory(templatePath);
                CopyFolder(_hostingEnvironment.ContentRootPath + "\\Template", templatePath);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="status"></param>
        /// <param name="comments"></param>
        public void UpdateShopStatus(long shopId, ShopInfo.ShopAuditStatus status, string comments = "", int TrialDays = 0)
        {
            //var shop = Context.ShopInfo.FirstOrDefault(p => p.Id == shopId);
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId).FirstOrDefault();
            shop.ShopStatus = status;
            if (!string.IsNullOrWhiteSpace(comments))
                shop.RefuseReason = comments;
            //官方自营永不过期
            if (shop.IsSelf) status = ShopInfo.ShopAuditStatus.Open;

            //设置店铺的结束日期
            if (status == ShopInfo.ShopAuditStatus.Open)
            {
                //TODO:DZY[150729] 官方自营店到期自动延期
                /* zjt  
                 * TODO可移除，保留注释即可
                 */
                if (shop.IsSelf)
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Open; //开启官方自营店
                    shop.EndDate = DateTime.Now.AddYears(10);
                }
                else
                {
                    shop.EndDate = DateTime.Now.AddDays(TrialDays);
                }

                //账户初始
                //if (!Context.ShopAccountInfo.Any(a => a.ShopId == shopId))
                if (!DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == shopId).Exist())
                {
                    ShopAccountInfo model = new ShopAccountInfo()
                    {
                        ShopId = shopId,
                        ShopName = shop.ShopName,
                        Settled = 0,
                        PendingSettlement = 0,
                        Balance = 0,
                        ReMark = "",
                    };
                    DbFactory.Default.Add(model);
                }

                //发送通知消息
                var shopMessage = new MessageShopInfo();
                shop.Stage = ShopInfo.ShopStage.Finish;
                shopMessage.ShopId = shopId;
                shopMessage.ShopName = shop.ShopName;
                shopMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                long uid = this.GetShopManagers(shopId);
                //var member = Context.UserMemberInfo.FirstOrDefault(p => p.Id == uid);
                var member = DbFactory.Default.Get<MemberInfo>().Where(p => p.Id == uid).FirstOrDefault();
                shopMessage.UserName = member.UserName;
                Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopAudited(uid, shopMessage));
                CopyTemplate(shopId);
            }
            if (status == ShopInfo.ShopAuditStatus.WaitPay)
            {
                //发送通知消息
                //var shopMessage = new MessageShopInfo();
                //shopMessage.ShopId = shopId;
                //shopMessage.ShopName = shop.ShopName;
                //shopMessage.SiteName = SiteSettingApplication.SiteSettings.SiteName;
                //Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopSuccess(shopId, shopMessage));
            }
            if (status == ShopInfo.ShopAuditStatus.Refuse)
            {
                shop.Stage = ShopInfo.ShopStage.CompanyInfo;
                //shop.ShopName = "";
            }
            shop.CreateDate = DateTime.Now;
            //Context.SaveChanges();
            DbFactory.Default.Update(shop);
            ClearShopCache(shopId);
        }

        public void SetShopFreight(long shopId, decimal freight, decimal freeFreight)
        {
            DbFactory.Default.Set<ShopInfo>()
                .Where(p => p.Id == shopId)
                .Set(n => n.Freight, freight)
                .Set(n => n.FreeFreight, freeFreight)
                .Succeed();
        }

        public void SetCompnayInfo(ShopCompanyInfo model)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == model.ShopId)
                .Set(p => p.CompanyName, model.CompanyName)
                .Set(p => p.CompanyAddress, model.CompanyAddress)
                .Set(p => p.CompanyEmployeeCount, model.CompanyEmployeeCount)
                .Set(p => p.CompanyRegionId, model.CompanyRegionId)
                .Succeed();
        }

        public bool IsExpiredShop(long shopId)
        {
            var date = DateTime.Now.Date.AddDays(-1);

            //店铺过期时间以晚上24点之前为节点，与商品自动下架同步
            return DbFactory.Default.Get<ShopInfo>()
                .Where(p => p.Id == shopId && (p.EndDate < date || p.ShopStatus == ShopInfo.ShopAuditStatus.HasExpired)).Exist();
        }

        /// <summary>
        /// 是否冻结
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool IsFreezeShop(long shopId)
        {
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId).FirstOrDefault();
            if (shop == null)
                throw new MessageException(ExceptionMessages.NoFound, "店铺");
            return shop.ShopStatus == ShopInfo.ShopAuditStatus.Freeze;
        }
        /// <summary>
        /// 是否官方自营店
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public bool IsSelfShop(long shopId)
        {
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId).FirstOrDefault();
            if (shop == null)
                throw new MessageException(ExceptionMessages.NoFound, "店铺");
            return shop.IsSelf;
        }
        #endregion

        #region 实现店铺等级服务

        public List<ShopGradeInfo> GetShopGrades()
        {
            return DbFactory.Default.Get<ShopGradeInfo>().ToList();
        }

        public ShopGradeInfo GetShopGrade(long id)
        {
            return DbFactory.Default.Get<ShopGradeInfo>().Where(p => p.Id == id).FirstOrDefault();
        }

        public void AddShopGrade(ShopGradeInfo shopGrade)
        {
            DbFactory.Default.Add(shopGrade);
        }

        public void DeleteShopGrade(long id)
        {
            if (DbFactory.Default.Get<ShopInfo>().Where(s => s.GradeId == id).Exist())
                throw new MessageException("删除失败，因为该套餐和店铺有关联，所以不能删除.");
            DbFactory.Default.Del<ShopGradeInfo>(p => p.Id == id);
        }

        public void UpdateShopGrade(ShopGradeInfo shopGrade)
        {
            DbFactory.Default.Set<ShopGradeInfo>().Where(p => p.Id == shopGrade.Id)
                .Set(p => p.Name, shopGrade.Name)
                .Set(p => p.ProductLimit, shopGrade.ProductLimit)
                .Set(p => p.ImageLimit, shopGrade.ImageLimit)
                .Set(p => p.ChargeStandard, shopGrade.ChargeStandard)
                .Succeed();
        }
        #endregion

        #region 店铺信息
        /// <summary>
        /// 获取门店使用空间
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public long GetShopUsageSpace(long shopId)
        {
            //TODO:FG 待转移入 Application 层
            var path = string.Format("/Storage/Shop/{0}/Products/", shopId);
            if (!MallIO.ExistDir(path))
                MallIO.CreateDir(path);
            return MallIO.GetDirMetaInfo(path).ContentLength / 1024 / 1024;
        }
        /// <summary>
        /// 获取门店剩余空间
        /// </summary>
        /// <returns></returns>
        public long GetShopSurplusSpace(long shop)
        {
            var useage = GetShopUsageSpace(shop);
            var limit = DbFactory.Default.Get<ShopGradeInfo>()
                .LeftJoin<ShopInfo>((g, s) => g.Id == s.GradeId)
                .Where<ShopInfo>(p => p.Id == shop)
                .Select(p => p.ImageLimit)
                .FirstOrDefault<int>();
            return limit - useage;
        }

        /// <summary>
        /// 获取店铺账户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ShopAccountInfo> GetShopAccounts(List<long> ids)
        {
            return DbFactory.Default.Get<ShopAccountInfo>().Where(p => p.ShopId.ExIn(ids)).ToList();
        }

        /// <summary>
        /// 获取单个店铺账户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopAccountInfo GetShopAccount(long id)
        {
            var model = DbFactory.Default.Get<ShopAccountInfo>().Where(p => p.ShopId == id).FirstOrDefault();

            #region 当新状态没值时，初始一个(因为之前代码有好些地方要用到ShopAccount但它没初始值，这里初始下)
            if (model == null)
            {
                var shop = GetShop(id);
                if (shop == null)
                    return null;
                model = new ShopAccountInfo();
                model.ShopId = id;
                model.ShopName = shop.ShopName;
                model.Balance = 0;
                model.PendingSettlement = 0;
                model.Settled = 0;
                model.ReMark = string.Empty;

                DbFactory.Default.Add(model);
            }
            #endregion
            return model;
        }

        public QueryPageModel<FavoriteShopInfo> GetUserConcernShops(long userId, int pageNo, int pageSize)
        {
            var favorite = DbFactory.Default.Get<FavoriteShopInfo>()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Id).ToPagedList(pageNo, pageSize);
            QueryPageModel<FavoriteShopInfo> pageModel = new QueryPageModel<FavoriteShopInfo>()
            {
                Models = favorite,
                Total = favorite.TotalRecordCount
            };
            return pageModel;
        }

        public void CancelConcernShops(List<long> ids, long userId)
        {
            DbFactory.Default.Del<FavoriteShopInfo>(a => a.UserId == userId && a.Id.ExIn(ids));
        }

        public void CancelConcernShops(long shopId, long userId)
        {
            DbFactory.Default.Del<FavoriteShopInfo>(a => a.UserId == userId && a.ShopId == shopId);
        }

        public ShopInfo CreateEmptyShop()
        {
            var emptyShop = new ShopInfo()
            {
                ShopName = "",
                GradeId = 0,
                IsSelf = false,
                ShopStatus = ShopInfo.ShopAuditStatus.Unusable,
                CreateDate = DateTime.Now,
                CompanyRegionId = 0,
                CompanyEmployeeCount = CompanyEmployeeCount.LessThanFive,
                CompanyRegisteredCapital = 0,
                BusinessLicenceNumberPhoto = "",
                BusinessLicenceRegionId = 0,
                BankRegionId = 0,
                FreeFreight = 0,
                Freight = 0,
                Stage = ShopInfo.ShopStage.Agreement
            };
            DbFactory.Default.Add(emptyShop);
            return emptyShop;
        }


        public void AddFavoriteShop(long memberId, long shopId)
        {
            if (DbFactory.Default.Get<FavoriteShopInfo>().Where(item => item.UserId == memberId && item.ShopId == shopId).Exist())
                throw new MallException("您已经关注过该店铺");
            DbFactory.Default.Add(new FavoriteShopInfo
            {
                ShopId = shopId,
                UserId = memberId,
                Date = DateTime.Now,
                Tags = "",
            });
        }

        public bool IsFavoriteShop(long memberId, long shopId)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>()
                .Where(item => item.UserId == memberId && item.ShopId == shopId).Exist();
        }

        public List<FavoriteShopInfo> GetFavoriteShopInfos(long memberId)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>()
                .Where(item => item.UserId == memberId).ToList();
        }


        public void SaveBusinessCategory(long id, decimal commisRate)
        {
            if (commisRate > 100)
                throw new InvalidPropertyException("分佣比例不能大于100");
            else if (commisRate < 0)
                throw new InvalidPropertyException("分佣比例不能小于0");

            var result = DbFactory.Default.Set<BusinessCategoryInfo>()
                .Set(n => n.CommisRate, commisRate)
                .Where(p => p.Id == id)
                .Succeed();
        }

        public void UpdateLogo(long shopId, string img)
        {
            var result = DbFactory.Default.Set<ShopInfo>().Set(n => n.Logo, img).Where(p => p.Id == shopId).Succeed();
            if (result)
                ClearShopCache(shopId);
        }
        public bool UpdateOpenTopImageAd(long shopId, bool isOpenTopImageAd)
        {
            var result = DbFactory.Default.Set<ShopInfo>().Set(n => n.IsOpenTopImageAd, isOpenTopImageAd).Where(p => p.Id == shopId).Succeed();
            if (result)
                ClearShopCache(shopId);
            return result;
        }

        /// <summary>
        /// 判断店铺名称是否存在
        /// </summary>
        /// <param name="shopName"></param>
        /// <returns></returns>
        public bool ExistShop(string shopName, long shopId = 0)
        {
            return DbFactory.Default.Get<ShopInfo>()
                .Where(p => p.ShopName == shopName && p.Id != shopId)
                .Exist();
        }


        public List<StatisticOrderCommentInfo> GetShopStatisticOrderComments(long shopId)
        {
            //TODO:FG 此查询实现 待优化
            return DbFactory.Default.Get<StatisticOrderCommentInfo>().Where(c => c.ShopId == shopId).ToList();
        }


        public List<StatisticOrderCommentInfo> GetStatisticOrderComments(List<long> shops) {
            return DbFactory.Default.Get<StatisticOrderCommentInfo>(p => p.ShopId.ExIn(shops)).ToList();
        }

        public ShopInfo.ShopVistis GetShopVistiInfo(DateTime startDate, DateTime endDate, long shopId)
        {
            var ordercount = DbFactory.Default.Get<OrderInfo>()
                .Where(c => c.OrderDate >= startDate && c.OrderDate <= endDate && c.ShopId == shopId)
                .Select(n => n.ExCount(false));
            var shopVistis = DbFactory.Default.Get<ShopVistiInfo>().Where(p => p.ShopId == shopId && p.Date >= startDate && p.Date <= endDate)
                .GroupBy(p => p.ShopId).Select(g => new
                {
                    SaleAmounts = g.SaleAmounts.ExSum(),
                    SaleCounts = g.SaleCounts.ExSum(),
                    VistiCounts = g.VistiCounts.ExSum(),
                    OrderCounts = ordercount.ExResolve<decimal>(),
                    OrderPayCount = g.OrderPayCount.ExSum(),
                }).FirstOrDefault<ShopInfo.ShopVistis>();
            //if (shopVistis != null)
            //{
            //    shopVistis.OrderCounts = DbFactory.Default.Get<OrderInfo>().Where(c => c.OrderDate >= startDate && c.OrderDate <= endDate && c.ShopId == shopId).Count();
            //}
            return shopVistis;
        }


        public bool ExistCompanyName(string companyName, long shopId = 0)
        {
            return DbFactory.Default.Get<ShopInfo>().Where(s => s.CompanyName == companyName && s.Id != shopId).Exist();
        }

        public bool ExistBusinessLicenceNumber(string BusinessLicenceNumber, long shopId = 0)
        {
            if (!string.IsNullOrWhiteSpace(BusinessLicenceNumber))
                return DbFactory.Default.Get<ShopInfo>().Where(s => s.BusinessLicenceNumber.Equals(BusinessLicenceNumber) && s.Id != shopId).Exist();
            else
                return false;
        }

        public void AddShopRenewRecord(ShopRenewRecordInfo record, bool isShopAccount = false)
        {
            string AccountNo = record.ShopId.ToString() + record.Id + new Random().Next(10000);
            if (!string.IsNullOrEmpty(record.TradeNo))
            {
                AccountNo = record.TradeNo;

            }
            DbFactory.Default.InTransaction(() =>
            {
                if (isShopAccount)
                {
                    var ShopAccount = DbFactory.Default.Get<ShopAccountInfo>().Where(a => a.ShopId == record.ShopId).FirstOrDefault();
                    ShopAccountItemInfo info = new ShopAccountItemInfo();
                    info.IsIncome = false;
                    info.ShopId = ShopAccount.ShopId;
                    info.DetailId = record.Id.ToString();
                    info.ShopName = ShopAccount.ShopName;
                    info.AccountNo = AccountNo;
                    info.ReMark = record.OperateContent;
                    info.TradeType = record.OperateType == ShopRenewRecordInfo.EnumOperateType.ReNew ? ShopAccountType.Renew : ShopAccountType.Upgrade;
                    info.CreateTime = DateTime.Now;
                    info.Amount = record.Amount;
                    info.AccoutID = ShopAccount.Id;
                    ShopAccount.Balance -= info.Amount;//总余额减钱
                    info.Balance = ShopAccount.Balance;//变动后当前剩余金额

                    DbFactory.Default.Add(info);
                    DbFactory.Default.Update(ShopAccount);
                }
                DbFactory.Default.Add(record);
                var PlatAccount = DbFactory.Default.Get<PlatAccountInfo>().FirstOrDefault();
                PlatAccountItemInfo pinfo = new PlatAccountItemInfo();
                pinfo.IsIncome = true;
                pinfo.DetailId = record.Id.ToString();
                pinfo.AccountNo = AccountNo;
                pinfo.ReMark = "入驻缴费:" + record.OperateContent;
                pinfo.TradeType = CommonModel.PlatAccountType.SettledPayment;
                pinfo.CreateTime = DateTime.Now;
                pinfo.Amount = record.Amount;
                pinfo.AccoutID = PlatAccount.Id;
                PlatAccount.Balance += pinfo.Amount;//总余额加钱
                pinfo.Balance = PlatAccount.Balance;

                DbFactory.Default.Add(pinfo);
                DbFactory.Default.Update(PlatAccount);
            });
        }

        public void ShopReNew(long shopid, int year)
        {
            var shop = DbFactory.Default.Get<ShopInfo>().Where(r => r.Id == shopid).FirstOrDefault();
            if (shop == null)
                throw new MessageException(ExceptionMessages.NoFound, "门店");

            var enddate = shop.EndDate;
            if (enddate < DateTime.Now)
                enddate = DateTime.Now;
            shop.EndDate = enddate.AddYears(year);
            shop.ShopStatus = ShopInfo.ShopAuditStatus.Open;
            DbFactory.Default.Update(shop);
        }

        public void ShopUpGrade(long shopid, long gradeid)
        {
            var result = DbFactory.Default.Set<ShopInfo>()
                .Where(p => p.Id == shopid)
                .Set(p => p.GradeId, gradeid)
                .Set(p => p.ShopStatus, ShopInfo.ShopAuditStatus.Open)
                .Succeed();
            if (!result)
                throw new MessageException(ExceptionMessages.NoFound, "门店");
        }

        public QueryPageModel<ShopRenewRecordInfo> GetShopRenewRecords(ShopQuery query)
        {
            var data = DbFactory.Default.Get<ShopRenewRecordInfo>()
                .Where(r => r.ShopId == query.BrandId)
                .OrderByDescending(o => o.Id)
                .ToPagedList(query.PageNo, query.PageSize);

            return new QueryPageModel<ShopRenewRecordInfo>
            {
                Models = data,
                Total = data.TotalRecordCount
            };
        }


        public void ApplyShopBusinessCate(long shopId, List<long> categoryIds)
        {
            var shop = DbFactory.Default.Get<ShopInfo>().Where(p => p.Id == shopId).FirstOrDefault();
            var info = new BusinessCategoryApplyInfo
            {
                ApplyDate = DateTime.Now,
                AuditedStatus = BusinessCategoryApplyInfo.BusinessCateApplyStatus.UnAudited,
                ShopId = shop.Id,
                ShopName = shop.ShopName
            };
            DbFactory.Default.Add(info);

            var excite = DbFactory.Default.Get<BusinessCategoryInfo>()
                .Where(a => a.ShopId == shopId && a.CategoryId.ExIn(categoryIds))
                .Select(p => p.CategoryId)
                .ToList<long>();
            categoryIds = categoryIds.Where(p => !excite.Contains(p)).ToList();

            var categories = DbFactory.Default.Get<CategoryInfo>().Where(item => item.Id.ExIn(categoryIds)).ToList();
            var list = categories.Select(item => new BusinessCategoryApplyDetailInfo
            {
                ApplyId = info.Id,
                CategoryId = item.Id,
                CommisRate = item.CommisRate,
            });
            DbFactory.Default.AddRange(list);
        }
        public List<BusinessCategoryApplyDetailInfo> GetBusinessCategoriesApplyDetails(long applyid) {
            return DbFactory.Default.Get<BusinessCategoryApplyDetailInfo>(p => p.ApplyId == applyid).ToList();
        }
        public void AuditShopBusinessCate(long applyId, BusinessCategoryApplyInfo.BusinessCateApplyStatus status)
        {
            var apply = DbFactory.Default.Get<BusinessCategoryApplyInfo>().Where(a => a.Id == applyId).FirstOrDefault();
            apply.AuditedStatus = status;
            if (status == BusinessCategoryApplyInfo.BusinessCateApplyStatus.Audited) //审核通过
            {
                var details = DbFactory.Default.Get<BusinessCategoryApplyDetailInfo>(p => p.ApplyId == apply.Id).ToList();
                foreach (var t in details)
                {
                    var cate = DbFactory.Default.Get<BusinessCategoryInfo>().Where(a => a.ShopId == apply.ShopId && a.CategoryId == t.CategoryId).Exist();
                    if (!cate)
                    {
                        var businessCategoies = new BusinessCategoryInfo()
                        {
                            CategoryId = t.CategoryId,
                            ShopId = apply.ShopId,
                            CommisRate = t.CommisRate
                        };
                        DbFactory.Default.Add(businessCategoies);
                    }
                }
            }
            apply.AuditedDate = DateTime.Now;
            DbFactory.Default.Update(apply);
        }

        public QueryPageModel<BusinessCategoryApplyInfo> GetBusinessCateApplyList(BussinessCateApplyQuery query)
        {
            var apply = DbFactory.Default.Get<BusinessCategoryApplyInfo>();

            if (!string.IsNullOrEmpty(query.ShopName))
                apply.Where(a => a.ShopName.Contains(query.ShopName));

            if (query.shopId.HasValue && query.shopId.Value != 0)
                apply.Where(a => a.ShopId == query.shopId.Value);

            if (query.Status.HasValue)
                apply.Where(a => a.AuditedStatus == query.Status.Value);

            var result = apply.OrderByDescending(a => a.ApplyDate).ToPagedList(query.PageNo, query.PageSize);
            return new QueryPageModel<BusinessCategoryApplyInfo>()
            {
                Models = result,
                Total = result.TotalRecordCount
            };
        }

        public BusinessCategoryApplyInfo GetBusinessCategoriesApplyInfo(long applyId)
        {
            return DbFactory.Default.Get<BusinessCategoryApplyInfo>().Where(p => p.Id == applyId).FirstOrDefault();
        }

        public List<CategoryRateModel> GetThirdBusinessCategory(long id, long shopId)
        {
            var list = new List<CategoryRateModel>();
            var categoriesAll = DbFactory.Default.Get<CategoryInfo>().ToList();
            var category = DbFactory.Default.Get<CategoryInfo>().Where(p => p.Id == id).FirstOrDefault();
            if (category.Depth == 1 && category.ParentCategoryId == 0)
            {
                var second = categoriesAll.Where(item => item.ParentCategoryId == category.Id).Select(item => item.Id);
                var three = categoriesAll.Where(item => second.Contains(item.ParentCategoryId));
                list.AddRange(three.Select(item => new CategoryRateModel
                {
                    Id = item.Id,
                    Rate = item.CommisRate
                }));
            }
            else if (category.Depth == 2)
            {
                var three = categoriesAll.Where(a => a.ParentCategoryId == category.Id);
                list.AddRange(three.Select(item => new CategoryRateModel
                {
                    Id = item.Id,
                    Rate = item.CommisRate
                }));
            }
            else
            {
                list.Add(new CategoryRateModel { Id = category.Id, Rate = category.CommisRate });
            }

            //移除已申请分类
            var mycate = DbFactory.Default.Get<BusinessCategoryInfo>().Where(a => a.ShopId == shopId).Select(item => item.CategoryId).ToList<long>();
            list = list.Where(a => !mycate.Contains(a.Id)).ToList();

            //补充分类完整路径
            var map = GetCategoryNamePath(list.Select(p => p.Id).ToList());
            list.ForEach(item =>
            {
                if (map.ContainsKey(item.Id))
                    item.Path = map[item.Id];
            });
            return list;
        }

        /// <summary>
        /// 冻结/解冻店铺
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state">true冻结 false解冻</param>
        public void FreezeShop(long id, bool state)
        {
            //var shop = Context.ShopInfo.FirstOrDefault(d => d.Id == id);
            var shop = DbFactory.Default.Get<ShopInfo>().Where(d => d.Id == id).FirstOrDefault();
            if (shop == null)
            {
                throw new MallException("错误的店铺编号");
            }
            if (shop.IsSelf == false)
            {
                if (state)
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Freeze;
                    //var shopbranchs = Context.ShopBranchInfo.Where(n => n.ShopId == id).ToList();
                    var shopbranchs = DbFactory.Default.Get<ShopBranchInfo>().Where(n => n.ShopId == id).ToList();
                    foreach (var shopbranch in shopbranchs)
                    {
                        //shopbranch.Status = ShopBranchStatus.Freeze;
                        DbFactory.Default.Set<ShopBranchInfo>().Set(n => n.Status, ShopBranchStatus.Freeze).Where(p => p.Id == shopbranch.Id).Succeed();
                    }
                    //关闭微店
                    DbFactory.Default.Set<VShopInfo>().Set(d => d.IsOpen, false).Set(a => a.State, VShopInfo.VShopStates.Close).Where(p => p.ShopId == shop.Id).Succeed();
                    //下架所有分销商品
                    DbFactory.Default.Set<DistributionProductInfo>()
                        .Set(d => d.ProductStatus, 1)
                        .Where(p => p.ShopId == shop.Id)
                        .Succeed();
                }
                else
                {
                    shop.ShopStatus = ShopInfo.ShopAuditStatus.Open;
                    DbFactory.Default.Set<VShopInfo>().Set(d => d.State, VShopInfo.VShopStates.Normal).Succeed();//开启时微店只审核通过，不开启
                }

                SaleOffAllProduct(shop.Id);
                CloseAllMarketingAction(shop.Id);
                //Context.SaveChanges();
                DbFactory.Default.Update(shop);
                ClearShopCache(shop.Id);
            }
        }
        /// <summary>
        /// 关闭所有营销活动
        /// </summary>
        public void CloseAllMarketingAction(long id)
        {
            //Context.Database.ExecuteSqlCommand("update Mall_FlashSale set Status=4,EndDate='" + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss") + "' where ShopId=" + id.ToString() + "");
            DbFactory.Default.Set<FlashSaleInfo>().Set(n => n.Status, 4).Set(n => n.EndDate, DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss")).Where(p => p.ShopId == id).Succeed();
        }
        /// <summary>
        /// 将所有在售的商品下架
        /// </summary>
        /// <param name="id"></param>
        public void SaleOffAllProduct(long id)
        {
            DbFactory.Default.Set<ProductInfo>().Set(n => n.SaleStatus, ProductInfo.ProductSaleStatus.InStock).Where(p => p.ShopId == id && p.SaleStatus == ProductInfo.ProductSaleStatus.OnSale).Succeed();
            DbFactory.Default.Set<ShopBranchSkuInfo>().Set(n => n.Status, ShopBranchSkuStatus.InStock).Where(p => p.ShopId == id && p.Status == ShopBranchSkuStatus.Normal).Succeed();
        }
        

        public int GetShopFavoritesCount(long shopId)
        {
            return DbFactory.Default.Get<FavoriteShopInfo>().Where(item => item.ShopId == shopId).Count();
        }


        public int GetShopProductCount(long shopId)
        {
            return DbFactory.Default.Get<ProductInfo>().Where(item => item.ShopId == shopId && item.SaleStatus == ProductInfo.ProductSaleStatus.OnSale && item.AuditStatus == ProductInfo.ProductAuditStatus.Audited && item.IsDeleted == false).Count();
        }

        /// <summary>
        /// 获取单条入驻缴费记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShopRenewRecordInfo GetShopRenewRecord(long id)
        {
            return DbFactory.Default.Get<ShopRenewRecordInfo>().Where(a => a.Id == id).FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// 获取商铺管理员会员ID
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        public long GetShopManagers(long ShopId)
        {
            return DbFactory.Default.Get<ManagerInfo>()
                .LeftJoin<MemberInfo>((mi, mbi) => mi.UserName == mbi.UserName)
                .Where(p => p.ShopId == ShopId && p.RoleId==0)
                .Select<MemberInfo>(p => p.Id)
                .FirstOrDefault<long>();
        }

        /// <summary>
        /// 获取自营店铺信息
        /// </summary>
        /// <returns></returns>
        public ShopInfo GetSelfShop()
        {
            return DbFactory.Default.Get<ShopInfo>().Where(p => p.IsSelf == true).FirstOrDefault();
        }

        /// <summary>
        /// 获取商铺免邮活动 邮费
        /// </summary>
        /// <param name="ShopId"></param>
        /// <returns></returns>
        public decimal GetShopFreeFreight(long id)
        {
            return DbFactory.Default.Get<ShopInfo>().Where(a => a.Id == id).Select(a => a.FreeFreight).FirstOrDefault<decimal>();
        }

        /// <summary>
        /// 根据网店管家uCode获取对应店铺
        /// </summary>
        /// <param name="uCode"></param>
        /// <returns></returns>
        public ShopWdgjSettingInfo GetshopInfoByCode(string uCode)
        {
            return DbFactory.Default.Get<ShopWdgjSettingInfo>().Where(a => a.uCode == uCode).FirstOrDefault();
        }

        public ShopWdgjSettingInfo GetshopWdgjInfoById(long shopId)
        {
            return DbFactory.Default.Get<ShopWdgjSettingInfo>().Where(a => a.ShopId == shopId).FirstOrDefault();
        }
        /// <summary>
        /// 更新网店管家参数
        /// </summary>
        /// <param name="wdgj"></param>
        public void UpdateShopWdgj(ShopWdgjSettingInfo wdgj)
        {
            if (DbFactory.Default.Get<ShopWdgjSettingInfo>().Where(d => d.uCode == wdgj.uCode && d.Id != wdgj.Id).Exist())
                throw new MessageException(ExceptionMessages.WDGJAlreadyExisted);

            DbFactory.Default.Set<ShopWdgjSettingInfo>()
                .Set(n => n.uCode, wdgj.uCode)
                .Set(n => n.uSign, wdgj.uSign)
                .Where(p => p.Id == wdgj.Id)
                .Succeed();
        }
        /// <summary>
        /// 增加网店管家参数
        /// </summary>
        /// <param name="wdgj"></param>
        public void AddShopWdgj(ShopWdgjSettingInfo wdgj)
        {
            if (DbFactory.Default.Get<ShopWdgjSettingInfo>().Where(d => d.uCode == wdgj.uCode).Exist())
                throw new MessageException(ExceptionMessages.WDGJAlreadyExisted);
            DbFactory.Default.Add(wdgj);
        }

        public void SetAutoPrint(long id, bool enable)
        {
            DbFactory.Default.Set<ShopInfo>()
               .Where(p => p.Id == id)
               .Set(p => p.IsAutoPrint, enable)
               .Succeed();
        }
        public void SetPrintCount(long id, int count)
        {
            DbFactory.Default.Set<ShopInfo>()
               .Where(p => p.Id == id)
               .Set(p => p.PrintCount, count)
               .Succeed();
        }

        public bool HasProvideInvoice(List<long> shops)
        {
            return DbFactory.Default.Get<ShopInvoiceConfigInfo>().Where(p => p.ShopId.ExIn(shops) && p.IsInvoice == true).Exist();
        }

        /// <summary>
        /// 设置申请商家步骤
        /// </summary>
        /// <param name="shopStage">第几步</param>
        /// <param name="id">店铺Id</param>
        public void SetShopStage(ShopInfo.ShopStage shopStage, long id)
        {
            DbFactory.Default.Set<ShopInfo>().Where(p => p.Id == id).Set(p => p.Stage, shopStage).Succeed();
        }


        #region TDO:ZYF ShopInvoiceConfig

        /// <summary>
        /// 获取商家发票管理配置
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopInvoiceConfigInfo GetShopInvoiceConfig(long shopId)
        {
            return DbFactory.Default.Get<ShopInvoiceConfigInfo>().Where(p => p.ShopId == shopId).FirstOrDefault();
        }

        /// <summary>
        /// 获取用户默认的发票信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public InvoiceTitleInfo GetInvoiceTitleInfo(long userId,InvoiceType typeId)
        {
            return DbFactory.Default.Get<InvoiceTitleInfo>().Where(i => i.UserId == userId && i.InvoiceType == typeId && i.IsDefault == 1).FirstOrDefault();
        }

        #endregion

    }
}
