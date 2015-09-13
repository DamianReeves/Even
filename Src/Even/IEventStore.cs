﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Even
{
    // stores should implement at least IEventStore, and optionally IProjectionStore
    // stores should implement all interfaces in the same type

    public interface IEventStore : IEventStoreReader, IEventStoreWriter
    { }

    public interface IProjectionStore : IProjectionStoreWriter, IProjectionStoreReader
    { }

    // the following interfaces exist only for internal use

    public interface IEventStoreReader
    {
        Task WriteAsync(IReadOnlyCollection<UnpersistedRawEvent> events);
        Task WriteStreamAsync(string streamId, int expectedSequence, IReadOnlyCollection<UnpersistedRawEvent> events);
    }

    public interface IEventStoreWriter
    {
        Task ReadAsync(long start, int count, Action<IPersistedRawEvent> readCallback, CancellationToken ct);
        Task ReadStreamAsync(string streamId, int start, int count, Action<IPersistedRawEvent> readCallback, CancellationToken ct);
    }
    
    public interface IProjectionStoreWriter
    {
        Task WriteProjectionIndexAsync(string streamId, int expectedSequence, IReadOnlyCollection<long> globalSequences);
        Task WriteProjectionCheckpointAsync(string streamId, long globalSequence);
    }

    public interface IProjectionStoreReader
    {
        Task<long> ReadProjectionCheckpointAsync(string streamId);
        Task<long> ReadHighestIndexedProjectionGlobalSequenceAsync(string streamId);
        Task<int> ReadHighestIndexedProjectionStreamSequenceAsync(string streamId);

        Task ReadIndexedProjectionStreamAsync(string streamId, int initialSequence, int maxEvents, Action<IPersistedRawEvent> readCallback, CancellationToken ct);
    }
}
