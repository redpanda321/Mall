
using NPoco;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mall.Entities
{
    public partial class BusinessCategoryInfo
    {
        [ResultColumn]
        public string CategoryName { get; set; }
    }
}
