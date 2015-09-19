﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Even
{
    public interface IUnpersistedRawEvent
    {
        long GlobalSequence { get; set; }
        Guid EventID { get; }
        string EventType { get; }
        DateTime UtcTimestamp { get; }
        byte[] Metadata { get; }
        byte[] Payload { get; }
        int PayloadFormat { get; }
    }

    public interface IUnpersistedRawStreamEvent : IUnpersistedRawEvent
    {
        string StreamID { get; }
    }

    public class UnpersistedRawEvent : IUnpersistedRawStreamEvent
    {
        public UnpersistedRawEvent(Guid eventId, string streamId, string eventType, DateTime utcTimestamp, byte[] metadata, byte[] payload, int payloadFormat)
        {
            Argument.Requires(eventId != Guid.Empty);
            Argument.Requires(!String.IsNullOrEmpty(streamId), nameof(streamId));
            Argument.Requires(!String.IsNullOrEmpty(eventType));
            Argument.Requires(utcTimestamp != default(DateTime));
            Argument.Requires(payload != null);

            StreamID = streamId;
            EventID = eventId;
            EventType = eventType;
            UtcTimestamp = utcTimestamp;
            Metadata = metadata;
            Payload = payload;
            PayloadFormat = payloadFormat;
        }

        public long GlobalSequence { get; set; }
        public string StreamID { get; }
        public Guid EventID { get; }
        public string EventType { get; }
        public DateTime UtcTimestamp { get; }
        public byte[] Metadata { get; }
        public byte[] Payload { get; }
        public int PayloadFormat { get; }

        public static List<UnpersistedRawEvent> FromUnpersistedEvents(IEnumerable<UnpersistedEvent> events, ISerializer serializer)
        {
            Argument.Requires(events != null);
            Argument.Requires(serializer != null);

            return events.Select(e =>
            {
                var format = EvenStorageFormatAttribute.GetStorageFormat(e.DomainEvent.GetType());
                var metadata = serializer.SerializeMetadata(e.Metadata);
                var payload = serializer.SerializeEvent(e.DomainEvent, format);

                var re = new UnpersistedRawEvent(e.EventID, e.StreamID, e.EventType, e.UtcTimestamp, metadata, payload, format);

                return re;
            }).ToList();
        }
    }
}
