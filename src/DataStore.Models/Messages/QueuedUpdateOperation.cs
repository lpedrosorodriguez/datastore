﻿using System;
using System.Threading.Tasks;
using DataStore.Interfaces;
using DataStore.Interfaces.LowLevel;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;

namespace DataStore.Models.Messages
{
    public class QueuedUpdateOperation<T> : IQueuedDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public QueuedUpdateOperation(string methodCalled, T model, IDocumentRepository repo, IMessageAggregator messageAggregator)
        {
            CommitClosure = async () =>
            {
                await messageAggregator.CollectAndForward(new UpdateOperation<T>
                    {
                        TypeName = typeof(T).FullName,
                        MethodCalled = methodCalled,
                        Created = DateTime.UtcNow,
                        Model = model
                    })
                    .To(repo.UpdateAsync).ConfigureAwait(false);

                Committed = true;
            };

            Created = DateTime.UtcNow;
            Model = model;
            AggregateId = model.id;
        }

        public double StateOperationCost { get; set; }
        public TimeSpan StateOperationDuration { get; set; }
        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public DateTime Created { get; set; }
        public Guid AggregateId { get; set; }
        public T Model { get; set; }
    }

    public class UpdateOperation<T> : IDataStoreWriteOperation<T> where T : class, IAggregate, new()
    {
        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public DateTime Created { get; set; }
        public double StateOperationCost { get; set; }
        public TimeSpan StateOperationDuration { get; set; }
        public T Model { get; set; }
    }
}