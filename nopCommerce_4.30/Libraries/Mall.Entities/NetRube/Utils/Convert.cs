using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;

namespace NetRube
{
	public static partial class Utils
	{
		// 扩展方式转换操作

		#region ToBool_
		/// <summary>将不为 '0' 的字符转换为 true</summary>
		/// <param name="c">要转换的字符，'0' 为 false，其余为 true</param>
		/// <returns>'0' 为 false，其余为 true</returns>
		public static bool ToBool_(this char c)
		{
			return !(c == '0' || c == '\0');
		}

		private static string[] FalseStrings = new string[] { "false", "no", "not", "null", "0" };
		/// <summary>将不为空值、null 值、"false"字符串的内容转换为 true</summary>
		/// <param name="str">要转换的字符串，空值、null 值、"false" 为 false，其余为 true</param>
		/// <returns>空值、null 值、"false" 为 false，其余为 true</returns>
		public static bool ToBool_(this string str)
		{
			if(string.IsNullOrEmpty(str)) return false;
			if(str.In_(FalseStrings)) return false;
			if(str.IsNumber_()) return str.ToDouble_().ToBool_();
			return true;
		}

		/// <summary>将大于 0 的数字或不为空值、null 值、"false"字符串的内容转换为 true</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>大于 0 的数字或不为空值、null 值、"false"字符串的内容转换为 true</returns>
		public static bool ToBool_<T>(this T obj)
		{
			if(null == obj) return false;
			try { return Convert.ToBoolean(obj); }
			catch { return obj.ToString().ToBool_(); }
		}
		#endregion

		#region ToChar_
		/// <summary>转换为相应的字符</summary>
		/// <param name="b">要转换的布尔值，true 为 1，false 为 0</param>
		/// <returns>true 为 1，false 为 0</returns>
		public static char ToChar_(this bool b)
		{
			return b ? '1' : '0';
		}

		/// <summary>转换为相应的字符</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>字符串的第一个字符或默认值</returns>
		public static char ToChar_(this string str, char defval = '0')
		{
			return str.IsNullOrEmpty_() ? defval : str[0];
		}

		/// <summary>转换为相应的字符</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的字符或默认值</returns>
		public static char ToChar_<T>(this T obj, char defval = '0')
		{
			if(null == obj) return defval;
			try { return Convert.ToChar(obj); }
			catch { return defval; }
		}

