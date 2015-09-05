﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Even.Persistence
{
    public class InMemoryStore : IStreamStore
    {
        List<PersistedRawEvent> _events = new List<PersistedRawEvent>();
        Dictionary<string, List<PersistedRawEvent>> _projections = new Dictionary<string, List<PersistedRawEvent>>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, long> _projectionCheckpoints = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        public List<PersistedRawEvent> GetEvents()
        {
            lock (_events)
            {
                return _events.ToList();
            }
        }

        #region StreamStore

        public Task WriteEventsAsync(string streamId, int expectedSequence, IReadOnlyCollection<UnpersistedRawEvent> events)
        {
            lock (_events)
            {
                var streamEvents = _events
                    .Where(e => String.Equals(e.StreamID, streamId, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.StreamSequence);

                var lastStreamSequence = streamEvents.Any() ? streamEvents.Max() : 0;

                if (expectedSequence >= 0 && expectedSequence != lastStreamSequence)
                    throw new UnexpectedStreamSequenceException();

                var globalSequence = _events.Count + 1;
                var streamSequence = lastStreamSequence + 1;

                foreach (var e in events)
                {
                    e.SetSequences(globalSequence++, streamSequence++);

                    var p = new PersistedRawEvent
                    {
                        GlobalSequence = e.GlobalSequence,
                        EventID = e.EventID,
                        StreamID = streamId,
                        StreamSequence = e.StreamSequence,
                        EventType = e.EventType,
                        UtcTimestamp = e.UtcTimestamp,
                        Metadata = e.Metadata,
                        Payload = e.Payload
                    };

                    _events.Add(p);
                }
            }

            return Task.CompletedTask;
        }

        public Task<long> ReadHighestGlobalSequence()
        {
            lock (_events)
            {
                return Task.FromResult((long) _events.Count);
            }
        }

        public Task<int> ReadHighestStreamSequenceAsync(string streamId)
        {
            lock (_events)
            {
                var streamEvents = _events
                    .Where(e => String.Equals(e.StreamID, streamId, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.StreamSequence);

                return Task.FromResult(streamEvents.Any() ? streamEvents.Max() : 0);
            }
        }

        public Task ReadAsync(long initialCheckpoint, int maxEvents, Action<IPersistedRawEvent> readCallback, CancellationToken ct)
        {
            lock (_events)
            {
                foreach (var e in _events.Skip((int)initialCheckpoint).Take(maxEvents))
                    readCallback(e);
            }

            return Task.CompletedTask;
        }

        public Task ReadStreamAsync(string streamId, int initialSequence, int maxEvents, Action<IPersistedRawEvent> readCallback, CancellationToken ct)
        {
            lock (_events)
            {
                var streamEvents = _events
                    .Where(e => String.Equals(e.StreamID, streamId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.StreamSequence);

                foreach (var e in streamEvents.Skip(initialSequence).Take(maxEvents))
                    readCallback(e);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
