using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rocky.ServiceManagers;
using Rocky.ServiceManagers.Data;

namespace Rocky
{
    public class WebAppConfig
    {
        public readonly WebApplicationFactoryClientOptions ClientOptions;
        
        public readonly IEnumerable<ServiceDescriptor> Dependencies;
        public readonly IEnumerable<IDynamicServiceDescriptor> DynamicDependencies;
        public readonly IEnumerable<IDataServiceManager> DataDependencies;
        
        public WebAppConfig(
            [DisallowNull] WebApplicationFactoryClientOptions clientOptions, 
            IEnumerable<ServiceDescriptor> dependencies = null,
            IEnumerable<IDynamicServiceDescriptor> dynamicDependencies = null, 
            IEnumerable<IDataServiceManager> dataDependencies = null)
        {
            ClientOptions = clientOptions;
            Dependencies = dependencies;
            DynamicDependencies = dynamicDependencies;
            DataDependencies = dataDependencies;
        }
    }
}