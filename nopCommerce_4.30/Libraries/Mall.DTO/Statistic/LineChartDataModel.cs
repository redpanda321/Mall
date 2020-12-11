using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mall.DTO
{
    public class LineChartDataModel<T> where T : struct
    {
        /// <summary>
        /// 图表中X轴显示的标题序列
        /// </summary>
        public string[] XAxisData { get; set; }

        /// <summary>
        /// 需要显示的数据，可能有多个比较数据集
        /// </summary>
        public List<ChartSeries<T>> SeriesData { get; set; } = new List<ChartSeries<T>>();

        /// <summary>
        /// 通用的扩展属性
        /// </summary>
        public string[] ExpandProp { get; set; }
    }
    public class ChartSeries<T> 
    {
        /// <summary>
        /// 该数据集的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 需要显示的数据序列
        /// </summary>
        public T[] Data { get; set; }
    }

    public class MapChartDataModel<T>
    {
        public T MaxValue { get; set; }
        public T MinValue { get; set; }
        public T TotalValue { get; set; }
        public List<MapChartDataItem<T>> Data { get; set; }
    }

    public class MapChartDataItem<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }

        public string Expand { get; set; }
    }


    public class ChartDataItem<TKey, TValue>
    {
        public TKey ItemKey { get; set; }
        public TValue ItemValue { get; set; }
        public string Expand { get; set; }
    }
}
