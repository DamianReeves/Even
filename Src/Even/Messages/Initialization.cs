﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Even.Messages
{
    public class InitializeEventStoreReader
    {
        public ICryptoService CryptoService { get; set; }
        public IDataSerializer Serializer { get; set; }
        public Func<IStorageReader> ReaderFactory { get; set; }
    }

    public class InitializeEventStoreWriter
    {
        public ICryptoService CryptoService { get; set; }
        public IDataSerializer Serializer { get; set; }
        public Func<IStorageWriter> WriterFactory { get; set; }
    }

    public class InitializeAggregateSupervisor
    {
        public IActorRef Writer { get; set; }
        public IActorRef Reader { get; set; }
    }

    public class InitializeProjectionIndexWriter
    {
        public Func<IStorageWriter> WriterFactory { get; set; }
    }

    public class InitializeProjectionSupervisor
    {
        public IActorRef Reader { get; set; }
    }

    public class InitializeAggregate
    {
        public string StreamID { get; set; }
        public IActorRef Reader { get; set; }
        public IActorRef Writer { get; set; }

        public IActorRef CommandSender { get; set; }
        public AggregateCommandRequest Command { get; set; }
    }

    public class InitializeProjectionStream
    {
        public ProjectionQuery Query { get; set; }
        public IActorRef EventReader { get; set; }
        public IActorRef IndexWriter { get; set; }
    }

    public class InitializeProjectionStreamReplayWorker
    {
        public ReplayEventsRequest Request { get; set; }
        public IActorRef ReplyTo { get; set; }
        public Func<IRawStorageEvent, IEvent> Deserializer { get; set; }
        public IStorageReader StorageReader { get; set; }
    }

    public class InitializeAggregateReplayWorker
    {
        public ReplayStreamRequest Request { get; set; }
        public IActorRef ReplyTo { get; set; }
        public InitializeEventStoreReader ReaderInitializer { get; set; }
    }

    public class InitializeProjection
    {
        public IActorRef Supervisor;
    }
}
