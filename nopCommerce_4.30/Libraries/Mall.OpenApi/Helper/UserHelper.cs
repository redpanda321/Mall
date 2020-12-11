using Mall.Application;
using Mall.CommonModel;
using Mall.Core;
using Mall.DTO.QueryModel;
using Mall.IServices;
using Mall.OpenApi.Model.DTO;
using Mall.Web.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mall.OpenApi
{
    /// <summary>
    /// 会员接口
    /// <para>仅供官方自营店使用</para>
    /// </summary>
    public class UserHelper : _BaseHelper
    {
        private IMemberService _iMemberService;
        private IMemberIntegralService _iMemberIntegralService;
        private IOrderService _iOrderService;

        public UserHelper()
        {
            _iMemberService = Mall.ServiceProvider.Instance<IMemberService>.Create;
            _iMemberIntegralService = Mall.ServiceProvider.Instance<IMemberIntegralService>.Create;
            _iOrderService = Mall.ServiceProvider.Instance<IOrderService>.Create;
        }

        /// <summary>
        /// 获取用户列表,根据时间段获取这段时间内注册的用户
        /// </summary>
        /// <param name="app_key"></param>
        /// <param name="start_time"></param>
        /// <param name="end_time"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public QueryPageModel<user_list_model> GetUsers(DateTime? start_time, DateTime? end_time, int page_no, int page_size)
        {
            //InitShopInfo(app_key);

            QueryPageModel<user_list_model> result = new QueryPageModel<user_list_model>()
            {
                Models = null,
                Total = 0
            };
            List<user_list_model> resultdata = new List<user_list_model>();

            #region 构建查询条件
            MemberQuery userque = new MemberQuery()
            {
                PageSize = page_size,
                PageNo = page_no,
            };
            if (start_time.HasValue)
            {
                userque.RegistTimeStart = start_time.Value;
            }
            if (end_time.HasValue)
            {
                userque.RegistTimeEnd = end_time.Value;
            }
            #endregion

            //获取数据
            var datalist = _iMemberService.GetMembers(userque);
            //转换数据
            result.Total = datalist.Total;
            if (datalist.Total > 0)
            {
                foreach (var item in datalist.Models)
                {
                    resultdata.Add(UserMemberInfoMapDTO(item));
                }
                result.Models = resultdata;
            }
            return result;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public user_list_model GetUser(string user_name)
        {
            var data = _iMemberService.GetMemberByName(user_name);
            return UserMemberInfoMapDTO(data);
        }
        /// <summary>
        /// 添加会员
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="password"></param>
        /// <param name="created"></param>
        /// <param name="real_name"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="sex"></param>
        /// <param name="birthday"></param>
        /// <param name="state"></param>
        /// <param name="city"></param>
        /// <param name="district"></param>
        /// <param name="town"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        public long AddUser(string user_name, string password, DateTime created, string real_name, string mobile, string email, string sex, DateTime? birthday, string state, string city, string district, string town, string address)
        {
            long result = 0;
            var user = _iMemberService.Register(user_name, password, (int)PlatformType.PC, mobile, email, 0);
            user.Sex = SexType.Male;
            if (sex == "女")
            {
                user.Sex = SexType.Female;
            }
            user.RealName = real_name;
            user.CreateDate = created;
            user.BirthDay = birthday;
            user.Address = address;
            if (!string.IsNullOrWhiteSpace(district))
            {
                var dreg = _iRegionService.GetRegionByName(district);
                if (dreg != null)
                {
                    user.RegionId = dreg.Id;
                    user.TopRegionId = dreg.Parent.Parent.Id;
                    if (!string.IsNullOrWhiteSpace(town))
                    {
                        var treg = dreg.Sub.FirstOrDefault(p => p.Name.Contains(town) || town.Contains(p.Name));
                        if (treg != null)
                        {
                            user.RegionId = treg.Id;
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(city))
                {
                    var creg = _iRegionService.GetRegionByName(city);
                    if (creg != null)
                    {
                        user.RegionId = creg.Id;
                        user.TopRegionId = creg.Parent.Id;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        var sreg = _iRegionService.GetRegionByName(state);
                        if (sreg != null)
                        {
                            user.RegionId = sreg.Id;
                            user.TopRegionId = sreg.Id;
                        }
                    }

                }
                if (!string.IsNullOrWhiteSpace(town))
                {
                    user.Address = town + " " + address;
                }
            }
            _iMemberService.UpdateMemberInfo(user);
            int num = CouponApplication.RegisterSendCoupon(user.Id, user.UserName);
            result = user.Id;
            return result;
        }

        #region 私有
        private user_list_model UserMemberInfoMapDTO(Entities.MemberInfo data)
        {
            user_list_model result = null;
            if (data != null)
            {
                result = new user_list_model();
                var ui = MemberIntegralApplication.GetMemberIntegral(data.Id);
                if (ui == null)
                {
                    ui = new Mall.Entities.MemberIntegralInfo
                    {
                        MemberId = data.Id,
                        AvailableIntegrals = 0,
                        HistoryIntegrals = 0
                    };
                }
                result.uid = (int)data.Id;
                result.user_name = data.UserName;
                result.created = data.CreateDate;
                result.real_name = data.RealName;
                result.mobile = data.CellPhone;
                result.email = data.Email;
                result.avatar = Mall.Core.MallIO.GetRomoteImagePath(data.Photo);
                result.sex = data.Sex.ToDescription();
                result.birthday = data.BirthDay;
                result.state = "";
                result.city = "";
                result.district = "";
                result.town = "";
                result.address = "";
                result.points = ui.AvailableIntegrals;
                result.trade_count = data.OrderNumber;
                result.trade_amount = data.NetAmount;
            }
            return result;
        }
        #endregion
    }
}
