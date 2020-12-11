using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Mall.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Mall.Web
{
    public class AutoFacContainer : IinjectContainer
    {
		#region 字段
		private ContainerBuilder builder;
		private IContainer container;

        public IServiceProvider serviceProvider { get; set; }
        #endregion

        #region 构造函数


        public AutoFacContainer(IServiceCollection services)
        {
            builder = new ContainerBuilder();
            SetupResolveRules(builder);  //注入

            builder.Populate(services);

            container = builder.Build();

            serviceProvider = new AutofacServiceProvider(container);

        }


        public AutoFacContainer()
		{
			builder = new ContainerBuilder();
			SetupResolveRules(builder);  //
            //  builder.RegisterControllers(Assembly.GetExecutingAssembly());  //注入所有Controller           

            container = builder.Build();
      	//		DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
		}
		#endregion

		#region IinjectContainer 成员
		public void RegisterType<T>()
		{
			builder.RegisterType<T>();
		}

		public T Resolve<T>()
		{
			return container.Resolve<T>();
		}

		public object Resolve(Type type)
		{
			return container.Resolve(type);
		}
		#endregion

		#region 私有方法
		private void SetupResolveRules(ContainerBuilder builder)
		{
			var services = Assembly.Load("Mall.Service");
			builder.RegisterAssemblyTypes(services).Where(t => t.GetInterface(typeof(Mall.IServices.IService).Name)!=null).AsImplementedInterfaces().InstancePerLifetimeScope();
		//	var reader = new ConfigurationSettingsReader("autofac");
		//	builder.RegisterModule(reader);
		}
		#endregion
	}
}