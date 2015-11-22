﻿using Akka.Actor;
using Even.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Even
{
    public class ProjectionStreamSupervisor : ReceiveActor
    {
        public static Props CreateProps(IActorRef reader, IActorRef writer, GlobalOptions options)
        {
            return Props.Create<ProjectionStreamSupervisor>(reader, writer, options);
        }

        public ProjectionStreamSupervisor(IActorRef reader, IActorRef writer, GlobalOptions options)
        {
            Argument.RequiresNotNull(reader, nameof(reader));
            Argument.RequiresNotNull(writer, nameof(writer));
            Argument.RequiresNotNull(options, nameof(options));

            _reader = reader;
            _writer = writer;
            _options = options;

            Become(ReceivingRequests);
        }

        IActorRef _reader;
        IActorRef _writer;
        GlobalOptions _options;

        private void ReceivingRequests()
        {
            Receive<ProjectionSubscriptionRequest>(ps =>
            {
                var key = "stream-" + ps.Query.ProjectionStream;
                IActorRef pRef = Context.Child(key);

                // if the projection stream doesn't exist, start one
                if (pRef == ActorRefs.Nobody)
                {
                    var props = ProjectionStream.CreateProps(ps.Query, _reader, _writer, _options);
                    pRef = Context.ActorOf(props);
                }

                pRef.Forward(ps);
            });
        }
    }
}
