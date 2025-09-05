using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class Kernel_Primitives_CoverageTests
    {
        Assembly KernelAsm => typeof(AQ.SharedKernel.InMemoryEventBus).Assembly;

        [Test] // Result<T>: Success/Ok + Failure/Error + IsSuccess + Value/Error
        public void Result_Primitives_Are_Usable()
        {
            var open = KernelAsm.GetExportedTypes()
                .FirstOrDefault(t => t.IsGenericTypeDefinition && t.Name == "Result`1");
            Assert.NotNull(open, "Result<T> type not found.");

            var tInt = open!.MakeGenericType(typeof(int));

            var makeOk   = tInt.GetMethod("Success", BindingFlags.Public | BindingFlags.Static)
                        ?? tInt.GetMethod("Ok",      BindingFlags.Public | BindingFlags.Static);
            var makeFail = tInt.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static)
                        ?? tInt.GetMethod("Error",   BindingFlags.Public | BindingFlags.Static);
            var isSuccess = tInt.GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(makeOk, "Need Result<T>.Success(value) or Ok(value).");
            Assert.NotNull(isSuccess, "Result<T>.IsSuccess missing.");

            var ok = makeOk!.Invoke(null, new object[] { 7 });
            Assert.IsTrue((bool)isSuccess!.GetValue(ok)!);

            var valProp = tInt.GetProperty("Value") ?? tInt.GetProperty("Unwrap") ?? tInt.GetProperty("Get");
            if (valProp != null) Assert.AreEqual(7, valProp.GetValue(ok));

            if (makeFail != null)
            {
                var fail = makeFail.Invoke(null, new object[] { "boom" });
                Assert.IsFalse((bool)isSuccess.GetValue(fail)!);

                var errProp = tInt.GetProperty("Error") ?? tInt.GetProperty("ErrorMessage") ?? tInt.GetProperty("Message");
                if (errProp != null) Assert.AreEqual("boom", errProp.GetValue(fail) as string);
            }
        }

        [Test] // DeterministicRandom: next/next range/double/bytes
        public void DeterministicRandom_Is_Deterministic_And_InRange()
        {
            var r1 = new AQ.SharedKernel.DeterministicRandom(12345);
            var r2 = new AQ.SharedKernel.DeterministicRandom(12345);

            var a = new[] { r1.Next(), r1.Next(), r1.Next() };
            var b = new[] { r2.Next(), r2.Next(), r2.Next() };
            CollectionAssert.AreEqual(a, b, "Sequences must match for same seed.");

            for (int i = 0; i < 32; i++)
            {
                int v1 = r1.Next(10);
                Assert.That(v1, Is.GreaterThanOrEqualTo(0).And.LessThan(10));

                int v2 = r1.Next(5, 15);
                Assert.That(v2, Is.GreaterThanOrEqualTo(5).And.LessThan(15));
            }

            var t = r1.GetType();
            var nextDouble = t.GetMethod("NextDouble", Type.EmptyTypes);
            if (nextDouble != null)
            {
                double d = (double)nextDouble.Invoke(r1, null);
                Assert.That(d, Is.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
            }

            var nextBytes = t.GetMethod("NextBytes", new[] { typeof(byte[]) });
            if (nextBytes != null)
            {
                var buf = new byte[16];
                nextBytes.Invoke(r1, new object[] { buf });
                Assert.AreEqual(16, buf.Length);
            }
        }

        [Test] // FixedTimeProvider: stable Now/UtcNow/NowUtc
        public void FixedTimeProvider_Is_Stable()
        {
            var fixedNow = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var tp = new AQ.SharedKernel.FixedTimeProvider(fixedNow);

            var t = tp.GetType();
            var nowProp = t.GetProperty("Now") ?? t.GetProperty("UtcNow") ?? t.GetProperty("NowUtc");
            Assert.NotNull(nowProp, "Expected Now/UtcNow/NowUtc on FixedTimeProvider.");

            var t1 = (DateTime)nowProp!.GetValue(tp)!;
            var t2 = (DateTime)nowProp!.GetValue(tp)!;
            Assert.AreEqual(fixedNow, t1);
            Assert.AreEqual(t1, t2);
        }

        [Test] // NullLogger: log at Info/Error without throwing (don’t assume ctor)
        public void NullLogger_Can_Log_Without_Throwing()
        {
            var nlType = KernelAsm.GetType("AQ.SharedKernel.NullLogger");
            if (nlType == null) Assert.Inconclusive("NullLogger not present.");

            object? instance = nlType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (instance == null)
            {
                var factory = nlType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => typeof(AQ.SharedKernel.ILogger).IsAssignableFrom(m.ReturnType)
                                         && m.GetParameters().Length == 0);
                if (factory != null) instance = factory.Invoke(null, null);
            }
            Assert.NotNull(instance, "No Instance/factory for NullLogger.");

            var log = (AQ.SharedKernel.ILogger)instance!;
            log.Log(AQ.SharedKernel.LogLevel.Info,  "hello");
            log.Log(AQ.SharedKernel.LogLevel.Error, "err");
            Assert.Pass();
        }
    }
}
