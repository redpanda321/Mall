using System;
using System.ComponentModel;
namespace Mall.Core
{
    public enum FileCreateType
    {
        [Description("创建新文件")]
        CreateNew = 1,
        [Description("覆盖原文件")]
        Create=2
    }
}
