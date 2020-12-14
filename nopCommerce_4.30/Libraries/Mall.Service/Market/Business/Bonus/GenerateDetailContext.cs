﻿using Mall.Core;
using Mall.Entities;

namespace Mall.Service.Market.Business
{
    public class GenerateDetailContext
    {
        private IGenerateDetail _generate = null;
        private BonusInfo _bonus = null;

        public GenerateDetailContext(BonusInfo model)
        {
            this._bonus = model;
            switch (this._bonus.PriceType)
            {
                case BonusInfo.BonusPriceType.Fixed:
                    this._generate = new FixedGeneration((decimal)model.FixedAmount);
                    break;

                case BonusInfo.BonusPriceType.Random:
                    this._generate = new RandomlyGeneration((decimal)model.RandomAmountStart, (decimal)model.RandomAmountEnd);
                    break;
            }
        }

        public void Generate()
        {
            if (this._generate != null)
            {
                this._generate.Generate(this._bonus.Id, this._bonus.TotalPrice);
            }
            else
            {
                throw new MallException("生成红包详情策略构造异常");
            }
        }
    }
}