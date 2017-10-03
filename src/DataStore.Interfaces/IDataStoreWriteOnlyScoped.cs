namespace DataStore.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreWriteOnlyScoped<T> :
        IDataStoreCreateCapabilitiesScoped<T>,
        IDataStoreDeleteCapabilitiesScoped<T>,
        IDataStoreUpdateCapabilitiesScoped<T>,
        IDisposable where T : class, IAggregate, new()

    {
        Task CommitChanges();
    }
}