using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Mall.Web
{
	/// <summary>
	/// ms-excel视图
	/// </summary>
	public class ExcelResult:ViewResult
	{
		#region 字段
		private string _fileName;
		#endregion

		#region 构造函数
		public ExcelResult(string fileName, object model)
			: this(null, fileName, model)
		{
		}

		public ExcelResult(string viewName, string fileName, object model)
		{
			this.ViewName = viewName;
			this.ViewData.Model = model;
			_fileName = fileName;
		}
		#endregion

		#region 重写方法
		public      Task  ExecuteResultAsync(ControllerContext context)
		{
			base.ExecuteResult(context);

			var response = context.HttpContext.Response;
			response.ContentType = "application/ms-excel";
		//	response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");

            response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}-{1:yyyy-MM-dd}.xls", _fileName, DateTime.Now));

            return null;
        }
		#endregion
	}
}