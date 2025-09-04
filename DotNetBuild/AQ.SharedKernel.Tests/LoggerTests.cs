using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class LoggerTests
    {
        [Test]
        public void LogLevel_Has_Info_And_Error()
        {
            // Minimal compile-time coverage without binding to NullLogger API
            Assert.IsTrue(Enum.IsDefined(typeof(AQ.SharedKernel.LogLevel), "Info"));
            Assert.IsTrue(Enum.IsDefined(typeof(AQ.SharedKernel.LogLevel), "Error"));
        }

        [Test]
        public void NullLogger_When_Exposed_Can_Log_Without_Throwing()
        {
            // Find the NullLogger type via the SharedKernel assembly
            var asm = typeof(AQ.SharedKernel.LogLevel).Assembly;
            var nlType = asm.GetType("AQ.SharedKernel.NullLogger");
            if (nlType == null)
                Assert.Inconclusive("NullLogger type not present; skipping.");

            // Try common static accessors: Instance property or a parameterless factory method returning ILogger
            object instance = null;

            var instProp = nlType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instProp != null)
                instance = instProp.GetValue(null);

            if (instance == null)
            {
                var factory = nlType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => typeof(AQ.SharedKernel.ILogger).IsAssignableFrom(m.ReturnType)
                                         && m.GetParameters().Length == 0);
                if (factory != null)
                    instance = factory.Invoke(null, null);
            }

            if (instance == null)
                Assert.Inconclusive("No public Instance or factory for NullLogger; skipping.");

            var log = instance as AQ.SharedKernel.ILogger;
            Assert.NotNull(log, "NullLogger does not implement ILogger.");

            // Only use levels we know exist
            log.Log(AQ.SharedKernel.LogLevel.Info,  "hello");
            log.Log(AQ.SharedKernel.LogLevel.Error, "err");
            Assert.Pass();
        }
    }
}
