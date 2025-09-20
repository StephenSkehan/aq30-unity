using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Variants
{
    public static class WK3_2_VariantMenu
    {
        // The class that already exists in your project (contains ApplyVariant*_Menu methods)
        const string VariantType = "AQ.EditorTools.Content.ContentVariant";

        [MenuItem("AQ/WK3-2/Apply Variant A")]
        public static void ApplyA() => TryInvoke(VariantType, "ApplyVariantA_Menu");

        [MenuItem("AQ/WK3-2/Apply Variant B")]
        public static void ApplyB() => TryInvoke(VariantType, "ApplyVariantB_Menu");

        [MenuItem("AQ/WK3-2/Apply Variant C")]
        public static void ApplyC() => TryInvoke(VariantType, "ApplyVariantC_Menu");

        private static void TryInvoke(string typeName, string methodName)
        {
            var t = Type.GetType(typeName);
            var m = t?.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (m != null)
            {
                m.Invoke(null, null);
                Debug.Log($"Variants: Invoked {typeName}.{methodName}()");
            }
            else
            {
                Debug.LogWarning($"Variants: {typeName}.{methodName} not found. " +
                                 $"Open ContentVariant.cs and confirm the static menu method exists.");
            }
        }
    }
}
