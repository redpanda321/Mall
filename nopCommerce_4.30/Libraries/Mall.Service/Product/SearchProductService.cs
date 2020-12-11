using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using Nop.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class SearchProductService : ServiceBase, ISearchProductService
    {

        public void AddSearchProduct(long productId)
        {
            var sql = new Sql();
            sql.Append("INSERT INTO `Mall_searchproduct` (`ProductId`, `ProductName`, `ShopId`, `ShopName`, `BrandId`, `BrandName`, `FirstCateId`, `FirstCateName`, `SecondCateId`, `SecondCateName`, `ThirdCateId`, `ThirdCateName`, `AttrValues`, `Comments`, `SaleCount`, `SalePrice`, `OnSaleTime`, `ImagePath`, `CanSearch`, `BrandLogo`) ");
            sql.Append("select a.Id,a.ProductName,a.ShopId,b.ShopName,a.BrandId,c.`Name` as BrandName,SUBSTRING_INDEX(a.CategoryPath, '|', 1) AS FirstCateId, d.Name as FirstCateName,SUBSTRING_INDEX(SUBSTRING_INDEX(CategoryPath, '|', 2), '|', -1) AS SecondCateId, e.Name as SecondCateName,SUBSTRING_INDEX(CategoryPath, '|', -1) AS ThirdCateId, f.Name as ThirdCateName,g.AttrValues,0,0,a.MinSalePrice,a.AddedDate,a.ImagePath,(case when a.SaleStatus=1 and a.AuditStatus=2 and a.IsDeleted=0 then 1 else 0 end) as CanSearch,c.Logo from Mall_product a ");
            sql.Append("left join Mall_shop b on a.ShopId = b.Id ");
            sql.Append("left join Mall_brand c on a.BrandId = c.Id ");
            sql.Append("left join Mall_category d on SUBSTRING_INDEX(a.CategoryPath, '|', 1) = d.Id ");
            sql.Append("left join Mall_category e on SUBSTRING_INDEX(SUBSTRING_INDEX(a.CategoryPath, '|', 2), '|', -1) = e.Id ");
            sql.Append("left join Mall_category f on SUBSTRING_INDEX(a.CategoryPath,'|', -1) = f.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from Mall_productattribute group by productId) g on a.Id = g.ProductId ");
            sql.Append("where a.Id=@0", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateSearchProduct(long productId)
        {
            var sql = new Sql();
            sql.Append("update Mall_searchproduct a ");
            sql.Append("left join Mall_product b on a.ProductId = b.Id ");
            sql.Append("left join Mall_category c on SUBSTRING_INDEX(b.CategoryPath, '|', 1) = c.Id ");
            sql.Append("left join Mall_category d on SUBSTRING_INDEX(SUBSTRING_INDEX(b.CategoryPath, '|', 2), '|', -1) = d.Id ");
            sql.Append("left join Mall_category e on SUBSTRING_INDEX(b.CategoryPath, '|', -1) = e.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from Mall_productattribute group by productId) f on a.ProductId = f.ProductId ");
            sql.Append("left join Mall_shop g on b.ShopId = g.Id ");
            sql.Append("left join Mall_brand h on b.BrandId = h.Id ");
            sql.Append("set a.ProductName = b.ProductName,a.ShopName = g.ShopName,a.BrandId=IFNULL(h.Id,0),a.BrandName = h.`Name`,a.BrandLogo = h.Logo,a.FirstCateId = c.Id,a.FirstCateName = c.`Name`, ");
            sql.Append("a.SecondCateId = d.Id,a.SecondCateName = d.`Name`,a.ThirdCateId = e.Id,a.ThirdCateName = e.`Name`,a.AttrValues = f.AttrValues,a.SalePrice = b.MinSalePrice,a.ImagePath = b.ImagePath, ");
            sql.Append("a.CanSearch = (case when b.SaleStatus =1 and b.AuditStatus = 2 and b.IsDeleted=0 then 1 else 0 end) ");
            sql.Append("where a.ProductId=@0 ", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateShop(long shopId, string shopName)
        {
            var sql = new Sql("update Mall_searchProduct set ShopName=@0 where ShopId=@1", shopName, shopId);
            DbFactory.Default.Execute(sql);//, new { ShopName = shopName, ShopId = shopId });
            DbFactory.Default.Set<ActiveMarketServiceInfo>().Set(p => p.ShopName, shopName).Where(p => p.ShopId == shopId).Succeed();
        }

        public void UpdateSearchStatusByProduct(long productId)
        {
            var sql = new Sql("update Mall_searchproduct a left join Mall_product b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid=@0", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateSearchStatusByProducts(List<long> productIds)
        {
            var sql = new Sql("update Mall_searchproduct a left join Mall_product b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid in (@0)", productIds);

            DbFactory.Default.Execute(sql);
        }
        public void UpdateSearchStatusByShop(long shopId)
        {
            var sql = new Sql("update Mall_searchproduct a left join Mall_product b on a.productid = b.id set CanSearch = 0 where b.shopid = @0 and cansearch=1; ", shopId);
            sql.Append(" update Mall_searchproduct a left join Mall_product b on a.productid = b.id set CanSearch = 1 where a.cansearch=0 and a.shopid = @0 and b.AuditStatus = 2 and b.SaleStatus = 1 and b.IsDeleted = 0; ", shopId);
            DbFactory.Default.Execute(sql);
            //cmd.Parameters.AddWithValue("@id", shopId);
            //cmd.ExecuteNonQuery();
        }

        public void UpdateBrand(BrandInfo brand)
        {
            var sql = new Sql("update Mall_searchProduct set BrandName=@0,BrandLogo=@1 where BrandId=@2", brand.Name, brand.Logo, brand.Id);

            DbFactory.Default.Execute(sql);

        }
        public void UpdateCategory(CategoryInfo category)
        {
            var sql = "update Mall_searchProduct set {0}CateName=@0 where {0}CateId=@1";
            switch (category.Depth)
            {
                case 1:
                    sql = string.Format(sql, "First");
                    break;
                case 2:
                    sql = string.Format(sql, "Second");
                    break;
                case 3:
                    sql = string.Format(sql, "Third");
                    break;
            }
            DbFactory.Default.Execute(new Sql(sql, category.Name, category.Id));//, new { CateName = category.Name, CateId = category.Id });
        }

        /// <summary>
        /// 商品搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductResult SearchProduct(SearchProductQuery query)
        {
            //string selComments = "s.Comments";
            //以前评论条数是搜索表里的 统计的所有评论条数
            string selComments = "(SELECT COUNT(*) FROM Mall_productcomment pc WHERE pc.productid=s.productid AND pc.Ishidden=0) as Comments";//搜索这里评论数获取排除隐藏的条数

            SearchProductResult result = new SearchProductResult();
            string countsql = "select count(1) from Mall_searchproduct s  ";
            string sql = "select (p.SaleCounts+ IFNULL(p.VirtualSaleCounts,0)) as  SaleCount, s.ProductId,s.ProductName,s.SalePrice,s.ImagePath,s.ShopId,s.ShopName,s.ThirdCateId,p.DisplaySequence,p.ShopDisplaySequence,p.ProductType,IFNULL(p.VirtualSaleCounts,0) as  VirtualSaleCounts,p.MinSalePrice," + selComments + "   from Mall_searchproduct s ";

            var where = new Sql();
            GetSearchWhere(query, where);
            var order = new Sql();
            GetSearchOrder(query, order);
            string index = GetForceIndex(query);
            string page = GetSearchPage(query);
            index += string.Format(" left join Mall_Product p on p.Id=s.ProductId  ");//FORCE INDEX要放在join之前
            countsql += string.Format(" left join Mall_Product p on p.Id=s.ProductId  ");
            result.Data = DbFactory.Default.Query<ProductView>(string.Concat(sql, index, where.SQL, order.SQL, page), where.Arguments).ToList();
            result.Total = DbFactory.Default.ExecuteScalar<int>(string.Concat(countsql, where.SQL), where.Arguments);

            return result;
        }

        /// <summary>
        /// 商品属性、分类、品牌搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductFilterResult SearchProductFilter(SearchProductQuery query)
        {
            try
            {
                var result = new SearchProductFilterResult();
                var where = new Sql();
                GetSearchWhere(query, where);
                //将参数转为MySql参数
                //var @params = temp.ParameterNames.Select(name => new MySqlParameter("@" + name, temp.Get<object>(name))).ToArray();

                //取出缓存中的属性数据
                var listAttr = Cache.Get<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
                var listAttrVal = Cache.Get<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);

                //如果没有则从数据库查询半缓存，缓存时间为默认时间
                if (listAttr == null)
                {
                    listAttr = DbFactory.Default.Get<AttributeInfo>().ToList();
                    Cache.Insert(CacheKeyCollection.CACHE_ATTRIBUTE_LIST, listAttr);
                }

                //如果没有则从数据库查询半缓存，缓存时间为默认时间
                if (listAttrVal == null)
                {
                    listAttrVal = DbFactory.Default.Get<AttributeValueInfo>().ToList();
                    Cache.Insert(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST, listAttrVal);
                }

                //查出符合条件的属性值
                string attrSql = "select AttrValues from Mall_searchproduct s  left join Mall_Product p on p.Id=s.ProductId  " + where.SQL;
                var sttrValueIds = DbFactory.Default.Query<string>(attrSql, where.Arguments).ToList();
                //按','拆分
                var valueIds = sttrValueIds.Where(i => !string.IsNullOrWhiteSpace(i)).SelectMany(item => item.Split(',')).ToList();
                //过滤符合结果的属性值
                listAttrVal = listAttrVal.Where(r => valueIds.Contains(r.Id.ToString())).ToList();
                listAttr = listAttr.Where(r => listAttrVal.Select(z => z.AttributeId).Contains(r.Id)).ToList();

                result.Attribute = listAttr.Select(r => new AttributeView()
                {
                    AttrId = r.Id,
                    Name = r.Name,
                    AttrValues = listAttrVal.Where(z => z.AttributeId == r.Id).Select(s =>
                    new AttributeValue()
                    {
                        Id = s.Id,
                        Name = s.Value
                    }).ToList()
                }).ToList();

                //查询符合条件的品牌
                var brandSql = "select DISTINCT s.BrandId Id,BrandName Name,BrandLogo Logo from Mall_searchproduct s  left join Mall_Product p on p.Id=s.ProductId  " + where.SQL + " and s.BrandId is not null AND s.BrandId<>0";
                result.Brand = DbFactory.Default.Query<BrandView>(brandSql, where.Arguments).ToList();

             
                var allCategorys =  EngineContext.Current.Resolve<ICategoryService>().GetCategories();

                //查询符合条件的分类
                var categorySql = "select FirstCateId,MAX(FirstCateName) FirstCateName,SecondCateId,MAX(SecondCateName) SecondCateName,ThirdCateId,MAX(ThirdCateName) ThirdCateName from Mall_searchproduct s  left join Mall_Product p on p.Id=s.ProductId  " + where.SQL + " group by  FirstCateId , SecondCateId , ThirdCateId ";
                var data = DbFactory.Default.Query<CategorySeachModel>(categorySql, where.Arguments).ToList();
                result.Category = data.GroupBy(item => item.FirstCateId).Select(first => new CategoryView//根据一级分类分组
                {
                    Id = first.Key,
                    Name = first.Select(item => item.FirstCateName).FirstOrDefault(),
                    IsShow = allCategorys.FirstOrDefault(a => a.Id == first.Key) != null ? allCategorys.FirstOrDefault(a => a.Id == first.Key).IsShow : false,
                    SubCategory = first.GroupBy(item => item.SecondCateId).Select(second => new CategoryView//根据二级分类分组
                    {
                        Id = second.Key,
                        Name = second.Select(item => item.SecondCateName).FirstOrDefault(),
                        IsShow = allCategorys.FirstOrDefault(a => a.Id == second.Key) != null ? allCategorys.FirstOrDefault(a => a.Id == second.Key).IsShow : false,
                        SubCategory = second.GroupBy(item => item.ThirdCateId).Select(three => new CategoryView//根据三级分类分组
                        {
                            Id = three.Key,
                            Name = three.Select(item => item.ThirdCateName).FirstOrDefault(),
                            IsShow = allCategorys.FirstOrDefault(a => a.Id == three.Key) != null ? allCategorys.FirstOrDefault(a => a.Id == three.Key).IsShow : false
                        }).ToList()
                    }).ToList()
                }).ToList();
                result.Category = result.Category.Where(a => a.IsShow.HasValue && a.IsShow.Value).ToList();
                result.Category.ForEach(r =>
                {
                    r.SubCategory = r.SubCategory.Where(a => a.IsShow.HasValue && a.IsShow.Value).ToList();
                    r.SubCategory.ForEach(p =>
                    {
                        p.SubCategory = p.SubCategory.Where(b => b.IsShow.HasValue && b.IsShow.Value).ToList();
                    });
                });

                return result;
            }
            catch (Exception ex)
            {

                Log.Error("搜索不出来了：", ex);
                return new SearchProductFilterResult();
            }
        }
        #region 组装sql
        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private void GetSearchWhere(SearchProductQuery query, Sql where)
        {
            where.Append(" WHERE s.CanSearch=1 ");
            #region 过滤条件

            if (query.FilterVirtualProduct.HasValue && query.FilterVirtualProduct.Value)
            {
                where.Append(" AND p.ProductType=0 ");
            }

            if (query.ShopId != 0)
            {
                where.Append(" AND s.ShopId=@0 ", query.ShopId);
                //parms.Add("@ShopId", query.ShopId);
            }

            if (query.VShopId != 0)
            {
                where.Append(" AND s.ShopId IN (SELECT ShopId FROM Mall_vshop where Id=@0) ", query.VShopId);
                //parms.Add("@VShopId", query.VShopId);
            }

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
            {
                where.Append(" AND s.ProductId IN (select ProductId from Mall_productshopcategory where ShopCategoryId in(select id from Mall_ShopCategory where ShopId = @0 and(id = @1 or ParentCategoryId = @1))) ", query.ShopId, query.ShopCategoryId);
                //parms.Add("@ShopCategoryId", query.ShopCategoryId);

            }

            if (query.BrandId != 0)
            {
                where.Append(" AND s.BrandId=@0 ", query.BrandId);
                //parms.Add("@BrandId", query.BrandId);

            }

            if (query.FirstCateId != 0)
            {
                where.Append(" AND s.FirstCateId=@0 ", query.FirstCateId);
                //parms.Add("@FirstCateId", query.FirstCateId);

            }
            else if (query.SecondCateId != 0)
            {
                where.Append(" AND s.SecondCateId=@0 ", query.SecondCateId);
                //parms.Add("@SecondCateId", query.SecondCateId);

            }
            else if (query.ThirdCateId != 0)
            {
                where.Append(" AND s.ThirdCateId=@0 ", query.ThirdCateId);
                //parms.Add("@ThirdCateId", query.ThirdCateId);

            }

            if (query.StartPrice >= 0)
            {
                where.Append(" AND s.SalePrice>=@0 ", query.StartPrice);
                //parms.Add("@StartPrice", query.StartPrice);

            }

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
            {
                where.Append(" AND s.SalePrice <= @0 ", query.EndPrice);
                //parms.Add("@EndPrice", query.EndPrice);

            }

            if (query.AttrValIds.Count > 0)
            {
                where.Append("  AND s.ProductId IN (SELECT DISTINCT ProductId FROM Mall_ProductAttribute ");
                //此处属性筛选，要取交集非并集
                foreach (var item in query.AttrValIds)
                {
                    where.Append(" INNER JOIN (SELECT DISTINCT ProductId FROM Mall_ProductAttribute WHERE  (ValueId = " + item + " ) ) t" + query.AttrValIds.IndexOf(item) + " USING (ProductId) ");
                }
                where.Append(")");


            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                if (!query.IsLikeSearch)
                {
                    where.Append(" AND MATCH(s.ProductName) AGAINST(@0 IN BOOLEAN MODE) ", string.Concat(UrnHtml(query.Keyword.TrimEnd(' ')), "*").Replace(" ", "*"));
                    //parms.Add("@ProductName", string.Concat(query.Keyword, "*").Replace(" ", "*"));

                }
                else
                {
                    where.Append(" AND s.ProductName like @0 ", "%" + query.Keyword + "%");
                    //parms.Add("@ProductName", "%" + query.Keyword + "%");

                }
            }
            #endregion
        }

        /// <summary>
        /// 特殊表意符过滤(mysql有些数据库版本用AGAINST查询有些特殊符查询有误)
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        private string UrnHtml(string strHtml)
        {
            if (string.IsNullOrEmpty(strHtml))
                return string.Empty;

            string[] aryReg = { "'", "<", ">", "%", "\"", "+", "-", ">=", "=<" };
            for (int i = 0; i < aryReg.Length; i++)
            {
                strHtml = strHtml.Replace(aryReg[i], string.Empty);
            }
            return strHtml;
        }

        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private void GetSearchOrder(SearchProductQuery query, Sql order)
        {
            switch (query.OrderKey)
            {
                case 2:
                    order.Append(" ORDER BY SaleCount ");
                    break;
                case 3:
                    order.Append(" ORDER BY s.SalePrice ");
                    break;
                case 4:
                    //order.Append(" ORDER BY s.Comments ");
                    order.Append(" ORDER BY Comments ");//Comments现是子查询字段不是搜索产品表字段，则不需s.别名
                    break;
                case 5:
                    order.Append(" ORDER BY s.OnSaleTime ");
                    break;
                default:
                    if (query.ShopId > 0 || query.VShopId > 0)
                    {
                        order.Append(" ORDER BY p.ShopDisplaySequence desc , SaleCount desc , s.OnSaleTime desc ");
                    }
                    else
                    {
                        //按最新的排序规则作为默认排序【序号越大，在前台展示的商品越靠前，序号一致时，优先销量排前，销量一致时，优先上架时间排前】
                        order.Append(" ORDER BY p.DisplaySequence desc , SaleCount  desc ,s.OnSaleTime desc ");

                    }
                    break;
            }
            if (query.OrderKey > 1)//如果是默认，则按上面执行
            {
                if (!query.OrderType)
                    order.Append(" DESC ");
                else
                    order.Append(" ASC ");
            }
        }

        /// <summary>
        /// 非主键排序时强制使用索引
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetForceIndex(SearchProductQuery query)
        {
            if (!string.IsNullOrEmpty(query.Keyword))
                return string.Empty;

            string index = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    index = " FORCE INDEX(IX_SaleCount) ";
                    break;
                case 3:
                    index = " FORCE INDEX(IX_SalePrice)  ";
                    break;
                case 4:
                    index = " FORCE INDEX(IX_Comments) ";
                    break;
                case 5:
                    index = " FORCE INDEX(IX_OnSaleTime) ";
                    break;
            }

            return index;
        }
        /// <summary>
        /// 获取搜索商品分页sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchPage(SearchProductQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNumber - 1) * query.PageSize, query.PageSize);
        }
        #endregion

        #region 小程序商品查询


        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private void GetAppletSearchWhere(SearchProductQuery query, Sql where)
        {
            where.Append("WHERE CanSearch=1 ");
            #region 过滤条件
            if (query.ShopId != 0)
                where.Append("AND ps.ShopId=@0 ", query.ShopId);

            if (query.VShopId != 0)
                where.Append(" AND ps.ShopId IN (SELECT ShopId FROM Mall_vshop where Id=@0) ", query.VShopId);

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
                where.Append(" AND ps.ProductId IN (select ProductId from Mall_productshopcategory where ShopCategoryId in (select id from Mall_ShopCategory where ShopId = @1 and(id = @0 or ParentCategoryId = @0))) ", query.ShopCategoryId, query.ShopId);

            if (query.BrandId != 0)
                where.Append("AND ps.BrandId=@0 ", query.BrandId);

            if (query.FirstCateId != 0)
                where.Append("AND ps.FirstCateId=@0 ", query.FirstCateId);

            else if (query.SecondCateId != 0)
                where.Append("AND ps.SecondCateId=@0 ", query.SecondCateId);

            else if (query.ThirdCateId != 0)
                where.Append("AND ps.ThirdCateId=@0 ", query.ThirdCateId);

            if (query.StartPrice > 0)
                where.Append(" AND ps.SalePrice>=@0 ", query.StartPrice);

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
                where.Append(" AND ps.SalePrice <= @EndPrice ");

            if (query.AttrValIds.Count > 0)
                where.Append(" AND ps.ProductId IN (SELECT DISTINCT ProductId FROM Mall_ProductAttribute WHERE ValueId IN (@0)) ", query.AttrValIds);

            if (!string.IsNullOrEmpty(query.Keyword))
                if (!query.IsLikeSearch)
                    where.Append("AND MATCH(ps.ProductName) AGAINST(@0 IN BOOLEAN MODE) ", string.Concat(UrnHtml(query.Keyword), "*").Replace(" ", "*"));
                else
                    where.Append("AND ps.ProductName like @0 ", "%" + query.Keyword + "%");
            #endregion
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private void GetAppletSearchOrder(SearchProductQuery query, Sql order)
        {
            switch (query.OrderKey)
            {
                case 2:
                    order.Append(" ORDER BY ps.SaleCount ");
                    break;
                case 3:
                    order.Append(" ORDER BY ps.SalePrice ");
                    break;
                case 4:
                    order.Append(" ORDER BY ps.Comments ");
                    break;
                case 5:
                    order.Append(" ORDER BY ps.OnSaleTime ");
                    break;
                default:
                    order.Append(" ORDER BY ps.Id ");
                    break;
            }
            if (!query.OrderType)
                order.Append(" DESC ");
            else
                order.Append(" ASC ");
        }
        #endregion


        /// <summary>
        /// 同步商品的查询价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="newPrice"></param>
        public void UpdateSearchProductPrice(long productId, long shopId, decimal newPrice)
        {
            DbFactory.Default.Set<SearchProductInfo>().Set(p => p.SalePrice, newPrice).Where(p => p.ProductId == productId && p.ShopId == shopId).Succeed();
        }

        public SearchProductInfo GetSingleSearchProductInfo(long productId, long shopId)
        {
            return DbFactory.Default.Get<SearchProductInfo>().Where(p => p.ProductId == productId && p.ShopId == shopId).FirstOrDefault();
        }
    }
}
