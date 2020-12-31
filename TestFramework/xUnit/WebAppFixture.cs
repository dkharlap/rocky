using System;

namespace Rocky.TestFramework.xUnit
{
    public class WebAppContextFixture<TStartup, TWebAppConfig>
        where TStartup : class
        where TWebAppConfig : WebAppConfig
    {
        public WebAppContextFixture()
        {
            // Unfortunately xUnit doesn't support full DI, so using this dynamically create configuration
            _config = Activator.CreateInstance<TWebAppConfig>();
        }

        public WebApp<TStartup> CreateWebApp()
        {
            return new WebApp<TStartup>(_config);
        }

        private readonly TWebAppConfig _config;
    }
}