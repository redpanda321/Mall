using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mall.Entities;
namespace Mall.DTO
{
    public class ShopBonus
    {
        public ShopBonusReceiveInfo Receive { get; set; }
        public ShopBonusInfo Bonus { get; set; }
        public ShopBonusGrantInfo Grant { get; set; }
        public ShopInfo Shop { get; set; }
        public VShopInfo VShop { get; set; }
    }
}
