using System;
using System.Linq;
using System.Reflection;
using AQ.SharedKernel;

namespace AQ.Domain.Merge
{
    internal static class EventHelpers
    {
        public static object TryCreateMergePerformed(int fromIndex, int toIndex, ItemId left, ItemId right, ItemId result)
        {
            var t = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(x => x.Name == "MergePerformed" && x.Namespace == "AQ.Domain.Merge");

            if (t == null) return null;

            var sig = new[] { typeof(int), typeof(int), typeof(ItemId), typeof(ItemId), typeof(ItemId) };
            var ctor = t.GetConstructor(sig);
            if (ctor != null)
                return ctor.Invoke(new object[] { fromIndex, toIndex, left, right, result });

            var obj = Activator.CreateInstance(t);
            if (obj == null) return null;

            void Set(string name, object val)
            {
                var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (pi != null && pi.CanWrite) pi.SetValue(obj, val);
            }

            Set("fromIndex", fromIndex);
            Set("toIndex", toIndex);
            Set("left", left);
            Set("right", right);
            Set("result", result);
            return obj;
        }

        public static void PublishDynamic(IEventBus bus, object evt)
        {
            if (bus == null || evt == null) return;
            var m = typeof(IEventBus).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                     .FirstOrDefault(mi => mi.Name == "Publish" && mi.IsGenericMethodDefinition);
            if (m == null) return;
            var gm = m.MakeGenericMethod(evt.GetType());
            gm.Invoke(bus, new object[] { evt });
        }
    }
}
