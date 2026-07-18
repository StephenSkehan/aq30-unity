#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using AQ.App.Generators;
using AQ.App.Items;
using AQ.App.UI.Board;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>
    /// The audio-opening integration flip (design ruling 2026-07-18): registers
    /// the six audio_investigation ItemDefinitionSOs and the six-tier Field Kit
    /// generator in the scene's MergeBoardController, and makes the Field Kit
    /// the starting/default generator. Idempotent — safe to re-run.
    /// (The retired 10-tier gen_audio_rig was never authored; Field Kit replaces
    /// it as the carry/deploy generator producing audio items only in Ep1.)
    /// </summary>
    public static class IntegrateAudioOpening
    {
        [MenuItem("AQ/Setup/Integrate Audio Opening (Field Kit)")]
        public static void Integrate()
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) { Debug.LogWarning("[AudioOpen] MergeBoardController not found in open scene."); return; }

            Undo.RecordObject(board, "Integrate audio opening");
            var so = new SerializedObject(board);

            // 1) Register the six audio item SOs (skip ones already present).
            var itemsProp = so.FindProperty("itemDefinitions");
            var existing = new HashSet<Object>();
            for (int i = 0; i < itemsProp.arraySize; i++)
                existing.Add(itemsProp.GetArrayElementAtIndex(i).objectReferenceValue);

            int added = 0;
            for (int t = 1; t <= 6; t++)
            {
                var def = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(
                    $"Assets/ScriptableObjects/Items/audio_investigation_t{t}.asset");
                if (def == null) { Debug.LogWarning($"[AudioOpen] missing audio_investigation_t{t}"); continue; }
                if (existing.Contains(def)) continue;
                itemsProp.arraySize++;
                itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1).objectReferenceValue = def;
                added++;
            }

            // 2) Register the Field Kit generator type.
            var fieldKit = AssetDatabase.LoadAssetAtPath<GeneratorTypeSO>(
                "Assets/App/Generators/GeneratorType_FieldKit.asset");
            if (fieldKit == null) { Debug.LogWarning("[AudioOpen] GeneratorType_FieldKit missing."); return; }

            var gensProp = so.FindProperty("generatorTypes");
            bool genPresent = Enumerable.Range(0, gensProp.arraySize)
                .Any(i => gensProp.GetArrayElementAtIndex(i).objectReferenceValue == fieldKit);
            if (!genPresent)
            {
                gensProp.arraySize++;
                gensProp.GetArrayElementAtIndex(gensProp.arraySize - 1).objectReferenceValue = fieldKit;
            }

            // 3) Field Kit becomes the opening/default generator.
            so.FindProperty("defaultGeneratorType").objectReferenceValue = fieldKit;
            so.FindProperty("defaultGeneratorFamily").stringValue = "gen_field_kit";

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(board);

            // 4) The MergeEventsBridge resolves itemIds through the scene's
            // ItemRegistry (NOT the controller list) — without this, audio items
            // publish empty itemIds and requirements never satisfy. Bitten during
            // the 2026-07-18 integration; the wirer collects every SO on disk.
            AQ.Editor.Items.ItemRegistryWirer.WireRegistry();

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[AudioOpen] Integrated: +{added} audio item SOs, FieldKit registered={!genPresent}, registry wired, default generator=gen_field_kit.");
        }
    }
}
#endif
