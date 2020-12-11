using Mall.Entities;
using NetRube.Data;
using System;
using System.Collections.Generic;

namespace Mall.Service.Market.Business
{
    class FixedGeneration : IGenerateDetail
    {
        private readonly decimal _fixedAmount = 0;

        public FixedGeneration(decimal fixedAmount)
        {
            this._fixedAmount = fixedAmount;
        }

        public void Generate(long bounsId, decimal totalPrice)
        {
            try
            {
                DbFactory.Default
                    .InTransaction(() =>
                    {
                        var flag = true;
                        //红包个数
                        int detailCount = (int)(totalPrice / this._fixedAmount);
                        List<BonusReceiveInfo> list = new List<BonusReceiveInfo>();
                        //生成固定数量个红包
                        for (int i = 0; i < detailCount; i++)
                        {
                            BonusReceiveInfo detail = new BonusReceiveInfo
                            {
                                BonusId = bounsId,
                                Price = this._fixedAmount,
                                IsShare = false,
                                OpenId = null
                            };
                            list.Add(detail);
                            if (list.Count >= 500)  //500个一次批量提交
                            {
                                flag = DbFactory.Default.Add<BonusReceiveInfo>(list);
                                list.Clear();
                                if (!flag) return false;
                            }
                        }
                        flag = DbFactory.Default.Add<BonusReceiveInfo>(list);
                        return flag;
                    });
            }
            catch (Exception ex)
            {
                Core.Log.Error(ex.Message, ex);
            }
        }
    }
}
