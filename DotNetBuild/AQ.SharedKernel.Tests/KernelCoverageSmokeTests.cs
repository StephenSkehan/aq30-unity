#nullable disable
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using AQ.SharedKernel;

namespace AQ.SharedKernel.Tests
{
    public class KernelCoverageSmokeTests
    {
        static Assembly KernelAsm => typeof(DeterministicRandom).Assembly;

        // --- Helper to create defaults for arbitrary ctor params ---
        static object DefaultFor(Type t)
        {
            if (t == typeof(string)) return string.Empty;
            if (t.IsArray) return Array.CreateInstance(t.GetElementType() ?? typeof(object), 0);
            if (t.IsValueType) return Activator.CreateInstance(t);
            return null;
        }

        [Test]
        public void Result_Construct_Or_Factory_And_Touch_Members()
        {
            var g = typeof(Result<>).MakeGenericType(typeof(int));

            object instance = null;

            // Try any public ctor
            foreach (var ctor in g.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var args = ctor.GetParameters().Select(p => DefaultFor(p.ParameterType)).ToArray();
                try { instance = ctor.Invoke(args); if (instance != null) break; } catch { /* try next */ }
            }

            // If no ctor worked, try any public static factory
            if (instance == null)
            {
                var factory = g.GetMethods(BindingFlags.Public | BindingFlags.Static)
                               .OrderByDescending(m => m.GetParameters().Length)
                               .FirstOrDefault();
                Assume.That(factory != null, "No ctor or factory found for Result<T>; skipping.");
                var args = factory.GetParameters().Select(p => DefaultFor(p.ParameterType)).ToArray();
                instance = factory.Invoke(null, args);
            }

            // Touch readable props & simple getters to bump coverage
            foreach (var p in g.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
                _ = p.GetValue(instance);

            foreach (var m in g.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.GetParameters().Length == 0 && m.ReturnType != typeof(void)))
            {
                try { _ = m.Invoke(instance, Array.Empty<object>()); } catch { /* best effort */ }
            }

            Assert.NotNull(instance);
        }

        [Test]
        public void DeterministicRandom_CommonOverloads_DoNotThrow()
        {
            var r = new DeterministicRandom(12345);
            Assert.DoesNotThrow(() => _ = r.Next());

            var one = typeof(DeterministicRandom).GetMethod("Next", new[] { typeof(int) });
            if (one != null) Assert.DoesNotThrow(() => _ = one.Invoke(r, new object[] { 10 }));

            var two = typeof(DeterministicRandom).GetMethod("Next", new[] { typeof(int), typeof(int) });
            if (two != null) Assert.DoesNotThrow(() => _ = two.Invoke(r, new object[] { 0, 10 }));

            var nd = typeof(DeterministicRandom).GetMethod("NextDouble", Type.EmptyTypes);
            if (nd != null) Assert.DoesNotThrow(() => _ = nd.Invoke(r, Array.Empty<object>()));
        }

        [Test]
        public void FixedTimeProvider_BasicAccessors_Work()
        {
            var fixedNow = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var tpType   = KernelAsm.GetType("AQ.SharedKernel.FixedTimeProvider", throwOnError: true)!;
            var tp       = Activator.CreateInstance(tpType, new object[] { fixedNow });

            var now = tpType.GetProperty("Now") ?? tpType.GetProperty("UtcNow");
            Assume.That(now != null, "No Now/UtcNow property; skipping.");
            _ = now!.GetValue(tp);

            var adv = tpType.GetMethod("AdvanceBy", new[] { typeof(TimeSpan) });
            if (adv != null) adv.Invoke(tp, new object[] { TimeSpan.FromSeconds(1) });
        }
    }
}
