using System.Collections.Generic;
using UnityEngine;

namespace AQ.EditorTools.Util
{
    /// <summary>
    /// Helpers for getting a nice "Hierarchy/Path/To/Object" string for a Transform.
    /// We provide both static and extension forms to be compatible with older calls.
    /// </summary>
    public static class TransformPathExt
    {
        /// <summary>
        /// Static helper to match existing calls: TransformPathExt.Path(t)
        /// </summary>
        public static string Path(Transform t) => GetPath(t);

        /// <summary>
        /// Static helper alias some scripts might use.
        /// </summary>
        public static string GetPath(Transform t)
        {
            if (t == null) return "<null>";
            var stack = new List<string>(8);
            var cur = t;
            while (cur != null)
            {
                stack.Add(cur.name);
                cur = cur.parent;
            }
            stack.Reverse();
            return string.Join("/", stack);
        }

        /// <summary>
        /// Extension form: t.ToPath()
        /// </summary>
        public static string ToPath(this Transform t) => GetPath(t);
    }
}
