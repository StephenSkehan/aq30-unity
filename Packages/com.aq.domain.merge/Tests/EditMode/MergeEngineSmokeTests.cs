using System;
using System.Collections.Generic;
using NUnit.Framework;
using AQ.Domain.Merge;
using AQ.SharedKernel;

namespace AQ.Domain.Merge.Tests
{
    public class MergeEngineSmokeTests
    {
        private sealed class StubRecipes : IRecipeBook
        {
            public bool TryGetResult(ItemId a, ItemId b, out ItemId result)
            {
                result = new ItemId("merged");
                return true;
            }
        }

        // Tiny in-memory bus for tests
        private sealed class TestBus : IEventBus
        {
            public readonly List<object> Published = new List<object>();
            private readonly Dictionary<Type, List<Delegate>> _subs = new Dictionary<Type, List<Delegate>>();

            public void Publish<T>(T evt)
            {
                Published.Add(evt!);
                if (_subs.TryGetValue(typeof(T), out var handlers))
                {
                    // iterate a copy to tolerate unsubscription during publish
                    foreach (var h in handlers.ToArray())
                        ((Action<T>)h)(evt!);
                }
            }

            public IDisposable Subscribe<T>(Action<T> handler)
            {
                if (handler == null) return new ActionDisposable(null);

                var key = typeof(T);
                if (!_subs.TryGetValue(key, out var list))
                {
                    list = new List<Delegate>();
                    _subs[key] = list;
                }
                list.Add(handler);

                // return unsubscriber
                return new ActionDisposable(() =>
                {
                    if (_subs.TryGetValue(key, out var l))
                    {
                        l.Remove(handler);
                        if (l.Count == 0) _subs.Remove(key);
                    }
                });
            }

            private sealed class ActionDisposable : IDisposable
            {
                private Action _dispose;
                public ActionDisposable(Action dispose) { _dispose = dispose; }
                public void Dispose()
                {
                    var d = _dispose;
                    if (d != null)
                    {
                        _dispose = null;
                        d();
                    }
                }
            }
        }

        [Test]
        public void Construct_And_TryMerge_DoesNotThrow()
        {
            var bus = new TestBus();
            var engine = new MergeEngine(new StubRecipes(), bus);

            ItemId result;
            Assert.DoesNotThrow(() => engine.TryMerge(new ItemId("a"), new ItemId("b"), out result));

            // prove something was published
            Assert.GreaterOrEqual(bus.Published.Count, 1);
            // Assert.IsTrue(bus.Published.Exists(e => e is MergePerformed));
        }
    }
}
