using Mall.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mall.DTO.QueryModel;
using static Mall.Entities.ProductInfo;
using Mall.CommonModel;
using Mall.Entities;
using AutoMapper;
using Mall.DTO;
using Mall.Core;
using Mall.SmallProgAPI.Model;
using Mall.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Mall.SmallProgAPI
{
    public class FightGroupController : BaseApiController
    {
        /// <summary>
        /// 获取拼团活动列表
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetActiveList")]
        public  object GetActiveList(int pageSize = 5, int pageNo = 1)
        {
            FightGroupActiveQuery query = new FightGroupActiveQuery();
            query.SaleStatus = ProductSaleStatus.OnSale;//已销售状态商品
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            query.ActiveStatusList = new List<FightGroupActiveStatus> {
                FightGroupActiveStatus.Ongoing,
                FightGroupActiveStatus.WillStart
            };
            var data = FightGroupApplication.GetActives(query);
            var datalist = data.Models.ToList();
            foreach (DTO.FightGroupActiveListModel item in datalist)
            {
                if (!string.IsNullOrWhiteSpace(item.IconUrl))
                    item.IconUrl = Core.MallIO.GetRomoteImagePath(item.IconUrl);
            }
            return Json(new { rows = datalist, total = data.Total });
        }

        /// <summary>
        /// 拼团活动商品详情
        /// </summary>
        /// <param name="id">拼团活动ID</param>
        /// /// <param name="grouId">团活动ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetActiveDetail")]
        public object GetActiveDetail(long id, long grouId = 0, bool isFirst = true, string ids = "")
        {
            var userList = new List<FightGroupOrderInfo>();
            var data = FightGroupApplication.GetActive(id, true, true);
            FightGroupActiveModel result = data;
            //先初始化拼团商品主图
            result.InitProductImages();
            var imgpath = data.ProductImgPath;
            if (result != null)
            {
                result.IsEnd = true;
                if (data.EndTime.Date >= DateTime.Now.Date)
                {
                    result.IsEnd = false;
                }
                //商品图片地址修正
                result.ProductDefaultImage = MallIO.GetRomoteProductSizeImage(imgpath, 1, (int)ImageSize.Size_350);
                result.ProductImgPath = MallIO.GetRomoteProductSizeImage(imgpath, 1);
            }
            if (result.ProductImages != null)
            {//将主图相对路径处理为绝对路径
                result.ProductImages = result.ProductImages.Select(e => MallIO.GetRomoteImagePath(e)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(result.IconUrl))
            {
                result.IconUrl = Mall.Core.MallIO.GetRomoteImagePath(result.IconUrl);
            }
            bool IsUserEnter = false;
            long currentUser = 0;
            if (CurrentUser != null)
                currentUser = CurrentUser.Id;
            if (grouId > 0)//获取已参团的用户
            {
                userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers(id, grouId);
                foreach (var item in userList)
                {
                    item.Photo = !string.IsNullOrWhiteSpace(item.Photo) ? Core.MallIO.GetRomoteImagePath(item.Photo) : "";
                    item.HeadUserIcon = !string.IsNullOrWhiteSpace(item.HeadUserIcon) ? Core.MallIO.GetRomoteImagePath(item.HeadUserIcon) : "";
                    if (currentUser.Equals(item.OrderUserId))
                        IsUserEnter = true;
                }
            }
            #region 商品规格
            var product = ProductManagerApplication.GetProduct((long)result.ProductId);

            ProductShowSkuInfoModel model = new ProductShowSkuInfoModel();
            model.MinSalePrice = data.MiniSalePrice;
            model.ProductImagePath = string.IsNullOrWhiteSpace(imgpath) ? "" : MallIO.GetRomoteProductSizeImage(imgpath, 1, (int)Mall.CommonModel.ImageSize.Size_350);

            List<SKUDataModel> skudata = data.ActiveItems.Where(d => d.ActiveStock > 0).Select(d => new SKUDataModel
            {
                SkuId = d.SkuId,
                Color = d.Color,
                Size = d.Size,
                Version = d.Version,
                Stock = (int)d.ActiveStock,
                CostPrice = d.ProductCostPrice,
                SalePrice = d.ProductPrice,
                Price = d.ActivePrice,
            }).ToList();

            Entities.TypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (product != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(product.ColorAlias) ? product.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(product.SizeAlias) ? product.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(product.VersionAlias) ? product.VersionAlias : versionAlias;
            }
            model.ColorAlias = colorAlias;
            model.SizeAlias = sizeAlias;
            model.VersionAlias = versionAlias;

            if (result.ActiveItems != null && result.ActiveItems.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                var skus = ProductManagerApplication.GetSKUs((long)result.ProductId);
                foreach (var sku in result.ActiveItems)
                {
                    var specs = sku.SkuId.Split('_');
                    if (specs.Count() > 0 && !string.IsNullOrEmpty(sku.Color))
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = result.ActiveItems.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.ActiveStock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? " " : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = string.IsNullOrWhiteSpace(sku.ShowPic) ? "" : Core.MallIO.GetRomoteImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1 && !string.IsNullOrEmpty(sku.Size))
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = result.ActiveItems.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.ActiveStock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2 && !string.IsNullOrEmpty(sku.Version))
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = result.ActiveItems.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.ActiveStock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择规格",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }
            }
            #endregion

            var cashDepositModel = CashDepositsApplication.GetCashDepositsObligation((long)result.ProductId);//提供服务（消费者保障、七天无理由、及时发货）

            var GroupsData = new List<FightGroupsListModel>();
            List<FightGroupBuildStatus> stlist = new List<FightGroupBuildStatus>();
            stlist.Add(FightGroupBuildStatus.Ongoing);
            GroupsData = FightGroupApplication.GetGroups(id, stlist, null, null, 1, 10).Models.ToList();
            foreach (var item in GroupsData)
            {
                TimeSpan mid = item.AddGroupTime.AddHours((double)item.LimitedHour) - DateTime.Now;
                item.Seconds = (int)mid.TotalSeconds;
                item.EndHourOrMinute = item.ShowHourOrMinute(item.GetEndHour);
                item.HeadUserIcon = !string.IsNullOrWhiteSpace(item.HeadUserIcon) ? Core.MallIO.GetRomoteImagePath(item.HeadUserIcon) : "";
            }

            #region 商品评论
            ProductCommentShowModel modelSay = new ProductCommentShowModel();
            modelSay.ProductId = (long)result.ProductId;
            var productSay = ProductManagerApplication.GetProduct((long)result.ProductId);
            modelSay.CommentList = new List<ProductDetailCommentModel>();
            modelSay.IsShowColumnTitle = true;
            modelSay.IsShowCommentList = true;

            if (productSay == null)
            {
                //跳转到404页面
                throw new Core.MallException("商品不存在");
            }
            var comments = CommentApplication.GetCommentsByProduct(product.Id);
            modelSay.CommentCount = comments.Count;
            if (comments.Count > 0)
            {
                var comment = comments.OrderByDescending(a => a.ReviewDate).FirstOrDefault();
                var orderItem = OrderApplication.GetOrderItem(comment.SubOrderId);
                var order = OrderApplication.GetOrder(orderItem.OrderId);
                modelSay.CommentList = comments.OrderByDescending(a => a.ReviewDate)
                    .Take(1)
                    .Select(c => {
                        var images = CommentApplication.GetProductCommentImagesByCommentIds(new List<long> { c.Id });
                        return new ProductDetailCommentModel
                        {
                            Sku = ServiceProvider.Instance<IProductService>.Create.GetSkuString(orderItem.SkuId),
                            UserName = c.UserName,
                            ReviewContent = c.ReviewContent,
                            AppendContent = c.AppendContent,
                            AppendDate = c.AppendDate,
                            ReplyAppendContent = c.ReplyAppendContent,
                            ReplyAppendDate = c.ReplyAppendDate,
                            FinshDate = order.FinishDate,
                            Images = images.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                            AppendImages = images.Where(a => a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                            ReviewDate = c.ReviewDate,
                            ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                            ReplyDate = c.ReplyDate,
                            ReviewMark = c.ReviewMark,
                            BuyDate = order.OrderDate

                        };
                    }).ToList();
                foreach (var citem in modelSay.CommentList)
                {
                    if (citem.Images.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.Images.Count; _imgn++)
                        {
                            citem.Images[_imgn] = Mall.Core.MallIO.GetRomoteImagePath(citem.Images[_imgn]);
                        }
                    }
                    if (citem.AppendImages.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.AppendImages.Count; _imgn++)
                        {
                            citem.AppendImages[_imgn] = Mall.Core.MallIO.GetRomoteImagePath(citem.AppendImages[_imgn]);
                        }
                    }
                }
            }
            #endregion

            #region 店铺信息
            VShopShowShopScoreModel modelShopScore = new VShopShowShopScoreModel();
            modelShopScore.ShopId = result.ShopId;
            var shop = ServiceProvider.Instance<IShopService>.Create.GetShop(result.ShopId);
            if (shop == null)
            {
                throw new MallException("错误的店铺信息");
            }

            modelShopScore.ShopName = shop.ShopName;

            #region 获取店铺的评价统计
            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(result.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == Entities.StatisticOrderCommentInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;

            modelShopScore.SellerServiceAttitude = defaultValue;
            modelShopScore.SellerServiceAttitudePeer = defaultValue;
            modelShopScore.SellerServiceAttitudeMax = defaultValue;
            modelShopScore.SellerServiceAttitudeMin = defaultValue;

            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null && !shop.IsSelf)
            {
                modelShopScore.ProductAndDescription = productAndDescription.CommentValue;
                modelShopScore.ProductAndDescriptionPeer = productAndDescriptionPeer.CommentValue;
                modelShopScore.ProductAndDescriptionMin = productAndDescriptionMin.CommentValue;
                modelShopScore.ProductAndDescriptionMax = productAndDescriptionMax.CommentValue;
            }
            else
            {
                modelShopScore.ProductAndDescription = defaultValue;
                modelShopScore.ProductAndDescriptionPeer = defaultValue;
                modelShopScore.ProductAndDescriptionMin = defaultValue;
                modelShopScore.ProductAndDescriptionMax = defaultValue;
            }

            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null && !shop.IsSelf)
            {
                modelShopScore.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
                modelShopScore.SellerServiceAttitudePeer = sellerServiceAttitudePeer.CommentValue;
                modelShopScore.SellerServiceAttitudeMax = sellerServiceAttitudeMax.CommentValue;
                modelShopScore.SellerServiceAttitudeMin = sellerServiceAttitudeMin.CommentValue;
            }
            else
            {
                modelShopScore.SellerServiceAttitude = defaultValue;
                modelShopScore.SellerServiceAttitudePeer = defaultValue;
                modelShopScore.SellerServiceAttitudeMax = defaultValue;
                modelShopScore.SellerServiceAttitudeMin = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null && !shop.IsSelf)
            {
                modelShopScore.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
                modelShopScore.SellerDeliverySpeedPeer = sellerDeliverySpeedPeer.CommentValue;
                modelShopScore.SellerDeliverySpeedMax = sellerDeliverySpeedMax != null ? sellerDeliverySpeedMax.CommentValue : 0;
                modelShopScore.sellerDeliverySpeedMin = sellerDeliverySpeedMin != null ? sellerDeliverySpeedMin.CommentValue : 0;
            }
            else
            {
                modelShopScore.SellerDeliverySpeed = defaultValue;
                modelShopScore.SellerDeliverySpeedPeer = defaultValue;
                modelShopScore.SellerDeliverySpeedMax = defaultValue;
                modelShopScore.sellerDeliverySpeedMin = defaultValue;
            }
            #endregion

            modelShopScore.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(result.ShopId);
            modelShopScore.IsFavoriteShop = false;
            modelShopScore.FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopFavoritesCount(result.ShopId);
            if (CurrentUser != null)
            {
                modelShopScore.IsFavoriteShop = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Any(d => d.ShopId == result.ShopId);
            }

            long vShopId;
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
            }
            else
            {
                vShopId = vshopinfo.Id;
            }
            modelShopScore.VShopId = vShopId;
            modelShopScore.VShopLog = ServiceProvider.Instance<IVShopService>.Create.GetVShopLog(vShopId);

            if (!string.IsNullOrWhiteSpace(modelShopScore.VShopLog))
            {
                modelShopScore.VShopLog = Mall.Core.MallIO.GetRomoteImagePath(modelShopScore.VShopLog);
            }

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerServiceAndMQ(shop.Id);
            #endregion
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            var template = freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            string productAddress = string.Empty;
            if (template != null)
            {
                var fullName = ServiceApplication.Create<IRegionService>().GetFullName(template.SourceAddress);
                if (fullName != null)
                {
                    var ass = fullName.Split(' ');
                    if (ass.Length >= 2)
                    {
                        productAddress = ass[0] + " " + ass[1];
                    }
                    else
                    {
                        productAddress = ass[0];
                    }
                }
            }

            var ProductAddress = productAddress;
            var FreightTemplate = template;
            #endregion

            #region 获取店铺优惠信息
            VShopShowPromotionModel modelVshop = new VShopShowPromotionModel();
            modelVshop.ShopId = result.ShopId;
            var shopInfo = ServiceProvider.Instance<IShopService>.Create.GetShop(result.ShopId);
            if (shopInfo == null)
            {
                throw new MallException("错误的店铺编号");
            }

            modelVshop.FreeFreight = shop.FreeFreight;


            var bonus = ServiceApplication.Create<IShopBonusService>().GetByShopId(result.ShopId);
            if (bonus != null)
            {
                modelVshop.BonusCount = bonus.Count;
                modelVshop.BonusGrantPrice = bonus.GrantPrice;
                modelVshop.BonusRandomAmountStart = bonus.RandomAmountStart;
                modelVshop.BonusRandomAmountEnd = bonus.RandomAmountEnd;
            }
            FullDiscountActive fullDiscount = null;
            //var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(id, shop.Id);
            #endregion
            //商品描述

            var description = ProductManagerApplication.GetProductDescription(result.ProductId);
            if (description == null)
                throw new MallException("错误的商品编号");

            string DescriptionPrefix = "", DescriptiondSuffix = "";
            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();
            if (description.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(description.DescriptionPrefixId, product.ShopId);
                DescriptionPrefix = desc == null ? "" : desc.MobileContent;
            }

            if (description.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(description.DescriptiondSuffixId, product.ShopId);
                DescriptiondSuffix = desc == null ? "" : desc.MobileContent;
            }
            var productDescription = DescriptionPrefix + description.ShowMobileDescription + DescriptiondSuffix;
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

          //  AutoMapper.Mapper.CreateMap<FightGroupActiveModel, FightGroupActiveResult>();
            var fightGroupData = result.Map<FightGroupActiveResult>();
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopItem = ShopApplication.GetShop(result.ShopId);
            fightGroupData.MiniSalePrice = shopItem.IsSelf ? fightGroupData.MiniSalePrice * discount : fightGroupData.MiniSalePrice;

            string loadShowPrice = string.Empty;//app拼团详细页加载时显示的区间价
            loadShowPrice = fightGroupData.MiniSalePrice.ToString("f2");

            if (fightGroupData != null && fightGroupData.ActiveItems.Count() > 0)
            {
                decimal min = fightGroupData.ActiveItems.Min(s => s.ActivePrice);
                decimal max = fightGroupData.ActiveItems.Max(s => s.ActivePrice);
                loadShowPrice = (min < max) ? (min.ToString("f2") + " - " + max.ToString("f2")) : min.ToString("f2");
            }

            var _result = new
            {
                success = true,
                FightGroupData = fightGroupData,
                ShowSkuInfo = new
                {
                    ColorAlias = model.ColorAlias,
                    SizeAlias = model.SizeAlias,
                    VersionAlias = model.VersionAlias,
                    MinSalePrice = model.MinSalePrice,
                    ProductImagePath = model.ProductImagePath,
                    Color = model.Color.OrderByDescending(p => p.SkuId),
                    Size = model.Size.OrderByDescending(p => p.SkuId),
                    Version = model.Version.OrderByDescending(p => p.SkuId)
                },
                ShowPromotion = modelVshop,
                fullDiscount = fullDiscount,
                ShowNewCanJoinGroup = GroupsData,
                ProductCommentShow = modelSay,
                ProductDescription = productDescription.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage") + "/"),
                ShopScore = modelShopScore,
                CashDepositsServer = cashDepositModel,
                ProductAddress = ProductAddress,
                //Free = FreightTemplate.IsFree == FreightTemplateType.Free ? "免运费" : "",
                userList = userList,
                IsUserEnter = IsUserEnter,
                SkuData = skudata,
                CustomerServices = customerServices,
                IsOpenLadder = product.IsOpenLadder,
                VideoPath = string.IsNullOrWhiteSpace(product.VideoPath) ? string.Empty : Mall.Core.MallIO.GetRomoteImagePath(product.VideoPath),
                LoadShowPrice = loadShowPrice,   //商品时区间价
                ProductSaleCountOnOff = (SiteSettingApplication.SiteSettings.ProductSaleCountOnOff == 1),//是否显示销量
                SaleCounts = data.ActiveItems.Sum(d => d.BuyCount),    //销量
                FreightStr = FreightTemplateApplication.GetFreightStr(product.Id, FreightTemplate, CurrentUser, product),//运费多少或免运费
                SendTime = (FreightTemplate != null && !string.IsNullOrEmpty(FreightTemplate.SendTime) ? (FreightTemplate.SendTime + "h内发货") : ""), //运费模板发货时间
            };
            return Json(_result);
        }

        /// <summary>
        /// 获取能参加的拼团活动
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetActiveDetailSec")]
        public object GetActiveDetailSec(string ids = "")
        {
            ////获取能参加的拼团活动
            List<FightGroupBuildStatus> status = new List<FightGroupBuildStatus>();
            status.Add(FightGroupBuildStatus.Ongoing);
            var GroupsData = new List<FightGroupsListModel>();

            string[] activeIds = ids.Split(',');
            long[] unActiveId = new long[activeIds.Length];
            for (int i = 0; i < activeIds.Length; i++)
            {
                unActiveId[i] = Int64.Parse(activeIds[i]);
            }
            GroupsData = FightGroupApplication.GetCanJoinGroupsSecond(unActiveId, status);

            foreach (var item in GroupsData)
            {
                TimeSpan mid = item.AddGroupTime.AddHours((double)item.LimitedHour) - DateTime.Now;
                item.Seconds = (int)mid.TotalSeconds;
            }
            var _result = new
            {
                ShowNewCanJoinGroup = GroupsData
            };
            return Json(_result);
        }

        /// <summary>
        /// 获取拼团团组详情
        /// </summary>
        /// <param name="activeId">拼团活动ID</param>
        /// <param name="groupId">拼团团组ID</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetGroupDetail")]
        public object GetGroupDetail(long activeId, long groupId)
        {
            var data = FightGroupApplication.GetGroup(activeId, groupId);
            if (data == null)
            {
                throw new MallException("错误的拼团信息");
            }
            return Json(data);
        }

        /// <summary>
        /// 根据用户ID获取拼团订单列表
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetFightGroupOrderByUser")]
        public object GetFightGroupOrderByUser(int pageSize = 5, int pageNo = 1)
        {
            CheckUserLogin();
            long UserId = CurrentUserId;
            var userList = new List<FightGroupOrderInfo>();
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            var data = FightGroupApplication.GetJoinGroups(UserId, seastatus, pageNo, pageSize);
            var datalist = data.Models.ToList();

            List<FightGroupGetFightGroupOrderByUserModel> resultlist = new List<FightGroupGetFightGroupOrderByUserModel>();
            foreach (var item in datalist)
            {
                FightGroupGetFightGroupOrderByUserModel _tmp = new FightGroupGetFightGroupOrderByUserModel();
                _tmp.Id = item.Id;
                _tmp.ActiveId = item.ActiveId;
                _tmp.ProductName = item.ProductName;
                _tmp.ProductImgPath = MallIO.GetRomoteImagePath(item.ProductImgPath);
                _tmp.ProductDefaultImage = MallIO.GetRomoteProductSizeImage(item.ProductImgPath, 1, (int)ImageSize.Size_350);
                _tmp.GroupEndTime = item.OverTime.HasValue ? item.OverTime.Value : item.GroupEndTime;
                _tmp.BuildStatus = item.BuildStatus;
                _tmp.NeedNumber = item.LimitedNumber - item.JoinedNumber;
                _tmp.UserIcons = new List<string>();
                foreach (var sitem in item.GroupOrders)
                {
                    _tmp.UserIcons.Add(MallIO.GetRomoteImagePath(sitem.Photo));
                    if (sitem.OrderUserId == UserId)
                    {
                        _tmp.OrderId = sitem.OrderId;
                        _tmp.GroupPrice = sitem.SalePrice;
                    }
                }
                resultlist.Add(_tmp);
            }
            return Json(new { rows = resultlist, total = data.Total });
        }

        /// <summary>
        /// 获取拼团订单详情
        /// </summary>
        /// <param name="Id">订单Id</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetFightGroupOrderDetail")]
        public object GetFightGroupOrderDetail(long id)
        {
            var userList = new List<FightGroupOrderInfo>();

            var orderDetail = FightGroupApplication.GetFightGroupOrderStatusByOrderId(id);
            //团组活动信息
            orderDetail.UserInfo = new List<UserInfo>();
            var data = FightGroupApplication.GetActive((long)orderDetail.ActiveId, false, true);
           // Mapper.CreateMap<FightGroupActiveInfo, FightGroupActiveModel>();
            //规格映射
            //Mapper.CreateMap<FightGroupActiveItemInfo, FightGroupActiveItemModel>();

            FightGroupsModel groupsdata = FightGroupApplication.GetGroup(orderDetail.ActiveId, orderDetail.GroupId);
            if (groupsdata == null)
            {
                throw new MallException("错误的拼团信息");
            }
            if (data != null)
            {
                //商品图片地址修正
                data.ProductDefaultImage = MallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);
            }
            orderDetail.AddGroupTime = groupsdata.AddGroupTime;
            orderDetail.GroupStatus = groupsdata.BuildStatus;
            orderDetail.ProductId = data.ProductId;
            orderDetail.ProductName = data.ProductName;
            orderDetail.IconUrl = MallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_350);//result.IconUrl;
            orderDetail.thumbs = MallIO.GetRomoteProductSizeImage(data.ProductImgPath, 1, (int)ImageSize.Size_100);
            //在使用之后，再修正为绝对路径
            data.ProductImgPath = MallIO.GetRomoteImagePath(data.ProductImgPath);

            orderDetail.MiniGroupPrice = data.MiniGroupPrice;
            TimeSpan mids = DateTime.Now - (DateTime)orderDetail.AddGroupTime;
            orderDetail.Seconds = (int)(data.LimitedHour * 3600) - (int)mids.TotalSeconds;
            orderDetail.LimitedHour = data.LimitedHour.GetValueOrDefault();
            orderDetail.LimitedNumber = data.LimitedNumber.GetValueOrDefault();
            orderDetail.JoinedNumber = groupsdata.JoinedNumber;
            orderDetail.OverTime = groupsdata.OverTime.HasValue ? groupsdata.OverTime : orderDetail.AddGroupTime.AddHours((double)orderDetail.LimitedHour);
            if (orderDetail.OverTime.HasValue)
            {
                orderDetail.OverTime = DateTime.Parse(orderDetail.OverTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            //拼装已参团成功的用户
            userList = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveUsers((long)orderDetail.ActiveId, (long)orderDetail.GroupId);
            foreach (var userItem in userList)
            {
                var userInfo = new UserInfo();
                userInfo.Photo = !string.IsNullOrWhiteSpace(userItem.Photo) ? Core.MallIO.GetRomoteImagePath(userItem.Photo) : "";
                userInfo.UserName = userItem.UserName;
                userInfo.JoinTime = userItem.JoinTime;
                orderDetail.UserInfo.Add(userInfo);
            }
            //获取团长信息
            var GroupsData = ServiceProvider.Instance<IFightGroupService>.Create.GetGroup(orderDetail.ActiveId, orderDetail.GroupId);
            if (GroupsData != null)
            {
                orderDetail.HeadUserName = GroupsData.HeadUserName;
                orderDetail.HeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.HeadUserIcon) ? Core.MallIO.GetRomoteImagePath(GroupsData.HeadUserIcon) : "";
                orderDetail.ShowHeadUserIcon = !string.IsNullOrWhiteSpace(GroupsData.ShowHeadUserIcon) ? Core.MallIO.GetRomoteImagePath(GroupsData.ShowHeadUserIcon) : "";
            }
            //商品评论数
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(orderDetail.ProductId);
            var comCount = CommentApplication.GetCommentCountByProduct(product.Id);
            //商品描述

            var ProductDescription = ServiceApplication.Create<IProductService>().GetProductDescription(orderDetail.ProductId);
            if (ProductDescription == null)
            {
                throw new Mall.Core.MallException("错误的商品编号");
            }

            string description = ProductDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.MallIO.GetRomoteImagePath("/Storage/") + "/");//商品描述

            return Json(new
            {
                OrderDetail = orderDetail,
                ComCount = comCount,
                ProductDescription = description
            });
        }
    }
}