		/// <summary>转换为相应的字符，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的字符或 null</returns>
		public static char? ToCharOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_())
				return null;
			return str[0];
		}

		/// <summary>转换为相应的字符，转换不成功时返回 null</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的字符或 null</returns>
		public static char? ToCharOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToChar(obj); }
			catch { return null; }
		}
		#endregion

		#region ToByte_
		/// <summary>转换为相应的整数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static byte ToByte_(this string str, byte defval = 0)
		{
			if(str.IsNullOrEmpty_()) return defval;
			byte _retval;
			if(!byte.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的整数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static byte ToByte_<T>(this T obj, byte defval = 0)
		{
			if(null == obj) return defval;
			try { return Convert.ToByte(obj); }
			catch { return obj.ToString().ToByte_(defval); }
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的整数或 null</returns>
		public static byte? ToByteOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			byte _retval;
			if(!byte.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的整数或 null</returns>
		public static byte? ToByteOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToByte(obj); }
			catch { return obj.ToString().ToByteOrNull_(); }
		}
		#endregion

		#region ToInt_
		/// <summary>转换为相应的整数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static int ToInt_(this string str, int defval = 0)
		{
			if(str.IsNullOrEmpty_()) return defval;
			int _retval;
			if(!int.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的整数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static int ToInt_<T>(this T obj, int defval = 0)
		{
			if(null == obj) return defval;
			try { return Convert.ToInt32(obj); }
			catch { return obj.ToString().ToInt_(defval); }
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的整数，转换不成功时返回 null</returns>
		public static int? ToIntOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			int _retval;
			if(!int.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的整数或 null</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">要转换的字符串</param>
		/// <returns>相应的整数或 null</returns>
		public static int? ToIntOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToInt32(obj); }
			catch { return obj.ToString().ToIntOrNull_(); }
		}
		#endregion

		#region ToShort_
		/// <summary>转换为相应的整数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static short ToShort_(this string str, short defval = 0)
		{
			if(str.IsNullOrEmpty_()) return defval;
			short _retval;
			if(!short.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的整数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static short ToShort_<T>(this T obj, short defval = 0)
		{
			if(null == obj) return defval;
			try { return Convert.ToInt16(obj); }
			catch { return obj.ToString().ToShort_(defval); }
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的整数或 null</returns>
		public static short? ToShortOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			short _retval;
			if(!short.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的整数或 null</returns>
		public static short? ToShortOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToInt16(obj); }
			catch { return obj.ToString().ToShortOrNull_(); }
		}
		#endregion

		#region ToLong_
		/// <summary>转换为相应的整数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static long ToLong_(this string str, long defval = 0L)
		{
			if(str.IsNullOrEmpty_()) return defval;
			long _retval;
			if(!long.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的整数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的整数或默认值</returns>
		public static long ToLong_<T>(this T obj, long defval = 0L)
		{
			if(null == obj) return defval;
			try { return Convert.ToInt64(obj); }
			catch { return obj.ToString().ToLong_(defval); }
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的整数或 null</returns>
		public static long? ToLongOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			long _retval;
			if(!long.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的整数，转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的整数或 null</returns>
		public static long? ToLongOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToInt64(obj); }
			catch { return obj.ToString().ToLongOrNull_(); }
		}
		#endregion

		#region ToFloat_
		/// <summary>转换为相应的单精度浮点数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的单精度浮点数或默认值</returns>
		public static float ToFloat_(this string str, float defval = 0F)
		{
			if(str.IsNullOrEmpty_()) return defval;
			float _retval;
			if(!float.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的单精度浮点数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的单精度浮点数或默认值</returns>
		public static float ToFloat_<T>(this T obj, float defval = 0F)
		{
			if(null == obj) return defval;
			try { return Convert.ToSingle(obj); }
			catch { return obj.ToString().ToFloat_(defval); }
		}

		/// <summary>转换为相应的单精度浮点数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的单精度浮点数或 null</returns>
		public static float? ToFloatOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			float _retval;
			if(!float.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的单精度浮点数，转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的单精度浮点数或 null</returns>
		public static float? ToFloatOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToSingle(obj); }
			catch { return obj.ToString().ToFloatOrNull_(); }
		}
		#endregion

		#region ToDouble_
		/// <summary>转换为相应的双精度浮点数</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的双精度浮点数或默认值</returns>
		public static double ToDouble_(this string str, double defval = 0D)
		{
			if(str.IsNullOrEmpty_()) return defval;
			double _retval;
			if(!double.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的双精度浮点数</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的双精度浮点数或默认值</returns>
		public static double ToDouble_<T>(this T obj, double defval = 0D)
		{
			if(null == obj) return defval;
			try { return Convert.ToDouble(obj); }
			catch { return obj.ToString().ToDouble_(defval); }
		}

		/// <summary>转换为相应的双精度浮点数，转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的双精度浮点数或 null</returns>
		public static double? ToDoubleOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			double _retval;
			if(!double.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换为相应的双精度浮点数，转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的双精度浮点数或 null</returns>
		public static double? ToDoubleOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToDouble(obj); }
			catch { return obj.ToString().ToDoubleOrNull_(); }
		}
		#endregion

		#region ToDecimal_
		/// <summary>转换高精度的十进制数（一般用于货币）</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的十进制数或默认值</returns>
		public static decimal ToDecimal_(this string str, decimal defval = 0M)
		{
			if(str.IsNullOrEmpty_()) return defval;
			decimal _retval;
			if(!decimal.TryParse(str, NumberStyles.Any, null, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换高精度的十进制数（一般用于货币）</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的十进制数或默认值</returns>
		public static decimal ToDecimal_<T>(this T obj, decimal defval = 0M)
		{
			if(null == obj) return defval;
			try { return Convert.ToDecimal(obj); }
			catch { return obj.ToString().ToDecimal_(defval); }
		}

		/// <summary>转换高精度的十进制数（一般用于货币），转换不成功时返回 null</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的十进制数或 null</returns>
		public static decimal? ToDecimalOrNull_(this string str)
		{
			if(str.IsNullOrEmpty_()) return null;
			decimal _retval;
			if(!decimal.TryParse(str, NumberStyles.Any, null, out _retval))
				return null;
			return _retval;
		}

		/// <summary>转换高精度的十进制数（一般用于货币），转换不成功时返回 null</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的十进制数或 null</returns>
		public static decimal? ToDecimalOrNull_<T>(this T obj)
		{
			if(null == obj) return null;
			try { return Convert.ToDecimal(obj); }
			catch { return obj.ToString().ToDecimalOrNull_(); }
		}
		#endregion

		#region ToEnum_
		/// <summary>转换为相应的枚举值</summary>
		/// <typeparam name="T">枚举类型</typeparam>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的枚举值或默认值</returns>
		public static T ToEnum_<T>(this string str, T defval = default(T)) where T : struct
		{
			if(str.IsNullOrEmpty_()) return defval;
			Type _type = typeof(T);
			if(!_type.IsEnum) return defval;
			T _retval;
			if(Enum.TryParse(str, true, out _retval))
				if(Enum.IsDefined(_type, _retval))
					return _retval;
			return defval;
		}

		/// <summary>转换为相应的枚举值</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="type">枚举类型</param>
		/// <returns>相应的枚举值或默认值</returns>
		public static object ToEnum_(this string str, Type type)
		{
			return Enum.Parse(type, str, true);
		}

		/// <summary>转换为相应的枚举值</summary>
		/// <typeparam name="T">枚举类型</typeparam>
		/// <param name="num">要转换的数字</param>
		/// <param name="defval">转换不成功时的默认值</param>
		/// <returns>相应的枚举值或默认值</returns>
		public static T ToEnum_<T>(this object num, T defval = default(T)) where T : struct
		{
			Type _type = typeof(T);
			if(!_type.IsEnum) return defval;
			try
			{
				T _retval = (T)Enum.ToObject(_type, num);
				if(Enum.IsDefined(_type, _retval))
					return _retval;
				return defval;
			}
			catch { return num.ToString().ToEnum_(defval); }
		}

		/// <summary>转换为相应的枚举值</summary>
		/// <typeparam name="T">枚举类型</typeparam>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的枚举值或 null</returns>
		public static T? ToEnumOrNull_<T>(this string str) where T : struct
		{
			if(str.IsNullOrEmpty_()) return null;
			Type _type = typeof(T);
			if(!_type.IsEnum) return null;
			T _retval;
			if(Enum.TryParse(str, true, out _retval))
				if(Enum.IsDefined(_type, _retval))
					return _retval;
			return null;
		}

		/// <summary>转换为相应的枚举值</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="type">枚举类型</param>
		/// <returns>相应的枚举值或默认值</returns>
		public static object ToEnumOrNull_(this string str, Type type)
		{
			try { return Enum.Parse(type, str, true); }
			catch { return null; }
		}

		/// <summary>转换为相应的枚举值</summary>
		/// <typeparam name="T">枚举类型</typeparam>
		/// <param name="num">要转换的数字</param>
		/// <returns>相应的枚举值或 null</returns>
		public static T? ToEnumOrNull_<T>(this object num) where T : struct
		{
			Type _type = typeof(T);
			if(!_type.IsEnum) return null;
			try
			{
				T _retval = (T)Enum.ToObject(_type, num);
				if(Enum.IsDefined(_type, _retval))
					return _retval;
				return null;
			}
			catch { return num.ToString().ToEnumOrNull_<T>(); }
		}
		#endregion

		#region ToString_
		/// <summary>转换为相应的枚举值名称</summary>
		/// <param name="e">要转换的枚举值</param>
		/// <returns>枚举值名称</returns>
		public static string ToString_(this Enum e)
		{
			return null == e ? string.Empty : Enum.GetName(e.GetType(), e);
		}

		/// <summary>转换为不含分隔符的 GUID 字符串</summary>
		/// <param name="g">要转换的 GUID</param>
		/// <returns>不含分隔符的 GUID 字符串</returns>
		public static string ToString_(this Guid g)
		{
			if(g == null) g = Guid.Empty;
			return g.ToString("N");
		}

		/// <summary>将 true 转换为 "1"，false 转换为 "0"</summary>
		/// <param name="b">要转换的布尔值，true 为 "1"，false 为 "0"</param>
		/// <returns>true 为 "1"，false 为 "0"</returns>
		public static string ToString_(this bool b)
		{
			return b ? "1" : "0";
		}

		/// <summary>将 true 转换为 "1"，false 转换为 "0"</summary>
		/// <param name="b">要转换的布尔值，true 为 "1"，false 为 "0"，为 null 时返回空字符串</param>
		/// <returns>true 为 "1"，false 为 "0"，为 null 时返回空字符串</returns>
		public static string ToString_(this bool? b)
		{
			return null == b ? string.Empty : b.Value.ToString_();
		}

		/// <summary>转换为 yyyy-MM-dd HH:mm:ss 格式的字符串</summary>
		/// <param name="time">要转换的日期</param>
		/// <returns>yyyy-MM-dd HH:mm:ss 格式的字符串</returns>
		public static string ToString_(this DateTime time)
		{
			return time.ToString("yyyy-MM-dd HH:mm:ss");
		}

		/// <summary>转换为 yyyy-MM-dd HH:mm:ss 格式的字符串</summary>
		/// <param name="time">要转换的日期，为 null 时返回空字符串</param>
		/// <returns>yyyy-MM-dd HH:mm:ss 格式或空字符串</returns>
		public static string ToString_(this DateTime? time)
		{
			return null == time ? string.Empty : time.Value.ToString_();
		}

		/// <summary>转换成对应的字符串格式（x 天 xx 小时 xx 分 xx 秒 xxx 毫秒）</summary>
		/// <param name="ts">时间间隔</param>
		/// <returns>“x 天 xx 小时 xx 分 xx 秒 xxx 毫秒”格式的字符串</returns>
		public static string ToString_(this TimeSpan ts)
		{
			STR str = new STR(30);
			bool is0 = true;
			int temp;
			temp = ts.Days;
			if(temp != 0)
			{
				str.Append(temp).Append(Constants.TimeSpanDays);
				is0 = false;
			}
			temp = ts.Hours;
			if(temp != 0 || !is0)
			{
				str.Append(temp).Append(Constants.TimeSpanHours);
			}
			temp = ts.Minutes;
			if(temp != 0 || !is0)
			{
				str.Append(temp).Append(Constants.TimeSpanMinutes);
			}
			temp = ts.Seconds;
			if(temp != 0 || !is0)
			{
				str.Append(temp).Append(Constants.TimeSpanSeconds);
			}
			temp = ts.Milliseconds;
			if(temp != 0 || is0)
			{
				str.Append(temp).Append(Constants.TimeSpanMilliseconds);
			}

			return str.TrimEnd().ToString();
		}

		/// <summary>转换成对应的字符串格式（x 天 xx 小时 xx 分 xx 秒 xxx 毫秒）</summary>
		/// <param name="ts">时间间隔，为 null 时返回空字符串</param>
		/// <returns>“x 天 xx 小时 xx 分 xx 秒 xxx 毫秒”格式或空字符串</returns>
		public static string ToString_(this TimeSpan? ts)
		{
			return null == ts ? string.Empty : ts.Value.ToString_();
		}
		#endregion

		#region ToDate_
		/// <summary>转换为相应的日期</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认日期</param>
		/// <returns>相应的日期或默认值</returns>
		public static DateTime ToDate_(this string str, DateTime defval)
		{
			if(str.IsNullOrEmpty_()) return defval;
			DateTime _retval;
			if(!DateTime.TryParse(str, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>转换为相应的日期</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的日期</returns>
		public static DateTime ToDate_(this string str)
		{
			return ToDate_(str, BaseDateTime);
		}

		/// <summary>以指定格式转换为相应的日期</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="defval">转换不成功时的默认日期</param>
		/// <param name="format">日期格式（如：yyyy年MM月dd日 HH时mm分ss秒），如果格式中任一部分跟要转换的字符串不一致，都将导致转换失败而返回默认日期</param>
		/// <returns>相应的日期或默认值</returns>
		public static DateTime ToDate_(this string str, DateTime defval, string format)
		{
			if(str.IsNullOrEmpty_()) return defval;
			if(format.IsNullOrEmpty_()) return str.ToDate_(defval);
			DateTime _retval;
			if(!DateTime.TryParseExact(str, format, null, DateTimeStyles.None, out _retval))
				return defval;
			return _retval;
		}

		/// <summary>以指定格式转换为相应的日期</summary>
		/// <param name="str">要转换的字符串</param>
		/// <param name="format">日期格式（如：yyyy年MM月dd日 HH时mm分ss秒），如果格式中任一部分跟要转换的字符串不一致，都将导致转换失败而返回默认日期</param>
		/// <returns>相应的日期</returns>
		public static DateTime ToDate_(this string str, string format)
		{
			return ToDate_(str, BaseDateTime, format);
		}

		/// <summary>将以100纳（毫微）秒表示的日期转换为相应的日期</summary>
		/// <param name="num">要转换的数字</param>
		/// <param name="defval">转换不成功时的默认日期</param>
		/// <param name="kind">指示此数字是指定了本地时间、协调世界时 (UTC)，还是两者皆未指定。</param>
		/// <returns>相应的日期或默认值</returns>
		public static DateTime ToDate_(this long num, DateTime defval, DateTimeKind kind = DateTimeKind.Local)
		{
			try { return new DateTime(num, kind); }
			catch { return defval; }
		}

		/// <summary>将以100纳（毫微）秒表示的日期转换为相应的日期</summary>
		/// <param name="num">要转换的数字</param>
		/// <param name="kind">指示此数字是指定了本地时间、协调世界时 (UTC)，还是两者皆未指定。</param>
		/// <returns>相应的日期或默认值</returns>
		public static DateTime ToDate_(this long num, DateTimeKind kind = DateTimeKind.Local)
		{
			return ToDate_(num, BaseDateTime, kind);
		}

		/// <summary>转换为相应的日期</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="defval">转换不成功时的默认日期</param>
		/// <param name="format">如果 obj 为字符串时的日期格式（如：yyyy年MM月dd日 HH时mm分ss秒），如果格式中任一部分跟要转换的字符串不一致，都将导致转换失败而返回默认日期</param>
		/// <returns>相应的日期或默认值</returns>
		public static DateTime ToDate_<T>(this T obj, DateTime defval, string format = null)
		{
			if(null == obj) return defval;
			if(obj is DateTime) return Convert.ToDateTime(obj);
			return obj.ToString().ToDate_(defval, format);
		}

		/// <summary>转换为相应的日期</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="format">如果 obj 为字符串时的日期格式（如：yyyy年MM月dd日 HH时mm分ss秒），如果格式中任一部分跟要转换的字符串不一致，都将导致转换失败而返回默认日期</param>
		/// <returns>相应的日期</returns>
		public static DateTime ToDate_<T>(this T obj, string format = null)
		{
			return obj.ToDate_(BaseDateTime, format);
		}
		#endregion

		#region ToGuid_
		/// <summary>转换为相应的 GUID</summary>
		/// <param name="str">要转换的字符串</param>
		/// <returns>相应的 GUID</returns>
		public static Guid ToGuid_(this string str)
		{
			Guid g = Guid.Empty;
			if(!str.IsNullOrEmpty_())
				Guid.TryParse(str, out g);
			return g;
		}

		/// <summary>转换为相应的 GUID</summary>
		/// <param name="guid">要转换的 GUID</param>
		/// <returns>相应的 GUID</returns>
		public static Guid ToGuid_(this Guid guid)
		{
			return guid;
		}

		/// <summary>转换为相应的 GUID</summary>
		/// <typeparam name="T">数据类型</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <returns>相应的 GUID</returns>
		public static Guid ToGuid_<T>(this T obj)
		{
			if(null == obj) return Guid.Empty;
			return obj.ToString().ToGuid_();
		}
		#endregion

		
		/// <summary>将对象转换成 JSON 字符串</summary>
		/// <param name="obj">要转换的对象</param>
		/// <returns>转换后的 JSON 字符串</returns>
		public static string ToJson_(this object obj)
		{
			return new FastJson.Json().ToJson(obj);
		}



		

		

		

		
	}
}