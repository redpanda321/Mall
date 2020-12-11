using Mall.Core;
using Mall.IServices;
using Nop.Core.Infrastructure;

namespace Mall.Application
{
    public class BaseApplicaion
    {
        protected static T GetService<T>() where T:IService
        {
            // return ObjectContainer.Current.Resolve<T>();
            return (T)EngineContext.Current.Resolve(typeof(T));

        }
    }

    public class BaseApplicaion<T> : BaseApplicaion where T : IService
    {
        protected static T Service { get { return GetService<T>(); } }
    }
}
