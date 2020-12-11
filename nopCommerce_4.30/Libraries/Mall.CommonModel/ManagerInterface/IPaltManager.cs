using System.Collections.Generic;

namespace Mall.CommonModel
{
    public interface IPaltManager:IManager
    {
        List<AdminPrivilege> AdminPrivileges { set; get; }
    }
}
