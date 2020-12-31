using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rocky
{
    internal class WebAppFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public WebAppFactory([DisallowNull] WebAppConfig config)
        {
            _config = config;
        }

        // This function will be called by framework when initializing web application.
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(ConfigureTestServices);
        }
        
        // This is our gateway to inject dependencies required for testing
        private void ConfigureTestServices(IServiceCollection services)
        {
            // Override dependencies
            foreach (var dependency in _config.Dependencies)
            {
                services.Replace(dependency);
            }
            
            // Inject dynamic dependencies
            foreach (var dynDependency in _config.DynamicDependencies)
            {
                dynDependency.AddServiceDescriptor(services);
            }
            
            // Override db dependencies
            foreach (var dbDependency in _config.DataDependencies)
            {
                dbDependency.AddServiceDescriptor(services);
            }
        }

        #region Private members
        private readonly WebAppConfig _config;
        #endregion
    }
}