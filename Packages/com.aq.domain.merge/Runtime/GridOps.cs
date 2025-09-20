using System;
using System.Reflection;

namespace AQ.Domain.Merge
{
    internal static class GridOps
    {
        public static ItemId GetCell(IGrid grid, int index)
        {
            var t = grid.GetType();

            // Indexer: Item[int]
            var idx = t.GetProperty("Item", new[] { typeof(int) });
            if (idx != null && idx.CanRead)
                return (ItemId)idx.GetValue(grid, new object[] { index });

            // Methods: Get / GetAt / Read
            var m = t.GetMethod("Get", new[] { typeof(int) })
                 ?? t.GetMethod("GetAt", new[] { typeof(int) })
                 ?? t.GetMethod("Read", new[] { typeof(int) });
            if (m != null)
                return (ItemId)m.Invoke(grid, new object[] { index });

            // Fallback: any method returning ItemId with (int)
            foreach (var mi in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var p = mi.GetParameters();
                if (p.Length == 1 && p[0].ParameterType == typeof(int) && mi.ReturnType == typeof(ItemId))
                    return (ItemId)mi.Invoke(grid, new object[] { index });
            }

            return default;
        }

        public static void SetCell(IGrid grid, int index, ItemId value)
        {
            var t = grid.GetType();

            var idx = t.GetProperty("Item", new[] { typeof(int) });
            if (idx != null && idx.CanWrite)
            {
                idx.SetValue(grid, value, new object[] { index });
                return;
            }

            var m = t.GetMethod("Set", new[] { typeof(int), typeof(ItemId) })
                 ?? t.GetMethod("SetAt", new[] { typeof(int), typeof(ItemId) })
                 ?? t.GetMethod("Write", new[] { typeof(int), typeof(ItemId) });
            if (m != null)
            {
                m.Invoke(grid, new object[] { index, value });
                return;
            }

            foreach (var mi in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                var p = mi.GetParameters();
                if (p.Length == 2 && p[0].ParameterType == typeof(int) && p[1].ParameterType == typeof(ItemId) && mi.ReturnType == typeof(void))
                {
                    mi.Invoke(grid, new object[] { index, value });
                    return;
                }
            }
        }
    }
}
