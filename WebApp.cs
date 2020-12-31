using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using Rocky.ServiceManagers;

namespace Rocky
{
    public class WebApp<TStartup> : IDisposable
        where TStartup : class
    {
        public WebApp([DisallowNull] WebAppConfig config)
        {
            _config = config;
            _webAppFactory = new WebAppFactory<TStartup>(config);
            
            HttpClient = _webAppFactory.CreateClient(config.ClientOptions);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Get service, ensuring the scope is defined for scoped dependencies
        /// </summary>
        public TType GetService<TType>(IServiceScope scope = null)
            where TType : class
        {
            var services = _webAppFactory.Services;
            
            scope ??= services.CreateScope();
            return scope.ServiceProvider.GetService<TType>();
        }

        public void SetService<TType>([DisallowNull] TType obj)
            where TType : class
        {
            var services = _webAppFactory.Services;
            var typeFactory = services.GetService<DynamicDependencyFactory<TType>>();

            if (typeFactory == null)
                throw new Exception($"{obj.GetType()} is not defined as DynamicDependency");
            
            typeFactory.SetInstance(obj);
        }

        public readonly HttpClient HttpClient;

        private readonly WebAppConfig _config;
        private readonly WebAppFactory<TStartup> _webAppFactory;
    }
}