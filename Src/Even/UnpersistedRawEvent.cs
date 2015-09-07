﻿using System;
using System.Diagnostics.Contracts;

namespace Even
{
    public class UnpersistedRawEvent
    {
        public UnpersistedRawEvent(Guid eventId, string streamId, string eventType, DateTime utcTimestamp, byte[] metadata, byte[] payload, int payloadFormat)
        {
            Contract.Requires(eventId != Guid.Empty);
            Contract.Requires(!String.IsNullOrEmpty(streamId));
            Contract.Requires(!String.IsNullOrEmpty(eventType));
            Contract.Requires(utcTimestamp != default(DateTime));
            Contract.Requires(payload != null);

            EventID = eventId;
            StreamID = streamId;
            EventType = eventType;
            UtcTimestamp = utcTimestamp;
            Metadata = metadata;
            Payload = payload;
            PayloadFormat = payloadFormat;
        }

        public long GlobalSequence { get; private set; }
        public Guid EventID { get; }
        public string StreamID { get; }
        public string EventType { get; }
        public DateTime UtcTimestamp { get; }
        public byte[] Metadata { get; }
        public byte[] Payload { get; }
        public int PayloadFormat { get; }

        public bool SequenceWasSet { get; private set; }

        public void SetGlobalSequence(long globalSequence)
        {
            Contract.Requires(globalSequence > 0);

            if (SequenceWasSet)
                throw new InvalidOperationException("Sequences should be set only once.");

            GlobalSequence = globalSequence;

            SequenceWasSet = true;
        }
    }
}
