using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.DTO;
using Mall.Entities;

namespace Mall.IServices
{
    public interface IPaymentConfigService : IService
    {
        /// <summary>
        /// 是否开启
        /// </summary>
        bool IsEnable();

        /// <summary>
        /// 开启
        /// </summary>
        void Enable();

        /// <summary>
        /// 关闭
        /// </summary>
        void Disable();


        /// <summary>
        /// 保存商家的配置
        /// addressIds = "id,id,id,id....."
        /// </summary>
        void Save( string addressIds , string addressids_city , long shopid );

        ReceivingAddressConfigInfo Get( long shopid );

        List<string> GetAddressIdByShop( long shopid );
        List<string> GetAddressIdCityByShop( long shopid ); 

        string GetAddressIds( long shopid );
        /// <summary>
        /// 
        /// </summary>
        /// <param name="countyId">区级ID</param>
        /// <param name="cityId">市级ID</param>
        /// <returns></returns>
        bool IsCashOnDelivery(long cityId, long countyId);

        List<PaymentType> GetPaymentTypes();

        List<string> GetAddressId();

        List<string> GetAddressIdCity();

        void Save(string addressIds, string addressids_city);
    }
}
