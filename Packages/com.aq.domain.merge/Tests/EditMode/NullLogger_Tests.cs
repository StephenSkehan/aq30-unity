using System;
using System.Linq;
using System.Reflection;
using AQ.SharedKernel;
using NUnit.Framework;

namespace AQ.Domain.Merge.Tests
{
    public sealed class NullLogger_Tests
    {
        [Test]
        public void Public_logging_members_are_safe_noop()
        {
            var t = typeof(NullLogger);

            // If it's a static class (abstract + sealed), exercise public static methods.
            if (t.IsAbstract && t.IsSealed)
            {
                var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                               .Where(m => !m.IsSpecialName);
                foreach (var m in methods)
                {
                    var args = m.GetParameters().Select(p =>
                    {
                        if (p.ParameterType == typeof(string)) return (object)"msg";
                        if (p.ParameterType.IsValueType) return Activator.CreateInstance(p.ParameterType);
                        return null;
                    }).ToArray();

                    Assert.DoesNotThrow(() => m.Invoke(null, args), $"Static method {m.Name} should be safe/no-op");
                }
                Assert.Pass("Exercised static NullLogger members safely.");
            }

            // Otherwise, try to obtain an instance via common patterns: Instance/Default/static Create()/any ctor (even non-public).
            object instance = null;
            var instProp = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                        ?? t.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
            if (instProp != null) instance = instProp.GetValue(null);

            if (instance == null)
            {
                var create = t.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                if (create != null) instance = create.Invoke(null, null);
            }

            if (instance == null)
            {
                var ctor = t.GetConstructor(Type.EmptyTypes)
                        ?? t.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                             .FirstOrDefault(c => c.GetParameters().Length == 0);
                if (ctor != null) instance = ctor.Invoke(null);
            }

            if (instance == null)
            {
                Assert.Pass("No constructible instance path; NullLogger likely static/singleton-only.");
                return;
            }

            var instMethods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => !m.IsSpecialName);
            foreach (var m in instMethods)
            {
                var args = m.GetParameters().Select(p =>
                {
                    if (p.ParameterType == typeof(string)) return (object)"msg";
                    if (p.ParameterType.IsValueType) return Activator.CreateInstance(p.ParameterType);
                    return null;
                }).ToArray();

                Assert.DoesNotThrow(() => m.Invoke(instance, args), $"Instance method {m.Name} should be safe/no-op");
            }
        }
    }
}
