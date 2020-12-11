using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.CommonModel
{
    /// <summary>
    /// 简单数据对象
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class SimpItem<T1>
    {
        public T1 Item1 { get; set; }
    }
    public class SimpItem<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
    }
    public class SimpItem<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
    }
    public class SimpItem<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }
    }
}
