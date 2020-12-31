using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Rocky.ServiceManagers.Data
{
    public class MongoDbServiceManager : IDataServiceManager
    {
        public MongoDbServiceManager(bool cleanStructureOnExit = false)
        {
            CleanStructureOnExit = cleanStructureOnExit;
        }

        public bool CleanStructureOnExit { get; }

        public void AddServiceDescriptor(IServiceCollection serviceCollection)
        {
            throw new System.NotImplementedException();
        }

        public void CleanupData(DbContext context)
        {
            throw new System.NotImplementedException();
        }

        public void CleanupStructure(DbContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}