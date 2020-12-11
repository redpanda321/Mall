
using Mall.CommonModel;
using Mall.Core;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{

    /// <summary>
    /// 属性比较器
    /// </summary>
    public class AttrComparer : IEqualityComparer<AttributeInfo>
    {
        public bool Equals(AttributeInfo x, AttributeInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.Id != 0 && y.Id != 0;
        }
        public int GetHashCode(AttributeInfo attr)
        {
            if (Object.ReferenceEquals(attr, null)) return 0;
            int Id = (int)(attr.Id ^ attr.TypeId);
            return Id;
        }
    }

    public class AttrValueComparer : IEqualityComparer<AttributeValueInfo>
    {
        public bool Equals(AttributeValueInfo x, AttributeValueInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Value == y.Value;
        }
        public int GetHashCode(AttributeValueInfo attr)
        {
            if (Object.ReferenceEquals(attr, null)) return 0;
            int Id = attr.Value.GetHashCode();
            return Id;
        }
    }

    public class SpecValueComparer : IEqualityComparer<SpecificationValueInfo>
    {
        public bool Equals(SpecificationValueInfo x, SpecificationValueInfo y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Value == y.Value;
        }
        public int GetHashCode(SpecificationValueInfo spec)
        {
            if (Object.ReferenceEquals(spec, null)) return 0;
            int Id = spec.Value.GetHashCode();
            return Id;
        }
    }

    public class TypeService : ServiceBase, ITypeService
    {
        public TypeInfo GetType(long id)
        {
            return DbFactory.Default.Get<TypeInfo>().Where(p => p.Id == id && p.IsDeleted == false).FirstOrDefault();
        }
        public List<TypeInfo> GetTypes(List<long> ids) {
            return DbFactory.Default.Get<TypeInfo>().Where(p => p.Id.ExIn(ids) && p.IsDeleted == false).ToList();
        }

        public TypeInfo GetTypeByProductId(long productId)
        {
            return DbFactory.Default
                .Get<TypeInfo>()
                .InnerJoin<ProductInfo>((ti, pi) => ti.Id == pi.TypeId && pi.Id == productId)
                .FirstOrDefault();
        }

        /// <summary>
        /// 采取遍历列表更新的策略
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttr(TypeInfo model)
        {
            ProcessingAttrDeleteAndAdd(model.Id, model.AttributeInfo);
            ProcessingAttrUpdate(model.Id, model.AttributeInfo);
        }

        /// <summary>
        /// 更新属性，不会删除属性，只会删除和添加属性值
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttrUpdate(long type, List<AttributeInfo> models)
        {
            var attributes = DbFactory.Default.Get<AttributeInfo>().Where(a => a.TypeId == type).ToList();

            var attrs = attributes.Select(p => p.Id).ToList();
            var values = DbFactory.Default.Get<AttributeValueInfo>(p => p.AttributeId.ExIn(attrs)).ToList();

            foreach (var item in models.Where(p => p.Id > 0))
            {
                var attr = attributes.FirstOrDefault(a => a.Id == item.Id);
                var vals = values.Where(p => p.AttributeId == attr.Id).ToList();
                attr.Name = item.Name;
                attr.IsMulti = item.IsMulti;
                DbFactory.Default.Update(attr);

                #region 计算不删除原有相同名称属性，添加新的属性
                List<long> delValueId = new List<long>();
                foreach (var vvitem in vals)
                {
                    var firstvalue = item.AttributeValueInfo != null ? item.AttributeValueInfo.Where(t => t.Value == vvitem.Value).FirstOrDefault() : null;
                    if (firstvalue == null || string.IsNullOrEmpty(vvitem.Value))
                        delValueId.Add(vvitem.Id);//说明当前属性已不存在或空值则删除掉
                    else
                        item.AttributeValueInfo.Remove(firstvalue);//当前集合查找已存的删除掉查询里的，则后面不会重复添加；
                }

                if (delValueId.Count > 0)
                    DbFactory.Default.Del<AttributeValueInfo>(p => p.Id.ExIn(delValueId));//它原先空的值删除(例如多个逗号隔开)，如不先删除下面比较查询不了
                                
                if (item.AttributeValueInfo!=null && item.AttributeValueInfo.Where(t=>t.Value!="").Count() > 0)
                {
                    item.AttributeValueInfo.ForEach(p => p.AttributeId = attr.Id);
                    DbFactory.Default.AddRange(item.AttributeValueInfo.Where(t => t.Value != ""));
                }                
                #endregion
            }
        }

        /// <summary>
        /// 处理属性的添加、删除操作
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingAttrDeleteAndAdd(long type, List<AttributeInfo> models)
        {
            var attrs = DbFactory.Default.Get<AttributeInfo>(p => p.TypeId == type).ToList();

            //需要删除的属性
            var deleteAttr = attrs.Except(models, new AttrComparer()).ToList();
            var deleteAttrId = deleteAttr.Select(a => a.Id).ToList();
            DbFactory.Default.Del<AttributeValueInfo>(a => a.AttributeId.ExIn(deleteAttrId));
            DbFactory.Default.Del<AttributeInfo>(deleteAttr);

            //需要添加的属性
            var addAttr = models.Except(attrs, new AttrComparer()).ToList();
            if (addAttr.Count > 0)
                DbFactory.Default.AddRange(addAttr);
        }


        public void SaveType(TypeInfo type)
        {
            DbFactory.Default.Save(type);
        }

        
        public void SaveBrands(long type, List<long> brands)
        {
            DbFactory.Default.Del<TypeBrandInfo>(p => p.TypeId == type);
            var brandList = brands.Select(p => new TypeBrandInfo
            {
                TypeId = type,
                BrandId = p
            });
            DbFactory.Default.AddRange(brandList);
        }

        public List<long> GetBrandsByType(long type) {
            return DbFactory.Default.Get<TypeBrandInfo>(p => p.TypeId == type).Select(p => p.BrandId).ToList<long>();
        }

        private void UpdateSpecificationValues(TypeInfo model, SpecificationType specEnum)
        {
            var newSpec = model.SpecificationValueInfo.Where(s => s.Specification == specEnum);
            var values = DbFactory.Default.Get<SpecificationValueInfo>()
                .Where(s => s.TypeId == model.Id && s.Specification == specEnum)
                .ToList();

            var deleteSpec = values.Except(newSpec, new SpecValueComparer());
            DbFactory.Default.Del(deleteSpec);

            var addSpec = newSpec.Except(values, new SpecValueComparer());
            DbFactory.Default.Add(addSpec);
        }

        /// <summary>
        /// 采取更新的策略
        /// </summary>
        /// <param name="model"></param>
        private void ProcessingSpecificationValues(TypeInfo model)
        {
            var _model = DbFactory.Default.Get<TypeInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            //颜色
            if (model.IsSupportColor)
            {
                UpdateSpecificationValues(model, SpecificationType.Color);
                _model.ColorAlias = model.ColorAlias;
            }

            //尺寸
            if (model.IsSupportSize)
            {
                UpdateSpecificationValues(model, SpecificationType.Size);
                _model.SizeAlias = model.SizeAlias;
            }

            //版本
            if (model.IsSupportVersion)
            {
                UpdateSpecificationValues(model, SpecificationType.Version);
                _model.VersionAlias = model.VersionAlias;
            }

            _model.IsSupportVersion = model.IsSupportVersion;
            _model.IsSupportColor = model.IsSupportColor;
            _model.IsSupportSize = model.IsSupportSize;
            DbFactory.Default.Update(_model);
        }

        private void ProcessingCommon(TypeInfo model)
        {
            var actual = DbFactory.Default.Get<TypeInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
            actual.Name = model.Name;
            DbFactory.Default.Update(actual);
        }

        public void UpdateType(TypeInfo model)
        {
            DbFactory.Default.InTransaction(() =>
            {
                //更新的属性、属性值
                ProcessingAttr(model);

                //更新规格
                ProcessingSpecificationValues(model);

                //更新常用属性
                ProcessingCommon(model);

                SaveBrands(model.Id, model.TypeBrandInfo.Select(p => p.BrandId).ToList());

                Cache.Remove(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
                Cache.Remove(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);
            });

        }

        public void DeleteType(long id)
        {
            var existCategory = DbFactory.Default.Get<CategoryInfo>().Where(p => p.TypeId == id && p.IsDeleted == false).Exist();

            if (existCategory)
                throw new MallException("该类型已经有分类关联，不能删除.");

            var sql = new Sql("UPDATE Mall_Type SET IsDeleted=@1 WHERE Id=@0",id,true);
            DbFactory.Default.Execute(sql);
        }

        public void AddType(TypeInfo model)
        {
            DbFactory.Default.InTransaction(()=> 
            {
                //基本信息
                DbFactory.Default.Add(model);

                //关联品牌
                model.TypeBrandInfo.ForEach(p => p.TypeId = model.Id);
                DbFactory.Default.AddRange(model.TypeBrandInfo);
                
             
                //关联属性
                model.AttributeInfo.ForEach(p => p.TypeId = model.Id);
                DbFactory.Default.Add<AttributeInfo>(model.AttributeInfo);

                //关联属性值
                foreach (var attr in model.AttributeInfo)
                    attr.AttributeValueInfo.ForEach(p => p.AttributeId = attr.Id);
                var values = model.AttributeInfo.SelectMany(p => p.AttributeValueInfo).ToList();
                DbFactory.Default.Add<AttributeValueInfo>(values);

                //添加规格值
                model.SpecificationValueInfo.ForEach(p => p.TypeId = model.Id);
                DbFactory.Default.Add<SpecificationValueInfo>(model.SpecificationValueInfo);
            });
        }

        public List<TypeInfo> GetTypes()
        {
            return DbFactory.Default.Get<TypeInfo>().Where(p => p.IsDeleted == false).ToList();
        }

        public QueryPageModel<TypeInfo> GetTypes(string search, int pageNo, int pageSize)
        {
            var list = DbFactory.Default.Get<TypeInfo>().Where(p => p.IsDeleted == false);
            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(p => p.Name.Contains(search));
            var data = list.OrderByDescending(pp => pp.Id).ToPagedList(pageNo, pageSize);

            var result = new QueryPageModel<TypeInfo>()
            {
                Total = data.TotalRecordCount,
                Models = data
            };

            return result;
        }

        public List<SpecificationValueInfo> GetValuesByType(long type) {
            return DbFactory.Default.Get<SpecificationValueInfo>(p => p.TypeId == type).ToList();
        }

        public List<AttributeInfo> GetAttributesByType(long type)
        {
            return DbFactory.Default.Get<AttributeInfo>(p => p.TypeId == type).ToList();

        }
        public AttributeInfo GetAttribute(long id) {
            return DbFactory.Default.Get<AttributeInfo>(p => p.Id == id).FirstOrDefault();
        }
        public List<AttributeValueInfo> GetAttributeValues(long attribute) {
            return DbFactory.Default.Get<AttributeValueInfo>(p => p.AttributeId == attribute).ToList();
        }
        public List<AttributeValueInfo> GetAttributeValues(List<long> attributes)
        {
            return DbFactory.Default.Get<AttributeValueInfo>(p => p.AttributeId.ExIn(attributes)).ToList();
        }
    }
}
