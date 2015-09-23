﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Even
{
    public interface IPersistedRawEvent
    {
        long GlobalSequence { get; }
        Guid EventID { get; }
        string StreamID { get; }
        string OriginalStreamID { get; }
        DateTime UtcTimestamp { get; }
        string EventType { get; }
        byte[] Metadata { get; }
        byte[] Payload { get; }
        int PayloadFormat { get; }
    }

    public class PersistedRawEvent : IPersistedRawEvent
    {
        public long GlobalSequence { get; set; }
        public Guid EventID { get; set; }
        public string StreamID { get; set; }
        public string OriginalStreamID { get; set; }
        public DateTime UtcTimestamp { get; set; }
        public string EventType { get; set; }
        public byte[] Metadata { get; set; }
        public byte[] Payload { get; set; }
        public int PayloadFormat { get; set; }
    }
}
