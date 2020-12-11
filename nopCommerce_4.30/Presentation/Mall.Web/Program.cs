
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Senparc.CO2NET;

namespace Mall.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
             .UseServiceProviderFactory(new AutofacServiceProviderFactory())
             .ConfigureWebHostDefaults(webBuilder =>
                 {

                     webBuilder.UseStartup<Startup>().CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true");

                 });
             
             //.UseServiceProviderFactory(new SenparcServiceProviderFactory()); 

        }
        /*
         * 
         * 
         * webHostBuilder.UseKestrel(serverOptions =>
                 {
                     // Set properties and call methods on options
                 })
        .UseIISIntegration()
        .UseKestrel()
         .UseIISIntegration()
        .CaptureStartupErrors(true)
        .UseSetting("detailedErrors", "true")
            .UseStartup<Startup>();

        */
    }
}
