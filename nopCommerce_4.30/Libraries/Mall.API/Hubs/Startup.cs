
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;




namespace Mall.API.Hubs
{
    public class Startup:INopStartup
    {




        public void  ConfigureServices(IServiceCollection services, IConfiguration configuration) {

            services.AddSignalR().AddJsonProtocol(options => {


           
               
           

         } );
          
        
        }



        public void Configure(IApplicationBuilder application)
        {

         


            //app.MapSignalR();
            //服务器的hub注册

            /*
            //消息总线--集线器Hub配置
            app.Map("/print", map => {
                //SignalR允许跨域调用
                //map.UseCors(CorsOptions.AllowAll);
                HubConfiguration config = new HubConfiguration()
                {
                    //禁用JavaScript代理
                    EnableJavaScriptProxies = false,
                    //启用JSONP跨域
                    EnableJSONP = true,
                    //反馈结果给客户端
                    EnableDetailedErrors = true
                };
                map.RunSignalR(config);
            });


            */
            //WebApi允许跨域调用
            //app.UseCors(CorsOptions.AllowAll);
        }



        public int Order
        {
            get { return 0; } // add after nop is done
        }


    }
}
