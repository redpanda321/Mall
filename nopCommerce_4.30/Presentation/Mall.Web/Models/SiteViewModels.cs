using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mall.Web.Models
{
	public class SiteViewModels
	{
		public class PayViewModel
		{
			public string Controller { get; set; }

			public string Action { get; set; }

			public RouteValueDictionary RouteValueDictionary { get; set; }
		}
	}
}