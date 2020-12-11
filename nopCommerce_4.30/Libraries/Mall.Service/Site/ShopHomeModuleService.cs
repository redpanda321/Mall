using Mall.Core;
using Mall.DTO;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mall.Service
{
    public class ShopHomeModuleService : ServiceBase, IShopHomeModuleService
    {
        #region 老版本
        public void AddShopProductModule(ShopHomeModuleInfo shopProductModuleInfo)
        {
            if (string.IsNullOrWhiteSpace(shopProductModuleInfo.Name))
                throw new InvalidPropertyException("商品模块名称不能为空");

            DbFactory.Default.Add(shopProductModuleInfo);
        }

        public void UpdateShopProductModuleName(long shopId, long id, string name)
        {
            var flag = DbFactory.Default.Set<ShopHomeModuleInfo>().Set(n => n.Name, name).Where(item => item.Id == id && item.ShopId == shopId).Succeed();
            if (!flag) throw new MallException("在本店铺中未找到指定商品模块");
        }

        public void UpdateShopProductModuleProducts(long shopId, long id, IEnumerable<long> productIds)
        {
            var module = DbFactory.Default.Get<ShopHomeModuleInfo>().Where(item => item.Id == id && item.ShopId == shopId).FirstOrDefault();
            if (module == null)
                throw new MallException("在本店铺中未找到指定商品模块");

            var products = DbFactory.Default.Get<ShopHomeModuleProductInfo>().Where(p => p.HomeModuleId == module.Id).Select(p => p.ProductId).ToList<long>();

            //待删除的id
            var needToDeleteIds = products.Where(item => !productIds.Contains(item)).ToList();
            //待添加的商品id
            var list = productIds.Where(item => !products.Contains(item)).Select(p => new ShopHomeModuleProductInfo
            {
                ProductId = p,
                HomeModuleId = module.Id,
            });
            //删除
            DbFactory.Default.Del<ShopHomeModuleProductInfo>(item => item.Id.ExIn(needToDeleteIds));
            DbFactory.Default.AddRange(list);
        }


        public void Delete(long shopId, long id)
        {
            DbFactory.Default.Del<ShopHomeModuleInfo>(item => item.Id == id && item.ShopId == shopId);
        }


        public List<ShopHomeModuleInfo> GetAllShopHomeModuleInfos(long shopId)
        {
            var list = DbFactory.Default.Get<ShopHomeModuleInfo>().Where(p => p.DisplaySequence == 0 && p.ShopId == shopId).ToList();
            if (list.Count() > 1)
            {
                int index = 0;
                foreach (var model in list)
                {
                    DbFactory.Default.Set<ShopHomeModuleInfo>().Set(n => n.DisplaySequence, index++).Where(p => p.Id == model.Id).Succeed();
                }
            }
            return DbFactory.Default.Get<ShopHomeModuleInfo>().Where(item => item.ShopId == shopId).OrderBy(p => p.DisplaySequence).ToList();
        }

        public List<ShopHomeModuleTopImgInfo> GetImages(List<long> modules) {
            return DbFactory.Default.Get<ShopHomeModuleTopImgInfo>(p => p.HomeModuleId.ExIn(modules)).ToList();
        }

        public List<ShopHomeModuleProductInfo> GetProducts(List<long> modules) {
            return DbFactory.Default.Get<ShopHomeModuleProductInfo>(p => p.HomeModuleId.ExIn(modules)).ToList();
        }

        public ShopHomeModuleInfo GetShopHomeModuleInfo(long shopId, long id)
        {
            return DbFactory.Default.Get<ShopHomeModuleInfo>().Where(item => item.ShopId == shopId && item.Id == id).FirstOrDefault();
        }
        #endregion

        public string GetFooter(long shopid)
        {
            var model = DbFactory.Default.Get<ShopFooterInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (model == null)
            {
                model = new ShopFooterInfo();
                model.ShopId = shopid;
                model.Footer = "";
                DbFactory.Default.Add(model);
            }

            return model == null ? "" : model.Footer;
        }

        public void EditFooter(long shopid, string footer)
        {
            
            DbFactory.Default.Set<ShopFooterInfo>().Set(n => n.Footer, footer).Where(p => p.ShopId == shopid).Succeed();
        }

        public void SaveFloor(AddShopHomeModuleModel model)
        {
            if (model.Id <= 0) //新增
            {
                var rows = DbFactory.Default.Get<ShopHomeModuleInfo>().Count();
                int count = rows > 0 ? DbFactory.Default.Get<ShopHomeModuleInfo>().Max<int>(p => p.DisplaySequence) : 0;
                ShopHomeModuleInfo main = new ShopHomeModuleInfo();
                main.Name = model.Name;
                main.Url = model.Url;
                main.ShopId = model.ShopId;
                main.IsEnable = true;
                main.DisplaySequence = count + 1;
                DbFactory.Default.Add(main);

                foreach (var p in model.Products)
                {
                    ShopHomeModuleProductInfo product = new ShopHomeModuleProductInfo();
                    product.HomeModuleId = main.Id;
                    product.DisplaySequence = p.DisplaySequence;
                    product.ProductId = p.ProductId;
                    DbFactory.Default.Add(product);
                }

                foreach (var t in model.TopImgs)
                {
                    var top = new ShopHomeModuleTopImgInfo();
                    top.HomeModuleId = main.Id;
                    top.ImgPath = t.ImgPath;
                    top.Url = t.Url;
                    top.DisplaySequence = top.DisplaySequence;
                    DbFactory.Default.Add(top);
                }
            }
            else //修改
            {
                DbFactory.Default.Del<ShopHomeModuleProductInfo>(p => p.HomeModuleId == model.Id);
                DbFactory.Default.Del<ShopHomeModuleTopImgInfo>(p => p.HomeModuleId == model.Id);

                var main = DbFactory.Default.Get<ShopHomeModuleInfo>().Where(p => p.Id == model.Id).FirstOrDefault();
                main.Name = model.Name;
                main.Url = model.Url;

                foreach (var p in model.Products)
                {
                    var product = new ShopHomeModuleProductInfo();
                    product.HomeModuleId = main.Id;
                    product.DisplaySequence = p.DisplaySequence;
                    product.ProductId = p.ProductId;
                    DbFactory.Default.Add(product);
                }

                foreach (var t in model.TopImgs)
                {
                    var top = new ShopHomeModuleTopImgInfo();
                    top.HomeModuleId = main.Id;
                    top.ImgPath = t.ImgPath;
                    top.Url = t.Url;
                    top.DisplaySequence = top.DisplaySequence;
                    DbFactory.Default.Add(top);
                }
                DbFactory.Default.Update(main);
            }
        }

        public void Enable(long id, bool enable)
        {
            DbFactory.Default.Set<ShopHomeModuleInfo>().Set(n => n.IsEnable, enable).Where(p => p.Id == id).Succeed();
        }

        public void UpdateFloorSequence(long shopId, int oriRowNumber, int newRowNumber, string direction)
        {
            var source = DbFactory.Default.Get<ShopHomeModuleInfo>().Where(item => item.ShopId == shopId && item.DisplaySequence == oriRowNumber).FirstOrDefault();
            var destination = DbFactory.Default.Get<ShopHomeModuleInfo>().Where(item => item.ShopId == shopId && item.DisplaySequence == newRowNumber).FirstOrDefault();
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Set<ShopHomeModuleInfo>().Set(n => n.DisplaySequence, newRowNumber).Where(p => p.Id == source.Id).Succeed();
                DbFactory.Default.Set<ShopHomeModuleInfo>().Set(n => n.DisplaySequence, oriRowNumber).Where(p => p.Id == destination.Id).Succeed();
            });
        }

        public void DelFloor(long id)
        {
            DbFactory.Default.InTransaction(() =>
            {
                DbFactory.Default.Del<ShopHomeModuleProductInfo>(p => p.HomeModuleId == id);
                DbFactory.Default.Del<ShopHomeModuleTopImgInfo>(p => p.HomeModuleId == id);
                DbFactory.Default.Del<ShopHomeModuleInfo>(n => n.Id == id);
            });
        }
    }
}
