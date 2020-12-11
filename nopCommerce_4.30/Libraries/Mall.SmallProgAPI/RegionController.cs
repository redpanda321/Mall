using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace Mall.SmallProgAPI
{
    /// <summary>
    /// 地区
    /// </summary>
    public class RegionController : BaseApiController
    {
        private IRegionService iRegionService;
        public RegionController()
        {
            iRegionService = ServiceProvider.Instance<IRegionService>.Create;
        }
        /// <summary>
        /// 获取所有子级地址
        /// </summary>
        /// <param name="parentRegionId">此参数无实际意义，仅为了兼容云商城挖的坑，在云商城系统里此参数也未参与任务实际业务</param>
        /// <returns></returns>
        /// 
        [HttpGet("GetAll")]
        public object GetAll(long? parentRegionId = null)
        {
            var regions = iRegionService.GetSubs(0, true);
            var models = regions.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                city = (p.Sub != null ? p.Sub.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    area = (c.Sub != null ? c.Sub.Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        country = (a.Sub != null ? a.Sub.Select(v => new
                        {
                            id = v.Id,
                            name = v.Name
                        }) : null)
                    }) : null)
                }) : null)
            });

            return Json(models);
        }
        /// <summary>
        /// 获取直属子级
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetSub")]
        public object GetSub(long parentId)
        {
            var region = iRegionService.GetRegion(parentId);
            if (region == null)
            {
                return Json(ErrorResult<dynamic>(msg: "错误的参数：parentId"));
            }
            var models = region.Sub.Select(p => new
            {
                id = p.Id,
                name = p.Name,
            }).ToList();

            return Json(new
            {
                Depth = region.Level.GetHashCode(),
                Regions = models
            });
        }
    }
}
