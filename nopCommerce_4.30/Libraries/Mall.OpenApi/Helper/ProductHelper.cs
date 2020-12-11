using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.Web.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using Mall.Application;
using Hishop.Open.Api;

namespace Mall.OpenApi
{
    /// <summary>
    /// 商品辅助工具
    /// </summary>
    public class ProductHelper : _BaseHelper
    {
        private IProductService _iProductService;
        private ITypeService _iTypeService;
        public ProductHelper()
        {
            _iProductService = Mall.ServiceProvider.Instance<IProductService>.Create;
            _iTypeService = Mall.ServiceProvider.Instance<ITypeService>.Create;
        }

        /// <summary>
        /// 获取指定商品的详情信息
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model GetProduct(int num_iid, string app_key)
        {
            InitShopInfo(app_key);

            long proid = num_iid;
            var prodata = _iProductService.GetProduct(proid);
            if (prodata.ShopId != shopId)
            {
                throw new MallApiException(OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            var result = ProductInfoMapChange(prodata);
            return result;
        }

        /// <summary>
        /// 获取当前商家的商品列表
        /// </summary>
        /// <param name="start_modified"></param>
        /// <param name="end_modified"></param>
        /// <param name="approve_status"></param>
        /// <param name="q"></param>
        /// <param name="order_by"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public QueryPageModel<product_list_model> GetSoldProducts(DateTime? start_modified, DateTime? end_modified, string approve_status, string q, string order_by, int page_no, int page_size, string app_key)
        {
            InitShopInfo(app_key);


            var resultdata = new List<product_list_model>();

            #region 构建查询条件
            var query = new ProductQuery()
            {
                PageSize = page_size,
                PageNo = page_no,
                KeyWords = q
            };
            query.ShopId = shopId;
            if (start_modified != null)
            {
                query.StartDate = start_modified;
            }
            if (end_modified != null)
            {
                query.EndDate = end_modified;
            }
            ProductStatus queryps = ProductStatus.In_Stock;
            if (!string.IsNullOrWhiteSpace(approve_status))
            {
                if (Enum.TryParse(approve_status, true, out queryps))
                {
                    switch (queryps)
                    {
                        case ProductStatus.In_Stock:
                            query.SaleStatus = Entities.ProductInfo.ProductSaleStatus.InStock;
                            break;
                        case ProductStatus.On_Sale:
                            query.SaleStatus = Entities.ProductInfo.ProductSaleStatus.OnSale;
                            query.AuditStatus = new[] { Entities.ProductInfo.ProductAuditStatus.Audited };
                            break;
                        case ProductStatus.Un_Sale:
                            query.AuditStatus = new[] { Entities.ProductInfo.ProductAuditStatus.WaitForAuditing, Entities.ProductInfo.ProductAuditStatus.InfractionSaleOff };
                            break;
                        default:
                            throw new MallApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
                    }
                }
                else
                {
                    throw new MallApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
                }
            }
            if (!string.IsNullOrWhiteSpace(order_by))
            {
                bool _orderasc = true;
                if (order_by.IndexOf(":desc") > 0)
                {
                    _orderasc = false;
                }
                order_by = order_by.Split(':')[0];
                query.OrderKey = 1;
                query.OrderType = _orderasc;
                switch (order_by)
                {
                    case "create_time":
                        query.OrderKey = 2;
                        break;
                    case "sold_quantity":
                        query.OrderKey = 3;
                        break;
                }
            }
            #endregion

            var proqlist = ProductManagerApplication.GetProducts(query);
            return new QueryPageModel<product_list_model>
            {
                Total = proqlist.Total,
                Models = ProductInfoListMapChange(proqlist.Models)
            };
        }

        /// <summary>
        /// 商品/SKU库存修改(提供按照全量或增量形式修改宝贝/SKU库存
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="sku_id"></param>
        /// <param name="quantity"></param>
        /// <param name="type"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model UpdateProductQuantity(int num_iid, string sku_id, int quantity, int type, string app_key)
        {
            long proid = num_iid;
            var product = _iProductService.GetProduct(proid);
            if (product == null)
            {
                throw new MallApiException(OpenApiErrorCode.Invalid_Arguments, "num_iid");
            }
            InitShopInfo(app_key);
            if (product.ShopId != shopId)
            {
                throw new MallApiException(OpenApiErrorCode.Invalid_App_Key, "app_key");
            }
            Entities.SKUInfo skudata;
            if (!string.IsNullOrWhiteSpace(sku_id))
            {
                skudata = _iProductService.GetSku(sku_id);
                long oldstock = skudata.Stock;
                long nowstock = GetStockQuantity(oldstock, quantity, type);
                Application.ProductManagerApplication.SetSkuStock(StockOptionType.Add, sku_id, nowstock - oldstock);
            }
            else
            {
                var skus = _iProductService.GetSKUs(product.Id);
                if (skus.Count > 0)
                {
                    foreach (var item in skus)
                    {
                        item.Stock = GetStockQuantity(item.Stock, quantity, type);
                    }
                    _iProductService.UpdateProduct(product);   //直接修改整个商品数据
                }
                else
                {
                    //不做操作
                    //prodata.Quantity = GetStockQuantity(prodata.Quantity.Value, quantity, type);
                }
            }
            //获取商品新数据
            product = _iProductService.GetProduct(proid);
            var result = ProductInfoMapChange(product);
            return result;
        }

        /// <summary>
        /// 修改商品销售状态 (上架， 下架， 入库)
        /// </summary>
        /// <param name="num_iid"></param>
        /// <param name="approve_status"></param>
        /// <param name="app_key"></param>
        /// <returns></returns>
        public product_item_model UpdateProductApproveStatus(int num_iid, string approve_status, string app_key)
        {
            if (string.IsNullOrWhiteSpace(approve_status))
            {
                throw new MallApiException(OpenApiErrorCode.Product_ApproveStatus_Faild, "approve_status");
            }
            InitShopInfo(app_key);
            ProductStatus dataps = ProductStatus.In_Stock;
            if (Enum.TryParse(approve_status, true, out dataps))
            {
                try
                {
                    switch (dataps)
                    {
                        case ProductStatus.In_Stock:

                            _iProductService.SaleOff(num_iid, shopId);
                            break;
                        case ProductStatus.On_Sale:
                            _iProductService.OnSale(num_iid, shopId);
                            break;
                        case ProductStatus.Un_Sale:
                            _iProductService.SaleOff(num_iid, shopId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new MallApiException(OpenApiErrorCode.Product_Status_is_Invalid, "approve_status");
                }
            }
            else
            {
                throw new MallApiException(OpenApiErrorCode.Product_Status_is_Invalid, "approve_status");
            }

            long proid = num_iid;
            var prodata = _iProductService.GetProduct(proid);
            var result = ProductInfoMapChange(prodata);
            return result;
        }

        #region 私有
        /// <summary>
        /// 商品信息转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private product_item_model ProductInfoMapChange(Entities.ProductInfo data)
        {
            var prodata = data;
            if (prodata == null)
            {
                throw new MallApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }
            product_item_model result = new product_item_model();

            #region 装配信息
            result.num_iid = (int)prodata.Id;
            result.outer_id = prodata.ProductCode;
            result.brand_id = (int)prodata.BrandId;
            result.brand_name = prodata.BrandName;
            result.cid = (int)prodata.CategoryId;
            result.type_id = (int)prodata.TypeId;

            var category = CategoryApplication.GetCategory(prodata.CategoryId);
            var desc = ProductManagerApplication.GetProductDescription(prodata.Id);
            if (category != null)
            {
                result.cat_name = category.Name;
                var type = TypeApplication.GetType(category.TypeId);
                if (type != null)
                    result.type_name = type.Name;
            }
            result.title = prodata.ProductName.Trim();
            result.list_time = prodata.AddedDate;
            result.modified = prodata.AddedDate;
            result.display_sequence = (int)prodata.DisplaySequence;
            result.sold_quantity = (int)prodata.SaleCounts;
            result.desc = desc.Description;
            result.wap_desc = desc.MobileDescription;
            result.pic_url.Add(System.IO.Path.Combine(OpenAPIHelper.HostUrl, prodata.GetImage(ImageSize.Size_350, 1)));
            ProductStatus ps = GetProductStatus(prodata);
            result.approve_status = ps.ToString();

            #region 商品属性填充
            var prodAttrs = ProductManagerApplication.GetProductAttributes(prodata.Id);
            var prodAttrids = prodAttrs.Select(d => d.AttributeId).ToList();
            var prodAttrValues = _iTypeService.GetAttributeValues(prodAttrids);
            result.props_name = "";
            if (prodAttrids.Count > 0)
            {
                List<string> propslst = new List<string>();
                List<string> propsvallst = new List<string>();
                foreach (var curattid in prodAttrids)
                {
                    var item = prodAttrs.FirstOrDefault(d => d.AttributeId == curattid);
                    propsvallst.Clear();
                    var attribute = _iTypeService.GetAttribute(item.AttributeId);
                    var values = prodAttrValues.Where(p => p.AttributeId == curattid);
                    foreach (var attrV in values)
                    {
                        if (prodAttrs.Any(p => p.ValueId == attrV.Id))
                        {
                            propsvallst.Add(attrV.Value);
                        }
                    }
                    propslst.Add(attribute.Name + "#cln#[" + string.Join(",", propsvallst.ToArray()) + "]");
                }
                result.props_name = string.Join("#scln#", propslst.ToArray());
            }
            #endregion

            #region  发货地区

            var prolocid = FreightTemplateApplication.GetFreightTemplate(prodata.FreightTemplateId).SourceAddress;
            result.location = "";

            var locpath = _iRegionService.GetFullName(prolocid, ",");
            result.location = "{'city':'#c#', 'state':'#p#'}";
            if (!string.IsNullOrWhiteSpace(locpath))
            {
                var _tmparr = locpath.Split(',');
                result.location = result.location.Replace("#p#", _tmparr[0]);
                if (_tmparr.Length > 1)
                {
                    result.location = result.location.Replace("#c#", _tmparr[1]);
                }
                else
                {
                    result.location = result.location.Replace("#c#", "");
                }
            }

            #endregion

            #region SKUS

            Entities.TypeInfo typeInfo = _iTypeService.GetType(data.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            if (prodata != null)
            {
                colorAlias = !string.IsNullOrWhiteSpace(prodata.ColorAlias) ? prodata.ColorAlias : colorAlias;
                sizeAlias = !string.IsNullOrWhiteSpace(prodata.SizeAlias) ? prodata.SizeAlias : sizeAlias;
                versionAlias = !string.IsNullOrWhiteSpace(prodata.VersionAlias) ? prodata.VersionAlias : versionAlias;
            }
            result.skus = new List<product_sku_model>();
            var skus = ProductManagerApplication.GetSKUs(prodata.Id);
            foreach (var item in skus)
            {
                product_sku_model skudata = new product_sku_model();
                skudata.sku_id = item.Id;
                skudata.outer_sku_id = item.Sku;
                skudata.price = item.SalePrice;
                skudata.quantity = (int)item.Stock;
                //skudata.sku_properties_name = "颜色:" + item.Color + "尺寸:" + item.Size + "版本:" + item.Version;
                skudata.sku_properties_name = colorAlias + ":" + item.Color + " " + sizeAlias + ":" + item.Size + " " + versionAlias + ":" + item.Version;
                string sku_properties_name = item.Color + " " + item.Size + " " + item.Version;
                if (string.IsNullOrWhiteSpace(sku_properties_name))
                {
                    skudata.sku_properties_name = "";
                }
                if (!string.IsNullOrWhiteSpace(skudata.sku_properties_name))
                {
                    result.skus.Add(skudata);
                }
            }
            #endregion

            #endregion

            return result;
        }
        /// <summary>
        /// 商品列表信息项转换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private product_list_model ProductInfoListMapChange(Entities.ProductInfo data)
        {
            product_list_model dresult = null;
            var prodata = data;
            dresult = new product_list_model();

            if (prodata == null)
            {
                throw new MallApiException(OpenApiErrorCode.Product_Not_Exists, "num_iid");
            }

            #region 装配信息
            dresult.num_iid = (int)prodata.Id;
            dresult.outer_id = prodata.ProductCode;
            dresult.brand_id = (int)prodata.BrandId;
            dresult.brand_name = prodata.BrandName;
            dresult.cid = (int)prodata.CategoryId;
            dresult.type_id = (int)prodata.TypeId;

            var category = CategoryApplication.GetCategory(prodata.CategoryId);
            if (category != null)
            {
                dresult.cat_name = category.Name;
                var type = TypeApplication.GetType(category.TypeId);
                if (type != null)
                    dresult.type_name = type.Name;
            }
            dresult.price = prodata.MinSalePrice;
            var skus = ProductManagerApplication.GetSKUs(prodata.Id);
            if (skus.Count > 0)
            {
                dresult.num = (int)skus.Sum(d => d.Stock);
            }
            else
            {
                dresult.num = 0;
                dresult.num = prodata.Quantity;
            }
            dresult.title = prodata.ProductName.Trim();
            dresult.list_time = prodata.AddedDate;
            dresult.modified = prodata.AddedDate;
            dresult.sold_quantity = (int)prodata.SaleCounts;
            dresult.pic_url = new System.Collections.ArrayList();
            dresult.pic_url.Add(System.IO.Path.Combine(OpenAPIHelper.HostUrl, prodata.GetImage(ImageSize.Size_350, 1)));
            ProductStatus ps = GetProductStatus(prodata);
            dresult.approve_status = ps.ToString();
            #endregion

            return dresult;
        }
        /// <summary>
        /// 商品列表信息转换
        /// </summary>
        /// <param name="datalist"></param>
        /// <returns></returns>
        private List<product_list_model> ProductInfoListMapChange(List<Entities.ProductInfo> datalist)
        {
            List<product_list_model> result = new List<product_list_model>();
            foreach (var item in datalist)
            {
                result.Add(ProductInfoListMapChange(item));
            }
            return result;
        }

        /// <summary>
        /// 获取商品状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private ProductStatus GetProductStatus(Entities.ProductInfo data)
        {
            ProductStatus result = ProductStatus.Un_Sale;
            if (data.AuditStatus == Entities.ProductInfo.ProductAuditStatus.Audited && data.SaleStatus == Entities.ProductInfo.ProductSaleStatus.OnSale)
            {
                result = ProductStatus.On_Sale;
            }
            if (data.SaleStatus == Entities.ProductInfo.ProductSaleStatus.InStock)
            {
                result = ProductStatus.In_Stock;
            }
            return result;
        }

        /// <summary>
        /// 获取并计算库存
        /// </summary>
        /// <param name="currentQuantity">当前库存</param>
        /// <param name="quantity">库存计算量</param>
        /// <param name="type">1全量更新 2增量更新</param>
        /// <returns></returns>
        private long GetStockQuantity(long currentQuantity, long quantity, int type)
        {
            long result = 0;
            switch (type)
            {
                case 2:
                    result = quantity;
                    break;
                default:
                    result = currentQuantity + quantity;
                    break;
            }
            if (result < 0)
            {
                throw new MallApiException(OpenApiErrorCode.Product_UpdateeQuantity_Faild, "quantity");
            }
            return result;
        }
        #endregion
    }
}
