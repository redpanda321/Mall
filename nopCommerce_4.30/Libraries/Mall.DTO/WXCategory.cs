using System.Collections.Generic;

namespace Mall.DTO
{
    public class WXCategory
    {
        public WXCategory()
        {
            Child = new List<WXCategory>();
        }

        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public List<WXCategory> Child
        {
            get;
            set;
        }
    }
}
