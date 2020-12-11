using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.IServices;
using Mall.CommonModel;
using NetRube.Data;
using Mall.Entities;

namespace Mall.Service
{
    public class SpecificationService : ServiceBase, ISpecificationService
    {
        #region ISpecificationService 成员
        public List<SpecificationValueInfo> GetSpecification(long categoryId, long shopId)
        {
            var category = DbFactory.Default.Get<CategoryInfo>().Where(model => model.Id == categoryId).FirstOrDefault();

            var specifications = DbFactory.Default.Get<SpecificationValueInfo>().Where(model => model.TypeId == category.TypeId).OrderBy(a => a.Specification);

            var type = DbFactory.Default.Get<TypeInfo>(p => p.Id == category.TypeId).FirstOrDefault();
            //排除当前类型不支持的规格
            if (!type.IsSupportColor)
                specifications.Where(item => item.Specification != SpecificationType.Color);
            if (!type.IsSupportSize)
                specifications.Where(item => item.Specification != SpecificationType.Size);
            if (!type.IsSupportVersion)
                specifications.Where(item => item.Specification != SpecificationType.Version);

            var result = specifications.ToList();

            //获取商家自定义的规格
            var shopSpecifications = DbFactory.Default.Get<SellerSpecificationValueInfo>().Where(p => p.ShopId == shopId && p.TypeId == category.TypeId).ToList();
            //覆盖平台默认规格
            foreach (var item in shopSpecifications)
            {
                var temp = result.FirstOrDefault(model => model.Id == item.ValueId);
                if (temp != null)
                    temp.Value = item.Value;
            }

            return result;
        }
        #endregion
    }
}
