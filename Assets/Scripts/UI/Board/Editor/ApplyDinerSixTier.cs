#if UNITY_EDITOR
using AQ.App.Overflow;
using AQ.App.UI.Board;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>
    /// Diner 10→6 ruling (2026-07-18): after corner_diner_t07–t10 SOs were
    /// retired, the scene's item lists held dead references. This prunes them
    /// and re-runs the ItemRegistry wirer. Idempotent.
    /// </summary>
    public static class ApplyDinerSixTier
    {
        [MenuItem("AQ/Setup/Apply Diner Six-Tier Chain")]
        public static void Apply()
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) { Debug.LogWarning("[Diner6] MergeBoardController not found."); return; }

            Undo.RecordObject(board, "Diner six-tier");
            var so = new SerializedObject(board);
            var items = so.FindProperty("itemDefinitions");
            int removed = 0;
            for (int i = items.arraySize - 1; i >= 0; i--)
            {
                if (items.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    items.DeleteArrayElementAtIndex(i);
                    removed++;
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(board);

            AQ.Editor.Items.ItemRegistryWirer.WireRegistry();

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[Diner6] Pruned {removed} dead item refs; registry rewired.");
        }

        // ---- ceiling-test drivers (play mode) ----

        [MenuItem("AQ/Dev/QA Push Diner Pair (T5)")]
        private static void PushT5Pair() => PushPair(4);

        [MenuItem("AQ/Dev/QA Push Diner Pair (T6)")]
        private static void PushT6Pair() => PushPair(5);

        [MenuItem("AQ/Dev/QA Push Lab Pair (T5)")]
        private static void PushLabT5() => PushPair(4, "gen_investigation_lab");

        [MenuItem("AQ/Dev/QA Push Lab Pair (T6)")]
        private static void PushLabT6() => PushPair(5, "gen_investigation_lab");

        private static void PushPair(int tier, string family = "corner_diner")
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Diner6] Play mode only."); return; }
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            if (board == null) { Debug.LogWarning("[Diner6] no board."); return; }
            for (int i = 0; i < 2; i++)
                board.PlaceFromOverflow(new OverflowTileData
                {
                    kind = OverflowKind.Generator, family = family, tier = tier
                });
            Debug.Log($"[Diner6] Placed 2x {family} T{tier + 1} on board — run QA Merge First Pair.");
        }
    }
}
#endif
