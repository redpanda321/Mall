namespace Mall.Web.Areas.Admin.Models
{
    public class HomeFloor
    {
        public long StyleLevel
        {
            get;
            set;
        }

        public string Name { get; set; }

        public long DisplaySequence { get; set; }

        public bool Enable { get; set; }


        public long Id { get; set; }
    }
}