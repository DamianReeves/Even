﻿using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Even.Messages;
using System.Diagnostics.Contracts;

namespace Even
{
    public class EvenGateway 
    {
        public EvenGateway(EvenServices services, ActorSystem system, GlobalOptions options)
        {
            Argument.RequiresNotNull(services, nameof(services));
            Argument.RequiresNotNull(system, nameof(system));
            Argument.RequiresNotNull(options, nameof(options));

            this._system = system;
            this.Services = services;
            this._options = options;
        }

        public EvenServices Services { get; }
        ActorSystem _system;
        GlobalOptions _options;

        /// <summary>
        /// Sends a command to an aggregate using the aggregate's category as the stream id.
        /// </summary>
        public Task SendAggregateCommand<T>(object command, TimeSpan? timeout = null)
            where T : Aggregate
        {
            var streamId = ESCategoryAttribute.GetCategory(typeof(T));
            return SendAggregateCommand<T>(streamId, command, timeout);
        }

        /// <summary>
        /// Sends a command to an aggregate using the aggregate's category and an id to compose the stream id.
        /// The stream will be generated as "category-id".
        /// </summary>
        public Task<CommandResult> SendAggregateCommand<T>(object id, object command, TimeSpan? timeout = null)
            where T : Aggregate
        {
            Contract.Requires(id != null);

            var category = ESCategoryAttribute.GetCategory(typeof(T));
            var streamId = id != null ? category + "-" + id.ToString() : category;

            return SendAggregateCommand<T>(streamId, command, timeout);
        }

        /// <summary>
        /// Sends a command to an aggregate using the specified stream id.
        /// </summary>
        public Task<CommandResult> SendAggregateCommand<T>(string streamId, object command, TimeSpan? timeout = null)
            where T : Aggregate
        {
            Argument.RequiresNotNull(streamId, nameof(streamId));
            Argument.RequiresNotNull(command, nameof(command));

            var to = timeout ?? _options.DefaultCommandTimeout;

            var aggregateCommand = new AggregateCommand(streamId, command, to);
            var envelope = new AggregateCommandEnvelope(typeof(T), aggregateCommand);

            // TODO: add some threshold to Ask higher than the timeout
            return Ask(Services.Aggregates, envelope, to);
        }

        private static async Task<CommandResult> Ask(IActorRef actor, object msg, TimeSpan timeout)
        {
            object response;

            try
            {
                response = (CommandResponse)await actor.Ask(msg, timeout);
            }
            catch (TaskCanceledException)
            {
                throw new CommandException("Command timeout");
            }

            if (response is CommandSucceeded)
                return new CommandResult();

            if (response is CommandRejected)
                return new CommandResult(((CommandRejected)response).Reasons);

            if (response is CommandFailed)
            {
                var cf = (CommandFailed)response;
                throw new CommandException("An error occoured while processing the command: " + cf.Reason, cf.Exception);
            }

            throw new UnexpectedCommandResponseException(response);
        }

        public Task<object> Query(object query, TimeSpan? timeout = null)
        {
            var to = timeout ?? _options.DefaultQueryTimeout;
            var q = QueryFactory.Create(query, Timeout.In(to));
            var msg = QueryAsker.CreateMessage(q, to);

            var asker = _system.ActorOf(QueryAsker.Props);
            return asker.Ask(msg, to);
        }

        /// <summary>
        /// Sends a query through the event stream and responds with the first message it receives back.
        /// </summary>
        class QueryAsker : ReceiveActor
        {
            public static readonly Props Props = Props.Create<QueryAsker>();

            IActorRef _sender;

            public QueryAsker()
            {
                Receive<Request>(request =>
                {
                    _sender = Sender;
                    Context.System.EventStream.Publish(request.Message);

                    Become(() =>
                    {
                        SetReceiveTimeout(request.Timeout);

                        Receive<ReceiveTimeout>(_ =>
                        {
                            Context.Stop(Self);
                        });

                        ReceiveAny(response =>
                        {
                            _sender.Forward(response);
                            Context.Stop(Self);
                        });
                    });
                });
            }

            class Request
            {
                public IQuery Message { get; set; }
                public TimeSpan Timeout { get; set; }
            }

            public static object CreateMessage(IQuery message, TimeSpan timeout)
            {
                return new Request { Message = message, Timeout = timeout };
            }
        }
    }
}
