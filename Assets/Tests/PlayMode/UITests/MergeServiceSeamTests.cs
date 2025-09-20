using NUnit.Framework;
using UnityEngine;

namespace AQ.App.Tests
{
    public class MergeServiceSeamTests
    {
        [Test]
        public void Can_inject_domain_resolver_and_override_default_behavior()
        {
            // Create correctly as a component
            var svc = TestGame.CreateMergeService();

            // Inject label-based resolver: only merge identical labels, return upgraded label
            svc.InjectDomainResolver((string a, string b) =>
            {
                if (!string.IsNullOrEmpty(a) && a == b) return (true, a + "+");
                return (false, b ?? a ?? string.Empty);
            });

            // Should succeed for identical labels
            string result;
            var ok = svc.TryMerge("A", "A", out result);
            Assert.IsTrue(ok);
            Assert.AreEqual("A+", result);

            // Should fail for different labels
            ok = svc.TryMerge("A", "B", out result);
            Assert.IsFalse(ok);

            Object.DestroyImmediate(svc.gameObject);
        }
    }
}