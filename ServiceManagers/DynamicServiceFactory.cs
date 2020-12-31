using System.Diagnostics.CodeAnalysis;

namespace Rocky.ServiceManagers
{
    internal class DynamicDependencyFactory<TType>
        where TType : class
    {
        public DynamicDependencyFactory([DisallowNull] DynamicServiceManager<TType> serviceManager)
        {
            _serviceManager = serviceManager;
            _instance = (TType)serviceManager.DefaultInstance;
        }
        
        public TType CreateInstance()
        {
            return _instance;
        }

        public void SetInstance(TType instance)
        {
            _instance = instance;
        }

        private readonly DynamicServiceManager<TType> _serviceManager;
        private TType _instance;
    }
}