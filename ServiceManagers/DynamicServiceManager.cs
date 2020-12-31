using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rocky.ServiceManagers
{
    public class DynamicServiceManager<TType> : IDynamicServiceDescriptor
        where TType : class
    {
        public DynamicServiceManager([DisallowNull] TType defaultInstance)
        {
            _defaultInstance = defaultInstance;
        }

        public void AddServiceDescriptor([DisallowNull] IServiceCollection serviceCollection)
        {
            serviceCollection.RemoveAll(typeof(TType));
            serviceCollection.AddSingleton(new DynamicDependencyFactory<TType>(this));
            serviceCollection.AddSingleton(s =>
            {
                var factory = s.GetRequiredService<DynamicDependencyFactory<TType>>();
                return factory.CreateInstance();
            });
        }

        public object DefaultInstance => _defaultInstance;

        private readonly TType _defaultInstance;
    }
}