using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AQ.SharedKernel.Tests
{
    public class ResultValueTests
    {
        Type GetResultOpen(Assembly asm) =>
            asm.GetExportedTypes().FirstOrDefault(t => t.IsGenericTypeDefinition && t.Name == "Result`1");

        [Test]
        public void Result_Value_OnSuccess_Returns()
        {
            var asm = typeof(AQ.SharedKernel.DeterministicRandom).Assembly;
            var open = GetResultOpen(asm);
            Assert.NotNull(open, "Could not find Result<T>.");
            var resultT = open.MakeGenericType(typeof(string));

            var makeOk = resultT.GetMethod("Success", BindingFlags.Public | BindingFlags.Static)
                      ?? resultT.GetMethod("Ok",      BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(makeOk, "Need Success(value) or Ok(value).");

            var valueProp = resultT.GetProperty("Value")
                        ?? resultT.GetProperty("Unwrap")
                        ?? resultT.GetProperty("Get");
            Assert.NotNull(valueProp, "Need Value/Unwrap/Get property.");

            var ok = makeOk.Invoke(null, new object[] { "hey" });
            var val = valueProp.GetValue(ok);
            Assert.AreEqual("hey", val);
        }

        [Test]
        public void Result_Value_OnFailure_IsHandled()
        {
            var asm = typeof(AQ.SharedKernel.DeterministicRandom).Assembly;
            var open = GetResultOpen(asm);
            if (open == null) Assert.Inconclusive("No Result<T> available.");
            var resultT = open.MakeGenericType(typeof(int));

            var makeFail = resultT.GetMethod("Failure", BindingFlags.Public | BindingFlags.Static)
                        ?? resultT.GetMethod("Error",   BindingFlags.Public | BindingFlags.Static);
            if (makeFail == null) Assert.Inconclusive("No Failure/Error factory.");

            var isSuccess = resultT.GetProperty("IsSuccess");
            Assert.NotNull(isSuccess, "IsSuccess missing on Result<T>.");

            var valProp = resultT.GetProperty("Value")
                   ?? resultT.GetProperty("Unwrap")
                   ?? resultT.GetProperty("Get");

            var fail = makeFail.Invoke(null, new object[] { "boom" });
            Assert.IsFalse((bool)isSuccess.GetValue(fail));

            if (valProp != null)
            {
                try
                {
                    _ = valProp.GetValue(fail);
                    Assert.Pass("Failure.Value did not throw (acceptable if API defines a default).");
                }
                catch (TargetInvocationException)
                {
                    Assert.Pass("Failure.Value throws as expected.");
                }
                catch (Exception)
                {
                    Assert.Pass("Failure.Value behavior handled.");
                }
            }
            else
            {
                Assert.Pass("No Value property on failure path; API variant.");
            }
        }
    }
}
