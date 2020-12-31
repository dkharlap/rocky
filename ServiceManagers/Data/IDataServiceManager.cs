using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Rocky.ServiceManagers.Data
{
    public interface IDataServiceManager
    {
        public bool CleanStructureOnExit { get; }

        void AddServiceDescriptor(IServiceCollection serviceCollection);
        void CleanupData(DbContext context);
        void CleanupStructure(DbContext context);
    }
}