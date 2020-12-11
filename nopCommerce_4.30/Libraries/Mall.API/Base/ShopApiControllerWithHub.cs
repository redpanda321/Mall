using Microsoft.AspNetCore.SignalR;
using Nop.Core.Infrastructure;
using System;



namespace Mall.API.Base
{
    public abstract class ShopApiControllerWithHub<THub> : BaseShopLoginedApiController
     where THub : Hub
    {
        protected IHubClients  Clients { get; private set; }
        protected IGroupManager Groups { get; private set; }
        public  ShopApiControllerWithHub()
        {

            var context = EngineContext.Current.Resolve<IHubContext<Hub>>();
            Clients = context.Clients;
            Groups = context.Groups;
        }
    }
}
