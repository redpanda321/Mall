using AutoMapper;
using Mall.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Mall.Web.Framework.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring common features and middleware on application startup
    /// </summary>
    public class MallCommonStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
         
            //add Senparc
           // services.AddSenparcGlobalServices(configuration)//Senparc.CO2NET 全局注册
            //        .AddSenparcWeixinServices(configuration);//Senparc.Weixin 注册


            services.AddThemes();


           

        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            

            //use static files feature
            application.UseMallStaticFiles();

          
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 200; //common services should be loaded after error handlers
    }
}