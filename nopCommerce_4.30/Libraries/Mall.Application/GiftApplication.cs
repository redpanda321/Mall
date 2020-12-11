using Mall.CommonModel;
using Mall.DTO;
using Mall.DTO.QueryModel;
using Mall.Entities;
using Mall.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Application
{
    public class GiftApplication:BaseApplicaion<IGiftService>
    {
        public static GiftInfo GetGift(long id)
        {
            return Service.GetById(id);
        }

        public static IntegralMallAdInfo GetAdInfo(IntegralMallAdInfo.AdActivityType adtype, IntegralMallAdInfo.AdShowPlatform adplatform)
        {
            return Service.GetAdInfo(adtype, adplatform);
        }
        public static QueryPageModel<GiftModel> GetGifts(GiftQuery query)
        {
            return Service.GetGifts(query);
        }
        public static GiftInfo GetById(long id)
        {
            return Service.GetById(id);
        }
    }
}
