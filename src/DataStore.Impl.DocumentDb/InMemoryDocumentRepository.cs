﻿using System;

namespace DataStore.DataAccess.Impl.DocumentDb
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DataStore.DataAccess.Interfaces.Events;

    using Infrastructure.PureFunctions.PureFunctions.Extensions;
    using Interfaces;
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    public class InMemoryDocumentRepository : IDocumentRepository
    {
        public List<IAggregate> Aggregates { get; set; } = new List<IAggregate>();

        #region IDocumentRepository Members

        public Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate
        {
            Aggregates.Add(aggregateAdded.Model);

            return Task.FromResult(aggregateAdded.Model);
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            return Clone(Aggregates.Where(x => x is T).Cast<T>()).AsQueryable();
        }

        public Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate
        {
            var aggregate = Aggregates.Single(a => a.id == aggregateHardDeleted.Model.id);

            Aggregates.RemoveAll(a => a.id == aggregateHardDeleted.Model.id);

            return Task.FromResult((T) aggregate);
        }

        public Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate
        {
            var aggregate = Aggregates.Single(a => a.id == aggregateSoftDeleted.Model.id);
            (aggregate as dynamic).Active = false;

            return Task.FromResult((T) aggregate);
        }

        public void Dispose()
        {
            Aggregates.Clear();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var cloned = aggregatesQueried.Query.ToList().AsEnumerable();

            return Task.FromResult(cloned);
        }

        public Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            return Task.FromResult(Aggregates.Exists(a => a.id == aggregateQueriedById.Id));
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            return Task.FromResult(Aggregates.Cast<T>().Single(a => a.id == aggregateQueriedById.Id));
        }

        public Task<Document> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            var queryable = Aggregates.AsQueryable().Where(x => x.id == aggregateQueriedById.Id);

            var d = new Document();

            var json = JsonConvert.SerializeObject(queryable.ToList().Single());

            d.LoadFrom(new JsonTextReader(new StringReader(json)));

            return Task.FromResult(d);
        }

        public Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate
        {
            return UpdateAsync(aggregateUpdated.Model);
        }

        #endregion

        private Task<T> UpdateAsync<T>(T item) where T : IHaveAUniqueId
        {
            var toUpdate = Aggregates.Single(x => x.id == item.id);

            item.CopyProperties(toUpdate);

            return Task.FromResult((T) toUpdate);
        }

        private IEnumerable<T> Clone<T>(IEnumerable<T> toClone) where T: IHaveAUniqueId
        {
            var asJson = JsonConvert.SerializeObject(toClone);
            var cloned = JsonConvert.DeserializeObject<IEnumerable<T>>(asJson);
            return cloned;
        }
    }

}