using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AQ.App
{
    /// <summary>
    /// PlayMode safety net: if the SharedKernel expects a global/factory ILogger,
    /// seed it with a NullLogger. Reflection avoids hard deps on package internals.
    /// </summary>
    public static class SharedKernelBoot
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureNullLoggerFactory()
        {
            try
            {
                var asm = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "AQ.SharedKernel");
                if (asm == null) return;

                var iLogger = asm.GetType("AQ.SharedKernel.ILogger");
                var nullLoggerType = asm.GetType("AQ.SharedKernel.NullLogger");
                if (iLogger == null || nullLoggerType == null) return;

                var loggerInstance = Activator.CreateInstance(nullLoggerType);

                // Strategy: set any public static property/field of type ILogger.
                foreach (var t in asm.GetTypes())
                {
                    foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (p.CanWrite && p.PropertyType == iLogger)
                        {
                            p.SetValue(null, loggerInstance, null);
                        }
                    }

                    foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (f.FieldType == iLogger)
                        {
                            f.SetValue(null, loggerInstance);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Best-effort only; never fail boot.
            }
        }
    }
}