using UnityEditor;
using UnityEngine;
using AQ.App.Items;

namespace AQ.Editor.Items
{
    public static class WireItemRegistry
    {
        [MenuItem("AQ/Wire Item Registry in Scene")]
        public static void Wire()
        {
            var registry = Object.FindAnyObjectByType<ItemRegistry>();
            if (registry == null)
            {
                Debug.LogError("[WireItemRegistry] No ItemRegistry found in scene.");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:ItemDefinitionSO");
            var defs = new ItemDefinitionSO[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                defs[i] = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(path);
            }

            var so = new SerializedObject(registry);
            var prop = so.FindProperty("_items");
            prop.arraySize = defs.Length;
            for (int i = 0; i < defs.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = defs[i];
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(registry);
            Debug.Log($"[WireItemRegistry] Assigned {defs.Length} ItemDefinitionSOs to ItemRegistry.");
        }
    }
}
