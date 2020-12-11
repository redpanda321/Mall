



using Microsoft.AspNetCore.SignalR;
using Nop.Core.Infrastructure;
using System;



namespace Mall.API.Base
{
    public abstract class ApiControllerWithHub<THub> : BaseShopBranchLoginedApiController
       where THub : Hub
    {
       protected IHubClients  Clients { get; private set; }
        protected IGroupManager Groups { get; private set; }
        public  ApiControllerWithHub()
        {

            IHubContext<Hub> hubcontext = EngineContext.Current.Resolve<IHubContext<Hub>>();

            var context = hubcontext;
            Clients = context.Clients;
            Groups = context.Groups;
        }
    }
}
