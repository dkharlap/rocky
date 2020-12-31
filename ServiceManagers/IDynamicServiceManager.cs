using System;
using Microsoft.Extensions.DependencyInjection;

namespace Rocky.ServiceManagers
{
    public interface IDynamicServiceDescriptor
    {
        public object DefaultInstance { get; }
        
        public void AddServiceDescriptor(IServiceCollection serviceCollection);
    }
}