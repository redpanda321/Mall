using System.Collections.Generic;

namespace Mall.DTO
{
    public class TypeAttributesModel
    {
        public long AttrId { get; set; }
        public string Selected { get; set; }
        public bool IsMulti { get; set; }
        public string Name { get; set; }
        public List<TypeAttrValue> AttrValues { get; set; }
    }

    public class TypeAttrValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}