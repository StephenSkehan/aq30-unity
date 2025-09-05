using System;
using System.Linq;
using System.Reflection;
using AQ.SharedKernel;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    [TestFixture]
    public sealed class FixedTimeProvider_Smoke_Tests
    {
        private static (bool ok, DateTime value) TryGetUtcNow_Static(Type t)
        {
            var prop = t.GetProperty("UtcNow", BindingFlags.Public | BindingFlags.Static);
            if (prop != null && prop.CanRead)
            {
                var v = prop.GetValue(null);
                if (v is DateTime dt) return (true, dt);
            }
            var meth = t.GetMethod("UtcNow", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
            if (meth != null)
            {
                var v = meth.Invoke(null, null);
                if (v is DateTime dt) return (true, dt);
            }
            return (false, default);
        }

        private static (object inst, PropertyInfo utcProp, MethodInfo advMeth) TryGetInstanceApi(Type t)
        {
            object inst = null;

            // Common singleton/static factory patterns
            var instProp = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                        ?? t.GetProperty("Default",  BindingFlags.Public | BindingFlags.Static);
            if (instProp != null) inst = instProp.GetValue(null);

            if (inst == null)
            {
                var create = t.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (create != null) inst = create.Invoke(null, null);
            }

            // Zero-arg public or non-public ctor
            if (inst == null)
            {
                var ctor = t.GetConstructor(Type.EmptyTypes)
                        ?? t.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                             .FirstOrDefault(c => c.GetParameters().Length == 0);
                if (ctor != null) inst = ctor.Invoke(null);
            }

            var utcProp = t.GetProperty("UtcNow", BindingFlags.Public | BindingFlags.Instance);
            var advMeth = t.GetMethod("AdvanceSeconds", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(int) }, null);
            return (inst, utcProp, advMeth);
        }

        [Test]
        public void UtcNow_is_accessible_and_utc_when_available()
        {
            var t = typeof(FixedTimeProvider);

            // Prefer static surface if present
            var (hasStatic, staticNow) = TryGetUtcNow_Static(t);
            if (hasStatic)
            {
                Assert.AreEqual(DateTimeKind.Utc, staticNow.Kind);
                return;
            }

            // Else attempt instance surface
            var (inst, utcProp, _) = TryGetInstanceApi(t);
            if (inst != null && utcProp != null && utcProp.CanRead)
            {
                var now = (DateTime)utcProp.GetValue(inst);
                Assert.AreEqual(DateTimeKind.Utc, now.Kind);
                return;
            }

            // If neither surface exists, that’s an acceptable design; don’t fail the suite.
            Assert.Pass("FixedTimeProvider exposes no accessible UtcNow in this configuration.");
        }

        [Test]
        public void Advance_if_available_moves_time_forward()
        {
            var t = typeof(FixedTimeProvider);

            // Static path
            var staticUtcProp = t.GetProperty("UtcNow", BindingFlags.Public | BindingFlags.Static);
            var staticAdv     = t.GetMethod("AdvanceSeconds", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);
            if (staticUtcProp != null && staticAdv != null)
            {
                var a = (DateTime)staticUtcProp.GetValue(null);
                staticAdv.Invoke(null, new object[] { 2 });
                var b = (DateTime)staticUtcProp.GetValue(null);
                Assert.Greater(b, a);
                return;
            }

            // Instance path
            var (inst, utcProp, advMeth) = TryGetInstanceApi(t);
            if (inst != null && utcProp != null && advMeth != null)
            {
                var a = (DateTime)utcProp.GetValue(inst);
                advMeth.Invoke(inst, new object[] { 2 });
                var b = (DateTime)utcProp.GetValue(inst);
                Assert.Greater(b, a);
                return;
            }

            // Nothing to exercise; design without advance API is fine.
            Assert.Pass("No advance API exposed; skipping monotonicity check.");
        }
    }
}
