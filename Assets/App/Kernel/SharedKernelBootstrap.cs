using System;
using System.Reflection;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// Some package tests expect a default NullLogger instance/factory to exist.
    /// We defensively ensure a singleton instance via reflection before any scene loads.
    /// </summary>
    public static class SharedKernelBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureNullLogger()
        {
            // Try a few likely assembly/type names without taking a hard compile-time dependency.
            TrySetInstance("AQ.SharedKernel.NullLogger",           "com.aq.sharedkernel");
            TrySetInstance("AQ.SharedKernel.Logging.NullLogger",   "com.aq.sharedkernel");
            TrySetInstance("AQ.SharedKernel.NullLogger",           "AQ.SharedKernel");
            TrySetInstance("AQ.SharedKernel.Logging.NullLogger",   "AQ.SharedKernel");
        }

        private static void TrySetInstance(string typeName, string asm)
        {
            try
            {
                var t = Type.GetType(typeName + ", " + asm, throwOnError: false);
                if (t == null) return;

                var prop = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (prop == null) return;

                var current = prop.GetValue(null, null);
                if (current != null) return;

                var obj = Activator.CreateInstance(t, nonPublic: true);
                prop.SetValue(null, obj, null);
            }
            catch (Exception e)
            {
                Debug.Log($"SharedKernelBootstrap: {e.Message}");
            }
        }
    }
}