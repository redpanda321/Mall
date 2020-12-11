using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO;
using Mall.IServices;
using Mall.SmallProgAPI.Model;
using Mall.Web.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;


namespace Mall.SmallProgAPI
{
    /// <summary>
    /// 收货地址
    /// </summary>
    public class ShippingAddressController : BaseApiController
    {


        [HttpGet("GetShippingAddressList")]
        public object GetShippingAddressList(string openId, long? shopBranchId = 0, bool isCanDelive = false)
        {
            CheckUserLogin();
            var branchId = shopBranchId.HasValue ? shopBranchId.Value : 0;
            var shoppingAddress = OrderApplication.GetUserAddresses(CurrentUser.Id, branchId);

            var shippingAddressList = new List<ShippingAddress>();
            var shippingAddressListCanDelive = new List<ShippingAddress>();
            foreach (var item in shoppingAddress)
            {
                ShippingAddress shippingAddress = new ShippingAddress()
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
                    AddressDetail = item.AddressDetail ?? "",
                    UserId = item.UserId,
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
            dynamic result = Json(addressList);
            return result;
        }
        /// <summary>
        /// 获取收货地址列表
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetList")]
        public object GetList(string openId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);

            var result = shoppingAddress.ToList().Select(item => new
            {
                ShippingId = item.Id,
                ShipTo = item.ShipTo,
                CellPhone = item.Phone,
                FullRegionName = item.RegionFullName,
                Address = item.Address,
                RegionId = item.RegionId,
                FullRegionPath = item.RegionIdPath,
                IsDefault = item.IsDefault,
                LatLng = item.Latitude + "," + item.Longitude,
                FullAddress = item.RegionFullName + " " + item.Address + " " + item.AddressDetail,
                TelPhone = "",
                RegionLocation = "",
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                AddressDetail = item.AddressDetail ?? string.Empty,
                NeedUpdate = item.NeedUpdate
            });
            return Json(result);
        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetShippingAddress")]
        public object GetShippingAddress(long shippingId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var shopaddressInfo = shoppingAddress.FirstOrDefault(e => e.Id == shippingId);
            if (shopaddressInfo != null)
            {
                dynamic model = new ExpandoObject();

                model.ShippingId = shopaddressInfo.Id;
                model.ShipTo = shopaddressInfo.ShipTo;
                model.CellPhone = shopaddressInfo.Phone;
                model.FullRegionName = shopaddressInfo.RegionFullName;
                model.Address = shopaddressInfo.Address;
                model.RegionId = shopaddressInfo.RegionId;
                model.FullRegionPath = shopaddressInfo.RegionIdPath;
                model.IsDefault = shopaddressInfo.IsDefault;
                model.LatLng = shopaddressInfo.Latitude + "," + shopaddressInfo.Longitude;
                model.FullAddress = shopaddressInfo.RegionFullName + " " + shopaddressInfo.Address;
                model.TelPhone = "";
                model.RegionLocation = "";
                model.UserId = CurrentUserId;
                model.Zipcode = "";
                model.AddressDetail = shopaddressInfo.AddressDetail;
                return Json(model);
            }
            else
            {
                return Json(ErrorResult<dynamic>("参数错误"));
            }

        }

        /// <summary>
        /// 新增收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostAddAddress")]
        public object PostAddAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            Entities.ShippingAddressInfo shippingAddr = new Entities.ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return Json(shippingAddr.Id);
        }
        /// <summary>
        /// 新增收货地址(微信地址)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostAddWXAddress")]
        public object PostAddWXAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            try
            {
                if (string.IsNullOrWhiteSpace(value.address))
                {
                    throw new MallException("请填写详细地址");
                }
                if (value.regionId <= 0 && (string.IsNullOrWhiteSpace(value.city) || string.IsNullOrWhiteSpace(value.county)))
                {
                    throw new MallException("参数错误");
                }
                if (value.regionId <= 0)
                {
                    var _region = ServiceProvider.Instance<IRegionService>.Create.GetRegionByName(value.county, Region.RegionLevel.County);
                    if (_region != null)
                    {
                        value.regionId = _region.Id;
                    }
                }
                if (value.regionId <= 0)
                {
                    throw new MallException("错误的地区信息");
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }

            #region 如果存在相同地址就返回
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var _tmp = shoppingAddress.FirstOrDefault(d => d.RegionId == value.regionId && d.Address == value.address);
            if (_tmp != null)
            {
                return Json(_tmp.Id);
            }
            #endregion

            Entities.ShippingAddressInfo shippingAddr = new Entities.ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return Json(shippingAddr.Id);
        }
        /// <summary>
        /// 修改收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpPost("PostUpdateAddress")]
        public object PostUpdateAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            Entities.ShippingAddressInfo shippingAddr = new Entities.ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.Id = value.shippingId;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.UpdateShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return Json(shippingAddr.Id);
        }
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetSetDefault")]
        public object GetSetDefault(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingId, CurrentUserId);
            return Json(new { msg = "设置成功" });
        }
        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetDeleteAddress")]
        public object GetDeleteAddress(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.DeleteShippingAddress(shippingId, CurrentUser.Id);
            return Json(new { msg = "删除成功" });
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
            CheckUserLogin();
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, fullPath = string.Empty, newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            fullPath = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            return Json(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2} {3}", province, city, district, newStreet), street = newStreet });
        }

        /// <summary>
        /// 获取省市区信息
        /// </summary>
        /// <param name="parentRegionId"></param>
        /// <returns></returns>
        /// 
        [HttpGet("GetAll")]
        public object GetAll(long? parentRegionId = null)
        {
            var regions = RegionApplication.GetSubRegion(0, true);
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
            var region = RegionApplication.GetRegion(parentId);
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
        [HttpGet("GetShopGetAddress")]
        public object GetShopGetAddress(long shopId, long shopBranchId = 0)
        {
            if (shopBranchId <= 0)
            {
                var data = ShopShippersApplication.GetDefaultGetGoodsShipper(shopId);
                if (data == null)
                {
                    data = new DTO.ShopShipper() { };
                }
                else
                {
                    data.RegionStr = RegionApplication.GetFullName(data.RegionId);
                }
                var model = new
                {
                    Region = data.RegionStr,
                    Address = data.Address,
                    Phone = data.TelPhone,
                    ShipperName = data.ShipperName
                };
                return Json(model);
            }
            else
            {
                var data = ShopBranchApplication.GetShopBranchById(shopBranchId);
                string redionstr = "";
                if (data != null)
                {
                    redionstr = RegionApplication.GetFullName(data.AddressId);
                }
                var model = new
                {
                    Region = redionstr,
                    Address = data.AddressDetail,
                    Phone = data.ContactPhone,
                    ShipperName = data.ContactUser
                };
                return Json(model);
            }
        }
    }
}
