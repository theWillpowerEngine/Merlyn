using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    Interpreter.Error("Bizarre attempt to unsubscribe from queue: " + queue + " in a situation where that queue doesn't exist.  I fucked up somewhere this is not your fault.");

            lock (Queues[queue.ToLower()].Lock)
            {
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

            lock (Queues[queue.ToLower()].Lock)
            {
                foreach (var sub in Queues[queue.ToLower()].Subscriptions)
                {
                    Guid letId = Guid.NewGuid();

                    sub.Shiro.Symbols.Let("val", toke.Clone(), letId);
                    var res = sub.List.Eval(sub.Shiro);

                    if(awaitDelivery != null && awaitDeliveryInterpreter != null)
                        awaitDeliveryInterpreter.Symbols.Deliver(awaitDelivery, res.Clone());

                    sub.Shiro.Symbols.ClearLetId(letId);
                }
            }
        }

        internal bool HasQueue(string queue)
        {
            lock(QueueLock)
                return Queues.ContainsKey(queue.ToLower());
        }
    }
}
