﻿using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Even.Messages;
using Even.Messages.Initialization;
using System.Diagnostics.Contracts;

namespace Even
{
    public class EventStore : ReceiveActor
    {
        EventStoreSettings _settings;
        IEventStore _store;
        GlobalOptions _options;
        IActorRef _reader;
        IActorRef _writer;
        IActorRef _dispatcher;
        IActorRef _projectionStreams;
        IActorRef _commandProcessors;
        IActorRef _eventProcessors;
        EventRegistry _registry = new EventRegistry();

        public EventStore()
        {
            Become(Uninitialized);
        }

        void Uninitialized()
        {
            Receive<InitializeEventStore>(ini =>
            {
                _settings = ini.Settings;
                _store = ini.Settings.Store;

                InitializeChildren();
                InitializeEventProcessors(ini.EventProcessors);

                Sender.Tell(new EventStoreInitializationState
                {
                    Initialized = true,
                    CommandProcessors = _commandProcessors,
                    EventProcessors = _eventProcessors
                });

                Become(Ready);
            });
        }

        void Ready()
        { }

        void InitializeChildren()
        {
            // initialize reader
            var factory = new PersistedEventFactory2(_registry, _settings.Serializer);

            _reader = Context.ActorOf<EventStoreReader>("reader");
            _reader.Tell(new InitializeEventStoreReader(_store, factory));

            // initialize dispatcher
            _dispatcher = Context.ActorOf<EventDispatcher>("dispatcher");

            _dispatcher.Tell(new InitializeEventDispatcher
            {
                Reader = _reader
            });

            // initialize writer
            _writer = Context.ActorOf<EventStoreWriter>("writer");

            _writer.Tell(new InitializeEventStoreWriter
            {
                StoreWriter = _settings.Store,
                Serializer = _settings.Serializer,
                Dispatcher = _dispatcher
            });

            // initialize the projection streams supervisor
            var projectionStreamsProps = Props.Create<ProjectionStreams>(_reader, _writer, _options);
            _projectionStreams = Context.ActorOf(projectionStreamsProps, "projectionstreams");
            
            // initialize event processor supervisor
            _eventProcessors = Context.ActorOf<EventProcessorSupervisor>("eventprocessors");

            _eventProcessors.Tell(new InitializeEventProcessorSupervisor
            {
                ProjectionStreamSupervisor = _projectionStreams
            });

            // initialze command processors
            _commandProcessors = Context.ActorOf<CommandProcessorSupervisor>("commandprocessors");

            _commandProcessors.Tell(new InitializeCommandProcessorSupervisor
            {
                Reader = _reader,
                Writer = _writer
            });
        }

        void InitializeEventProcessors(IEnumerable<EventProcessorEntry> entries)
        {
            foreach (var e in entries)
            {
                _eventProcessors.Tell(new StartEventProcessor
                {
                    Type = e.Type,
                    Name = e.Name
                });
            }
        }
    }
}
