using Mall.Entities;

namespace Mall.DTO
{
    public class MemberModel : MemberInfo
    {
        public string StrLastLoginDate { get { return LastLoginDate.ToString("yyyy-MM-dd HH:mm"); } }

        public string StrCreateDate
        {
            get
            {
                return CreateDate.ToString( "yyyy-MM-dd HH:mm" );
            }
        }
    }
}