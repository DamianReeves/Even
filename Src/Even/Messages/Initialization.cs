﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Even.Messages
{
    public class InitializationResult
    {
        private InitializationResult()
        { }

        public bool Initialized { get; private set; }
        public Exception Exception { get; private set; }

        public static InitializationResult Successful()
        {
            return new InitializationResult { Initialized = true };
        }

        public static InitializationResult Failed(Exception ex)
        {
            return new InitializationResult { Initialized = false, Exception = ex };
        }
    }

    public class InitializeEventDispatcher
    {
        public IActorRef Reader { get; set; }
        public TimeSpan RecoveryStartTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }

    public class InitializeEventStoreReader
    {
        public EventRegistry EventRegistry { get; set; }
        public ISerializer Serializer { get; set; }
        public IEventStoreReader StoreReader { get; set; }
    }

    public class InitializeEventStoreWriter
    {
        public ISerializer Serializer { get; set; }
        public IEventStoreWriter StoreWriter { get; set; }
        public IActorRef Dispatcher { get; set; }
    }

    public class InitializeCommandProcessorSupervisor
    {
        public IActorRef Writer { get; set; }
        public IActorRef Reader { get; set; }
    }

    public class InitializeEventProcessorSupervisor
    {
        public IActorRef ProjectionStreamSupervisor { get; set; }
    }

    public class InitializeProjectionStreams
    {
        public IActorRef Reader { get; set; }
        public IActorRef Writer { get; set; }
    }

    public class InitializeCommandProcessor
    {
        public IActorRef CommandProcessorSupervisor { get; set; }
        public IActorRef Writer { get; set; }
    }

    public class InitializeAggregate
    {
        public string StreamID { get; set; }
        public IActorRef Reader { get; set; }
        public IActorRef Writer { get; set; }
        public IActorRef CommandProcessorSupervisor { get; set; }
    }

    public class AggregateInitializationState
    {
        public bool Initialized { get; set; }
        public string InitializationFailureReason { get; set; }
    }

    public class WillStop { }

    public class InitializeProjectionStream
    {
        public ProjectionStreamQuery Query { get; set; }
        public IActorRef Reader { get; set; }
        public IActorRef Writer { get; set; }
    }

    public class InitializeAggregateReplayWorker
    {
        public ReplayAggregateRequest Request { get; set; }
        public IActorRef ReplyTo { get; set; }
        public InitializeEventStoreReader ReaderInitializer { get; set; }
    }

    public class StartEventProcessor
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public class InitializeEventProcessor
    {
        public IActorRef ProjectionStreamSupervisor { get; set; }
    }
}
