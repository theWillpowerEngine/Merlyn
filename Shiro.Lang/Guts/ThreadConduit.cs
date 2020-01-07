using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shiro.Guts
{
    internal class ThreadConduit
    {
        private object QueueLock = new object();

        private class QueueItem
        {
            internal List<Subscription> Subscriptions = new List<Subscription>();
            internal object Lock = new object();
        }

        private class Subscription
        {
            internal Token List;
            internal Interpreter Shiro;
            internal Guid SubscriptionId = Guid.NewGuid();

            internal Guid InterpreterId => Shiro.InterpreterId;

            internal Subscription(Interpreter i, Token t)
            {
                Shiro = i;
                List = t;
            }
        }

        private readonly Dictionary<string, QueueItem> Queues = new Dictionary<string, QueueItem>();

        internal Guid Subscribe(string queue, Interpreter i, Token list)
        {
            lock(QueueLock)
                if (!Queues.ContainsKey(queue.ToLower()))
                    Queues.Add(queue.ToLower(), new QueueItem());

            var sub = new Subscription(i, list);
            lock (Queues[queue.ToLower()].Lock)
            {
                Queues[queue.ToLower()].Subscriptions.Add(sub);
            }
            return sub.SubscriptionId;
        }

        internal void Unsubscribe(string queue, Guid subId)
        {
            lock (QueueLock)
                if (!Queues.ContainsKey(queue.ToLower()))
                    return;

            lock (Queues[queue.ToLower()].Lock)
            {
                if(Queues[queue.ToLower()].Subscriptions.Any(s => s.SubscriptionId == subId))
                    Queues[queue.ToLower()].Subscriptions.Remove(Queues[queue.ToLower()].Subscriptions.First(s => s.SubscriptionId == subId));
            }

            if (Queues[queue.ToLower()].Subscriptions.Count == 0)
                lock (QueueLock)
                    Queues.Remove(queue.ToLower());
        }

        internal void Publish(string queue, Token toke, string awaitDelivery = null, Interpreter awaitDeliveryInterpreter = null)
        {
            lock(QueueLock)
                if (!Queues.ContainsKey(queue.ToLower()))
                    Interpreter.Error("No one is subscribed to the queue '" + queue + "'.  You might have to give your async-list more time to get setup, or else use the queue? predicate.");

            List<Subscription> subs = null;

            lock (Queues[queue.ToLower()].Lock)
                subs = Queues[queue.ToLower()].Subscriptions.Select(s => s).ToList();

            foreach (var sub in subs)
            {
                Guid letId = Guid.NewGuid();

                sub.Shiro.Symbols.Let("val", toke.Clone(), letId);
                var res = sub.List.Eval(sub.Shiro, true);

                if (sub.Shiro.Symbols == null)
                    Unsubscribe(queue, sub.SubscriptionId);
                else
                    sub.Shiro.Symbols.ClearLetId(letId);

                if (awaitDelivery != null && awaitDeliveryInterpreter != null)
                    awaitDeliveryInterpreter.Symbols.Deliver(awaitDelivery, res.Clone());
            }
        }

        internal bool HasQueue(string queue)
        {
            lock(QueueLock)
                return Queues.ContainsKey(queue.ToLower());
        }
    }
}
