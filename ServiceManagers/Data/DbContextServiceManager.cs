using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rocky.ServiceManagers.Data
{
    public class DbContextServiceManager<TDbContext> : IDataServiceManager
        where TDbContext : DbContext
    {
        public bool CleanStructureOnExit { get; }

        public DbContextServiceManager(
            Action<DbContextOptionsBuilder> optionsAction = null,
            bool cleanStructureOnExit = false,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        {
            CleanStructureOnExit = cleanStructureOnExit;
            OptionsAction = optionsAction;
            ContextLifetime = contextLifetime;
            OptionsLifetime = optionsLifetime;
        }

        public void AddServiceDescriptor([DisallowNull] IServiceCollection serviceCollection)
        {
            serviceCollection.RemoveAll<TDbContext>();
            serviceCollection.RemoveAll<DbContextOptions<TDbContext>>();

            serviceCollection.AddDbContext<TDbContext>(
                OptionsAction, ContextLifetime, OptionsLifetime);
        }

        public void CleanupData(DbContext context)
        {
            context.WipeAllDataFromDatabase();
        }

        public void CleanupStructure(DbContext context)
        {
            throw new NotImplementedException();
        }

        protected readonly Action<DbContextOptionsBuilder> OptionsAction;
        protected readonly ServiceLifetime ContextLifetime;
        protected readonly ServiceLifetime OptionsLifetime;
    }


    public class DbContextServiceManager<TDbContext, TDbContextImplementation> : DbContextServiceManager<TDbContext>
        where TDbContext : DbContext
        where TDbContextImplementation : TDbContext
    {
        public new void AddServiceDescriptor(IServiceCollection serviceCollection)
        {
            serviceCollection.RemoveAll<TDbContext>();
            serviceCollection.RemoveAll<DbContextOptions<TDbContext>>();

            serviceCollection.AddDbContext<TDbContext, TDbContextImplementation>(
                OptionsAction, ContextLifetime, OptionsLifetime);
        }
    }
}