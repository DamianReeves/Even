﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Even
{
    public class MessageHandler<TMessage>
    {
        /// <summary>
        /// Creates a new message handler for message of type <typeparamref name="TMessage"/>.
        /// </summary>
        /// <param name="mapper">
        /// Optional. A mapper function that returns the type to match the handler.
        /// If null, the actual message type will be used to find the matching handler.
        /// </param>
        public MessageHandler(Func<TMessage, Type> mapper = null)
        {
            _mapper = mapper;
        }

        Func<TMessage, Type> _mapper;
        class HandlerList : LinkedList<Func<TMessage, Task>> { }

        Dictionary<Type, HandlerList> _handlers = new Dictionary<Type, HandlerList>();

        public async Task Handle(TMessage message)
        {
            if (message == null)
                return;

            Type type;

            if (_mapper != null)
            {
                type = _mapper(message);

                if (type == null)
                    return;
            }
            else
            {
                type = message.GetType();
            }

            HandlerList list;

            if (_handlers.TryGetValue(type, out list))
            {
                foreach (var handler in list)
                    await handler(message);
            }
        }

        public void AddHandler<T>(Func<TMessage, Task> handler)
        {
            HandlerList list;

            if (!_handlers.TryGetValue(typeof(T), out list))
            {
                list = new HandlerList();
                _handlers.Add(typeof(T), list);
            }

            list.AddLast(handler);
        }

        public void AddHandler<T>(Action<TMessage> handler)
        {
            AddHandler<T>(msg =>
            {
                handler(msg);
                return Task.CompletedTask;
            });
        }
    }

    public class PersistedEventHandler : MessageHandler<IPersistedEvent>
    {
        public PersistedEventHandler()
            : base(e => e.DomainEvent?.GetType())
        { }
    }

    public class ObjectHandler : MessageHandler<object>
    { }
}
