using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Core.Plugins.Payment;
using Mall.IServices;
using Mall.DTO;
using NetRube.Data;
using Mall.Entities;

namespace Mall.Service
{
    public class PaymentConfigService : ServiceBase, IPaymentConfigService
    {

        public bool IsEnable()
        {
            return DbFactory.Default.Get<PaymentConfigInfo>().Select(n => n.IsCashOnDelivery).FirstOrDefault<bool>();
        }

        public void Enable()
        {
            DbFactory.Default.Set<PaymentConfigInfo>().Set(n => n.IsCashOnDelivery, true).Succeed();
        }

        public void Disable()
        {
            DbFactory.Default.Set<PaymentConfigInfo>().Set(n => n.IsCashOnDelivery, false).Succeed();
        }


        public List<PaymentType> GetPaymentTypes()
        {
            var payPlugins = Core.PluginsManagement.GetPlugins<IPaymentPlugin>().Select(t => t.PluginInfo);
            List<PaymentType> result = new List<PaymentType>();
            foreach (var p in payPlugins)
            {
                result.Add(new PaymentType(p.PluginId, p.DisplayName));
            }
            return result;
        }


        public void Save(string addressIds, string addressids_city, long shopid)
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (model != null)
            {
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
                DbFactory.Default.Update(model);
            }
            else
            {
                model = new ReceivingAddressConfigInfo();
                model.ShopId = shopid;
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
                DbFactory.Default.Add(model);
            }
        }

        public List<string> GetAddressIdByShop(long shopid)
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.AddressId))
                {
                    return model.AddressId.Split(',').ToList();
                }
            }
            return new List<string>();
        }
        

        public ReceivingAddressConfigInfo Get(long shopid)
        {
            return DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
        }

        public List<string> GetAddressIdCityByShop(long shopid)
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.AddressId_City))
                {
                    return model.AddressId_City.Split(',').ToList();
                }
            }
            return new List<string>();
        }

        public bool IsCashOnDelivery(long cityId, long countyId)
        {
            var model = DbFactory.Default.Get<PaymentConfigInfo>().FirstOrDefault();
            if (model != null && model.IsCashOnDelivery)
            {
                return DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.AddressId.Contains("'" + countyId + "'") || p.AddressId_City.Contains("'" + cityId + "'")).Exist();
            }
            return false;
        }

        public string GetAddressIds(long shopid)
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().Where(p => p.ShopId == shopid).FirstOrDefault();
            if (model != null)
            {
                return model.AddressId;
            }
            return "";
        }

        public List<string> GetAddressId()
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().FirstOrDefault();
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.AddressId))
                {
                    return model.AddressId.Split(',').ToList();
                }
            }
            return new List<string>();
        }

        public List<string> GetAddressIdCity()
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().FirstOrDefault();
            if (model != null)
            {
                if (!string.IsNullOrEmpty(model.AddressId_City))
                {
                    return model.AddressId_City.Split(',').ToList();
                }
            }
            return new List<string>();
        }

        public void Save(string addressIds, string addressids_city)
        {
            var model = DbFactory.Default.Get<ReceivingAddressConfigInfo>().FirstOrDefault();
            if (model != null)
            {
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
                DbFactory.Default.Update(model);
            }
            else
            {
                model = new ReceivingAddressConfigInfo();
                model.ShopId = 0;
                model.AddressId = addressIds;
                model.AddressId_City = addressids_city;
                DbFactory.Default.Add(model);
            }
        }
    }
}
