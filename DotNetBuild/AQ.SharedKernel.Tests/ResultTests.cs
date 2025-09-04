using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class ResultTests
    {
        [Test]
        public void Result_Success_And_Failure_Work_When_Available()
        {
            // Load the SharedKernel assembly we already reference
            var asm = typeof(AQ.SharedKernel.DeterministicRandom).Assembly;

            // Find a generic type like Result`1
            var resultOpen = asm.GetExportedTypes()
                .FirstOrDefault(t => t.IsGenericTypeDefinition && t.Name.Equals("Result`1", StringComparison.Ordinal));
            Assert.NotNull(resultOpen, "Could not find generic type Result<T> in SharedKernel.");

            var resultType = resultOpen!.MakeGenericType(typeof(int));

            // Try common factory names
            var makeOk   = resultType.GetMethod("Success", BindingFlags.Public | BindingFlags.Static)
                         ?? resultType.GetMethod("Ok",      BindingFlags.Public | BindingFlags.Static);
            var makeFail = resultType.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static)
                         ?? resultType.GetMethod("Error",   BindingFlags.Public | BindingFlags.Static);

            var isSuccessProp = resultType.GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(isSuccessProp, "Result<T> needs IsSuccess property.");

            // Success path (required)
            Assert.NotNull(makeOk, "Need static Result<T>.Success(value) or Ok(value).");
            var ok = makeOk!.Invoke(null, new object[] { 42 });
            Assert.IsTrue((bool)isSuccessProp!.GetValue(ok)!);

            // Failure path (best effort)
            if (makeFail != null)
            {
                var fail = makeFail.Invoke(null, new object[] { "boom" });
                Assert.IsFalse((bool)isSuccessProp.GetValue(fail)!);

                var errProp = resultType.GetProperty("Error")
                           ?? resultType.GetProperty("ErrorMessage")
                           ?? resultType.GetProperty("Message");
                if (errProp != null)
                {
                    var msg = errProp.GetValue(fail) as string;
                    Assert.AreEqual("boom", msg);
                }
            }
        }
    }
}
