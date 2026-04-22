using UnityEditor;
using UnityEngine;
using AQ.App.Items;
using AQ.App.Leads;

namespace AQ.Editor.Items
{
    /// <summary>
    /// Populates ItemRegistry._items from all ItemDefinitionSO assets in the project
    /// and wires MergeEventsBridge + LeadRequirementChecker refs on the same GameObject.
    /// Run via: Tools > AQ > Wire Item Registry In Scene
    /// MergeEventsBridge lives in Assembly-CSharp so we reference it by component name.
    /// </summary>
    public static class ItemRegistryWirer
    {
        [MenuItem("Tools/AQ/Wire Item Registry In Scene")]
        public static void WireRegistry()
        {
            var registry = Object.FindAnyObjectByType<ItemRegistry>();
            if (registry == null) { Debug.LogError("[ItemRegistryWirer] No ItemRegistry found in scene."); return; }

            // ── Populate _items with every ItemDefinitionSO in the project ────
            string[] guids = AssetDatabase.FindAssets("t:ItemDefinitionSO");
            var defs = new ItemDefinitionSO[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                defs[i] = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(AssetDatabase.GUIDToAssetPath(guids[i]));

            var registrySo = new SerializedObject(registry);
            var itemsProp = registrySo.FindProperty("_items");
            itemsProp.arraySize = defs.Length;
            for (int i = 0; i < defs.Length; i++)
                itemsProp.GetArrayElementAtIndex(i).objectReferenceValue = defs[i];
            registrySo.ApplyModifiedProperties();

            // ── Wire MergeEventsBridge._registry (Assembly-CSharp — use name lookup) ──
            foreach (var comp in registry.GetComponents<Component>())
            {
                if (comp == null || comp.GetType().Name != "MergeEventsBridge") continue;
                var bridgeSo = new SerializedObject(comp);
                var regProp = bridgeSo.FindProperty("_registry");
                if (regProp != null) { regProp.objectReferenceValue = registry; bridgeSo.ApplyModifiedProperties(); }
                break;
            }

            // ── Wire LeadRequirementChecker._repository ───────────────────────
            var checker = registry.GetComponent<LeadRequirementChecker>();
            if (checker != null)
            {
                var leadsRepo = Object.FindAnyObjectByType<LeadsRepository>();
                if (leadsRepo != null)
                {
                    var checkerSo = new SerializedObject(checker);
                    checkerSo.FindProperty("_repository").objectReferenceValue = leadsRepo;
                    checkerSo.ApplyModifiedProperties();
                }
                else Debug.LogWarning("[ItemRegistryWirer] LeadsRepository not in scene — wire _repository manually.");
            }

            EditorUtility.SetDirty(registry.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(registry.gameObject.scene);
            Debug.Log($"[ItemRegistryWirer] Done — {defs.Length} items wired. Bridge + Checker refs set.");
        }
    }
}
