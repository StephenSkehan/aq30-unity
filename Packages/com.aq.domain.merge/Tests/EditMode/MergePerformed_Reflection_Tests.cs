using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class MergePerformed_Reflection_Tests
    {
        private static ItemId Id(string s) => new ItemId(s);

        [Test]
        public void Can_construct_and_inspect_MergePerformed()
        {
            var type = typeof(MergeEngine).Assembly.GetTypes()
                         .FirstOrDefault(t => t.Name == "MergePerformed");
            Assert.IsNotNull(type, "MergePerformed type not found in AQ.Domain.Merge");

            // Find a public ctor with 3–5 parameters
            var ctor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                           .OrderBy(c => c.GetParameters().Length)
                           .FirstOrDefault();
            Assert.IsNotNull(ctor, "No public constructor found for MergePerformed");

            var parms = ctor.GetParameters();
            object[] args = parms.Select(p =>
            {
                if (p.ParameterType == typeof(int)) return (object)0;
                if (p.ParameterType == typeof(ItemId)) return (object)Id(p.Name);
                return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
            }).ToArray();

            var evt = ctor.Invoke(args);
            Assert.IsNotNull(evt);

            // Check ToString works
            Assert.IsNotEmpty(evt.ToString());

            // Probe any ItemId/int members
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var val = p.GetValue(evt);
                if (val is ItemId id) Assert.IsNotNull(id.ToString());
                if (val is int i) Assert.AreEqual(0, i);
            }
        }
    }
}
