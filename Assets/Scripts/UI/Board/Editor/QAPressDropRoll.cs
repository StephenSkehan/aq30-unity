#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AQ.App;
using AQ.App.Generators;
using AQ.App.Items;

namespace AQ.EditorTools
{
    /// <summary>
    /// Statistical check of the gen_junk drop table's Ep2 press gating, edit-mode
    /// only (no play session, no save mutation): rolls the T1 table with the
    /// Arthur flag off then on, asserts press never drops ungated, reports the
    /// gated press share, and resolves every rolled (family, tier) against the
    /// ItemDefinitionSO assets so a missing SO or icon fails loudly.
    /// </summary>
    public static class QAPressDropRoll
    {
        private const string Flag = "aq.char.arthur.active";
        private const int Rolls = 5000;

        [MenuItem("AQ/Dev/QA Press Drop Roll (edit mode)")]
        public static void Run()
        {
            var so = AssetDatabase.LoadAssetAtPath<GeneratorTypeSO>(
                "Assets/App/Generators/GeneratorType_GenJunk.asset");
            if (so == null) { Debug.LogError("[QAPressDropRoll] GeneratorType_GenJunk not found."); return; }

            bool hadFlag = NarrativeFlags.Has(Flag);

            NarrativeFlags.Clear(Flag);
            int pressOff = CountPress(so, out _);

            NarrativeFlags.Set(Flag);
            int pressOn = CountPress(so, out var unresolved);

            // restore whatever state the player actually had
            if (hadFlag) NarrativeFlags.Set(Flag); else NarrativeFlags.Clear(Flag);

            if (pressOff > 0)
                Debug.LogError($"[QAPressDropRoll] FAIL — {pressOff}/{Rolls} press drops with flag UNSET (gate leak).");
            else if (pressOn == 0)
                Debug.LogError($"[QAPressDropRoll] FAIL — 0/{Rolls} press drops with flag SET (gate never opens).");
            else if (unresolved.Count > 0)
                Debug.LogError($"[QAPressDropRoll] FAIL — rolled ids missing SO or icon: {string.Join(", ", unresolved)}");
            else
                Debug.Log($"[QAPressDropRoll] PASS — flag off: 0 press; flag on: {pressOn}/{Rolls} " +
                          $"({100f * pressOn / Rolls:F1}%); every rolled item resolved to an SO with an icon. " +
                          $"Flag restored to {(hadFlag ? "SET" : "UNSET")}.");
        }

        private static int CountPress(GeneratorTypeSO so, out List<string> unresolved)
        {
            var missing = new HashSet<string>();
            int press = 0;
            for (int i = 0; i < Rolls; i++)
            {
                var e = DropRoller.Roll(so, 0);
                if (e == null || e.Value.type != DropType.Item) continue;
                if (e.Value.itemFamily == "press") press++;

                string id = $"{e.Value.itemFamily}_t{e.Value.itemTier + 1:00}";
                var def = AssetDatabase.LoadAssetAtPath<ItemDefinitionSO>(
                    $"Assets/ScriptableObjects/Items/{id}.asset");
                if (def == null || def.icon == null) missing.Add(id);
            }
            unresolved = new List<string>(missing);
            return press;
        }
    }
}
#endif
