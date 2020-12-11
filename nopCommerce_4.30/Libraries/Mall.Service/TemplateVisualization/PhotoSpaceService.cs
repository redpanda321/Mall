using Mall.CommonModel;
using Mall.Entities;
using Mall.IServices;
using NetRube.Data;
using System;
using System.Collections.Generic;

namespace Mall.Service
{
    public class PhotoSpaceService : ServiceBase, IPhotoSpaceService
    {

        public PhotoSpaceCategoryInfo AddPhotoCategory(string name, long shopId = 0)
        {
            var photoSpace = new PhotoSpaceCategoryInfo
            {
                PhotoSpaceCatrgoryName = name,
                ShopId = shopId,
                DisplaySequence = DbFactory.Default.Get<PhotoSpaceCategoryInfo>().Max<long>(n => n.DisplaySequence) + 1
            };
            var flag = DbFactory.Default.Add(photoSpace);
            return photoSpace;
        }

        public void MovePhotoType(List<long> pList, int pTypeId, long shopId = 0)
        {
            //var oldPhotos = DbFactory.Default.Get<PhotoSpaceInfo>().Where(p => p.Id.ExIn(pList) && p.ShopId == shopId).ToList();
            //DbFactory.Default.InTransaction(() =>
            //{
            //    foreach (var item in oldPhotos)
            //    {
            //        item.PhotoCategoryId = pTypeId;
            //        item.LastUpdateTime = DateTime.Now;
            //        DbFactory.Default.Update(item);
            //    }
            //});
            DbFactory.Default.Set<PhotoSpaceInfo>().Set(n => n.PhotoCategoryId, pTypeId).Set(n => n.LastUpdateTime, DateTime.Now).Where(p => p.Id.ExIn(pList) && p.ShopId == shopId).Succeed();
        }

        public void UpdatePhotoCategories(Dictionary<long, string> photoCategorys, long shopId = 0)
        {
            DbFactory.Default.InTransaction(() =>
            {
                foreach (var item in photoCategorys)
                {
                    var cate = DbFactory.Default.Get<PhotoSpaceCategoryInfo>().Where(p => p.Id.Equals(item.Key) && p.ShopId == shopId).FirstOrDefault();
                    if (null != cate)
                    {
                        cate.PhotoSpaceCatrgoryName = item.Value;
                        DbFactory.Default.Update(cate);
                    }
                }
            });
        }

        public void DeletePhotoCategory(long categoryId, long shopId = 0)
        {
            //var photoCate = Context.PhotoSpaceCategoryInfo.FirstOrDefault(p => p.Id.Equals(categoryId) && p.ShopId.Equals(shopId));
            //if (null != photoCate)
            //{
            //    Context.PhotoSpaceCategoryInfo.Remove(photoCate);
            //}
            DbFactory.Default.Del<PhotoSpaceCategoryInfo>().Where(n => n.Id == categoryId && n.ShopId == shopId).Succeed();
        }

        public List<PhotoSpaceCategoryInfo> GetPhotoCategories(long shopId = 0)
        {
            return DbFactory.Default.Get<PhotoSpaceCategoryInfo>().Where(p => p.ShopId == shopId).ToList();
        }

        public QueryPageModel<PhotoSpaceInfo> GetPhotoList(string keyword, int pageIndex, int pageSize, int order, long categoryId = 0, long shopId = 0)
        {
            var complaints = DbFactory.Default.Get<PhotoSpaceInfo>();

            #region 条件组合

            complaints.Where(item => shopId == item.ShopId);
            if (categoryId != 0)
            {
                complaints.Where(item => item.PhotoCategoryId == categoryId);
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                complaints.Where(item => item.PhotoName.Contains(keyword));
            }
            #endregion

            var rets = complaints.OrderByDescending(o => o.UploadTime).ToPagedList(pageIndex, pageSize);

            var pageModel = new QueryPageModel<PhotoSpaceInfo>() { Models = rets, Total = rets.TotalRecordCount };
            return pageModel;
        }

        public void AddPhote(long categoryId, string photoName, string photoPath, int fileSize, long shopId = 0)
        {
            DbFactory.Default.Add(new PhotoSpaceInfo
            {
                FileSize = fileSize,
                LastUpdateTime = DateTime.Now,
                UploadTime = DateTime.Now,
                PhotoCategoryId = categoryId,
                PhotoName = photoName,
                PhotoPath = photoPath,
                ShopId = shopId
            });
        }

        public void DeletePhoto(long photoId, long shopId = 0)
        {
            DbFactory.Default.Del<PhotoSpaceInfo>().Where(p => p.Id.Equals(photoId) && p.ShopId.Equals(shopId)).Succeed();
        }

        public void RenamePhoto(long photoId, string newName, long shopId = 0)
        {
            //var photo = DbFactory.Default.Get<PhotoSpaceInfo>().Where(p => p.Id.Equals(photoId) && p.ShopId.Equals(shopId)).FirstOrDefault();
            //if (null != photo)
            //{
            //    photo.PhotoName = newName;
            //    DbFactory.Default.Update(photo);
            //}
            DbFactory.Default.Set<PhotoSpaceInfo>().Set(n => n.PhotoName, newName).Where(p => p.Id.Equals(photoId) && p.ShopId.Equals(shopId)).Succeed();
        }

        public string GetPhotoPath(long photoId, long shopId = 0)
        {
            throw new NotImplementedException();
        }

        public int GetPhotoCount(long shopId = 0)
        {
            throw new NotImplementedException();
        }

        public int GetDefaultPhotoCount(long shopId = 0)
        {
            throw new NotImplementedException();
        }
    }
}
