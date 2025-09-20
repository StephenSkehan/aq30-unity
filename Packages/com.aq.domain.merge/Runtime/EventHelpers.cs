using System;
using System.Linq;
using System.Reflection;
using AQ.SharedKernel; // ItemId, IEventBus (shim interface)

namespace AQ.Domain.Merge
{
    internal static class EventHelpers
    {
        /// <summary>
        /// Best-effort creation of AQ.Domain.Merge.MergePerformed with either:
        ///   ctor(int fromIndex, int toIndex, ItemId left, ItemId right, ItemId result)
        /// or default + settable members. Accepts synonyms (left/a, right/b, fromIndex/from, toIndex/to).
        /// </summary>
        public static object TryCreateMergePerformed(int fromIndex, int toIndex, ItemId left, ItemId right, ItemId result)
        {
            var t = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(x => x.Name == "MergePerformed" && x.Namespace == "AQ.Domain.Merge");

            if (t == null) return null;

            // Prefer the extended signature if present
            var sig = new[] { typeof(int), typeof(int), typeof(ItemId), typeof(ItemId), typeof(ItemId) };
            var ctor = t.GetConstructor(sig);
            if (ctor != null)
                return ctor.Invoke(new object[] { fromIndex, toIndex, left, right, result });

            // Fallback: default + settable members
            var obj = Activator.CreateInstance(t);
            if (obj == null) return null;

            void SetMany(string[] names, object val)
            {
                foreach (var name in names)
                {
                    var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (pi != null && pi.CanWrite) { pi.SetValue(obj, val); return; }
                    var fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (fi != null) { fi.SetValue(obj, val); return; }
                }
            }

            SetMany(new[] { "fromIndex", "from", "sourceIndex" }, fromIndex);
            SetMany(new[] { "toIndex", "to", "targetIndex" }, toIndex);
            SetMany(new[] { "left", "a" }, left);
            SetMany(new[] { "right", "b" }, right);
            SetMany(new[] { "result" }, result);
            return obj;
        }

        /// <summary>
        /// Reflects over the *runtime* bus type to locate Publish<T>(T) and invokes it with evt.
        /// Works with spies, shims, and canonical buses.
        /// </summary>
        public static void PublishDynamic(IEventBus bus, object evt)
        {
            if (bus == null || evt == null) return;

            var busType = bus.GetType();
            var evtType = evt.GetType();

            var publishGen = busType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m =>
                    m.Name == "Publish" &&
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 1);

            if (publishGen == null) return;

            try
            {
                var closed = publishGen.MakeGenericMethod(evtType);
                var pars = closed.GetParameters();
                if (pars.Length == 1 && pars[0].ParameterType.IsAssignableFrom(evtType))
                    closed.Invoke(bus, new object[] { evt });
            }
            catch
            {
                // Swallow: publishing is best-effort in domain helpers.
            }
        }
    }
}