using System;
using AQ.SharedKernel;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class InMemoryEventBus_Tests
    {
        private sealed class Ping { public int N; public Ping(int n){ N=n; } }

        [Test]
        public void Subscribe_then_Publish_invokes_handler()
        {
            var bus = new InMemoryEventBus();
            int count = 0;
            using (bus.Subscribe<Ping>(p => { if (p != null) count += p.N; }))
            {
                bus.Publish(new Ping(2));
                bus.Publish(new Ping(3));
            }
            // After dispose, should not receive more
            bus.Publish(new Ping(5));
            Assert.AreEqual(5, count);
        }

        [Test]
        public void Disposed_subscription_is_noop()
        {
            var bus = new InMemoryEventBus();
            int count = 0;
            var sub = bus.Subscribe<Ping>(_ => count++);
            sub.Dispose();
            bus.Publish(new Ping(1));
            Assert.AreEqual(0, count);
        }
    }
}
