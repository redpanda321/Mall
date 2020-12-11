using Mall.API.Model.ParamsModel;
using Mall.Application;
using Mall.Entities;
using Mall.IServices;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.API
{
    public class ShippingAddressController : BaseApiController
    {

        [HttpGet("GetShippingAddressList")]
        public object GetShippingAddressList(long? shopBranchId = 0, bool isCanDelive = false)
        {
            CheckUserLogin();
            var branchId = shopBranchId.HasValue ? shopBranchId.Value : 0;
            var shoppingAddress = OrderApplication.GetUserAddresses(CurrentUser.Id, branchId);//ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);

            var shippingAddressList = new List<ShippingAddressInfo>();
            var shippingAddressListCanDelive = new List<ShippingAddressInfo>();
            foreach (var item in shoppingAddress)
            {
                ShippingAddressInfo shippingAddress = new ShippingAddressInfo()
                {
                    Id = item.Id,
                    ShipTo = item.ShipTo,
                    Phone = item.Phone,
                    RegionFullName = item.RegionFullName,
                    Address = item.Address,
                    RegionId = item.RegionId,
                    RegionIdPath = item.RegionIdPath,
                    IsDefault = item.IsDefault,
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    AddressDetail = item.AddressDetail,
                    NeedUpdate = item.NeedUpdate
                };
                if (isCanDelive)
                {
                    if (item.NeedUpdate)
                    {
                        shippingAddressListCanDelive.Add(shippingAddress);
                    }
                    else
                    {
                        shippingAddressList.Add(shippingAddress);
                    }
                }
                else
                {
                    if (branchId == 0)
                    {
                        shippingAddressList.Add(shippingAddress);
                    }
                    else
                    {
                        if (item.CanDelive)
                            shippingAddressList.Add(shippingAddress);
                        else
                            shippingAddressListCanDelive.Add(shippingAddress);
                    }
                }
            }
            var addressList = new
            {
                CanDeliveAddressList = shippingAddressList,
                CanNotDeliveAddressList = shippingAddressListCanDelive
            };
            dynamic result = SuccessResult();
            result.ShippingAddress = addressList;
            #region 是否开启门店授权
            var isopenstore = SiteSettingApplication.SiteSettings != null && SiteSettingApplication.SiteSettings.IsOpenStore;
            result.IsOpenStore = isopenstore ? 1 : 0;
            #endregion
            return result;
        }


        [HttpGet("GetShippingAddress")]
        public object GetShippingAddress(long id)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var shopaddressInfo = shoppingAddress.FirstOrDefault(e => e.Id == id);
            if (shopaddressInfo != null)
            {
                var model = new ShippingAddressInfo()
                {
                    Id = shopaddressInfo.Id,
                    ShipTo = shopaddressInfo.ShipTo,
                    Phone = shopaddressInfo.Phone,
                    RegionFullName = shopaddressInfo.RegionFullName,
                    Address = shopaddressInfo.Address,
                    RegionId = shopaddressInfo.RegionId,
                    RegionIdPath = shopaddressInfo.RegionIdPath,
                    Latitude = shopaddressInfo.Latitude,
                    Longitude = shopaddressInfo.Longitude,
                    AddressDetail = shopaddressInfo.AddressDetail == null ? string.Empty : shopaddressInfo.AddressDetail
                };
                dynamic result = SuccessResult();
                result.ShippingAddress = model;
                return result;
            }
            else
            {
                dynamic result = SuccessResult();
                result.ShippingAddress = new ShippingAddressInfo();
                return result;
            }

        }
        //新增收货地址

        [HttpPost("PostAddShippingAddress")]
        public object PostAddShippingAddress(ShippingAddressAddModel value)
        {
            CheckUserLogin();
            Entities.ShippingAddressInfo shippingAddr = new Entities.ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.phone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.latitude;
            shippingAddr.Longitude = value.longitude;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
            return SuccessResult();
        }
        //删除收货地址

        [HttpPost("PostDeleteShippingAddress")]
        public object PostDeleteShippingAddress(ShippingAddressDeleteModel value)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.DeleteShippingAddress(value.id, CurrentUser.Id);
            return SuccessResult();
        }
        //编辑收货地址

        [HttpPost("PostEditShippingAddress")]
        public object PostEditShippingAddress(ShippingAddressEditModel value)
        {
            CheckUserLogin();
            Entities.ShippingAddressInfo shippingAddr = new Entities.ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.Id = value.id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.phone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Longitude = value.longitude;
            shippingAddr.Latitude = value.latitude;
            shippingAddr.AddressDetail = value.addressDetail;
            ServiceProvider.Instance<IShippingAddressService>.Create.UpdateShippingAddress(shippingAddr);
            return SuccessResult();
        }
        //设为默认收货地址

        [HttpPost("PostSetDefaultAddress")]
        public object PostSetDefaultAddress(ShippingAddressSetDefaultModel value)
        {
            CheckUserLogin();
            long addId = value.addId;
            ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(addId, CurrentUser.Id);
            return SuccessResult();
        }

        /// <summary>
        /// 根据搜索地址反向匹配出区域信息
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <returns></returns>
        /// 

        [HttpGet("GetRegion")]
        public object GetRegion(string fromLatLng = "")
        {
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, fullPath = string.Empty, newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            fullPath = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            dynamic result = SuccessResult();
            result.fullPath = fullPath;
            result.showCity = string.Format("{0} {1} {2} {3}", province, city, district, newStreet);
            result.street = newStreet;
            return result;
        }
    }
}
