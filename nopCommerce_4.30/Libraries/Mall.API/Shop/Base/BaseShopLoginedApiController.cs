using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mall.Core;
using Mall.Core.Helper;
using Mall.DTO;
using Mall.Application;
using Mall.Web.Framework;

namespace Mall.API
{
    [BaseShopLoginedActionFilter]
    public abstract class BaseShopLoginedApiController : BaseShopApiController
    {

    }
}
