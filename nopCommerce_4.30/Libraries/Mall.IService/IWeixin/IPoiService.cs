using Mall.DTO;
using Senparc.Weixin.MP.AdvancedAPIs.Poi;
using System.Collections.Generic;

namespace Mall.IServices
{
    public interface IPoiService : IService 
    {
        void init( string appid , string secret );

        bool AddPoi( CreateStoreData createStoreData );

        bool UpdatePoi( UpdateStoreData updateStoreData );

        bool DeletePoi( string poiId );

        string UploadImage( string filePath );

        GetStoreListResultJson GetPoiList( int page , int rows );

        List<GetStoreList_BaseInfo> GetPoiList();

        GetStoreBaseInfo GetPoi( string poiId );

        List<WXCategory> GetCategory();

    }
}
