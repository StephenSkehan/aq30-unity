#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace AQ.Editor.UI
{
    public static class AutoWireLeadsBar
    {
        private const string LeadCardPrefabPath  = "Assets/UI/Prefabs/LeadCard.prefab";
        private const string TierPopupPrefabPath = "Assets/UI/Prefabs/TierSetPopup.prefab";

        [MenuItem("AQ/UI/Wire LeadsBar (auto)")]
        public static void WireLeadsBar()
        {
            // 1) Find Canvas_Board (fallback to any Canvas)
            var canvasBoard = GameObject.Find("Canvas_Board");
            if (!canvasBoard)
            {
                var allCanvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.InstanceID);
                canvasBoard = allCanvas.FirstOrDefault()?.gameObject;
            }
            if (!canvasBoard)
            {
                Debug.LogError("[AutoWireLeadsBar] No Canvas found in scene.");
                return;
            }

            // 2) Find HUD_Board under Canvas_Board (fallback to canvas root)
            var hudBoard = FindChildByName(canvasBoard.transform, "HUD_Board")?.gameObject ?? canvasBoard;

            // 3) Locate the LeadsBar ScrollRect (prefer a GO named 'LeadsBar')
            ScrollRect leadsBar = null;
            var named = FindChildByName(canvasBoard.transform, "LeadsBar");
            if (named) leadsBar = named.GetComponent<ScrollRect>();

            if (!leadsBar)
            {
                // heuristic: a ScrollRect that contains "Viewport/Content_Leads"
                var scrollRects = Object.FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
                leadsBar = scrollRects.FirstOrDefault(s =>
                    s && s.transform.Find("Viewport/Content_Leads") != null);
            }

            if (!leadsBar)
            {
                Debug.LogError("[AutoWireLeadsBar] Could not find a LeadsBar ScrollRect (expected 'LeadsBar' with child 'Viewport/Content_Leads').");
                return;
            }

            // 4) Ensure TierSetPopup instance exists in scene (instantiate if missing)
            var tierPopupInstance = Object.FindFirstObjectByType<AQ.UI.Leads.TierSetPopup>(FindObjectsInactive.Include);
            if (!tierPopupInstance)
            {
                var prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(TierPopupPrefabPath);
                if (!prefabGO)
                {
                    Debug.LogError($"[AutoWireLeadsBar] Missing prefab at {TierPopupPrefabPath}");
                    return;
                }

                var inst = PrefabUtility.InstantiatePrefab(prefabGO, canvasBoard.scene) as GameObject;
                if (!inst)
                {
                    Debug.LogError("[AutoWireLeadsBar] Failed to instantiate TierSetPopup prefab.");
                    return;
                }
                Undo.RegisterCreatedObjectUndo(inst, "Instantiate TierSetPopup");
                // Place under Canvas_Board
                inst.transform.SetParent(canvasBoard.transform, false);

                tierPopupInstance = inst.GetComponent<AQ.UI.Leads.TierSetPopup>();
            }

            // 5) Create or find a host and add LeadsBarPopulator
            var host = FindChildByName(hudBoard.transform, "LeadsBarRuntime")?.gameObject;
            if (!host)
            {
                host = new GameObject("LeadsBarRuntime");
                Undo.RegisterCreatedObjectUndo(host, "Create LeadsBarRuntime");
                host.transform.SetParent(hudBoard.transform, false);
            }

            var pop = host.GetComponent<AQ.UI.Leads.LeadsBarPopulator>();
            if (!pop)
            {
                pop = Undo.AddComponent<AQ.UI.Leads.LeadsBarPopulator>(host);
            }

            // 6) Assign fields
            pop.leadsBar = leadsBar;

            // lead card prefab (component on prefab asset)
            var leadCardPrefabGO = AssetDatabase.LoadAssetAtPath<GameObject>(LeadCardPrefabPath);
            if (!leadCardPrefabGO)
            {
                Debug.LogError($"[AutoWireLeadsBar] Missing prefab at {LeadCardPrefabPath}");
                return;
            }
            var presenter = leadCardPrefabGO.GetComponent<AQ.UI.Leads.LeadCardPresenter>();
            if (!presenter)
            {
                Debug.LogError("[AutoWireLeadsBar] LeadCard.prefab does not have LeadCardPresenter on root.");
                return;
            }
            pop.leadCardPrefab = presenter.gameObject;

            // tier popup instance
            pop.tierSetPopup = tierPopupInstance;

            // 7) Load Stakeout Fuel tier icons (T1..T6) by known names; fallback to pattern search
            var tierPaths = new[]
            {
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t1_paper_cup.png",
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t2_hot_coffee_cup.png",
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t3_coffee_and_donut.png",
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t4_burger.png",
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t5_burger_fries_drink.png",
                "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master/stakeout_fuel_t6_takeaway_feast_caddy.png",
            };

            var sprites = new List<Sprite>(6);
            foreach (var p in tierPaths)
            {
                var sp = LoadSpriteAtPath(p);
                if (sp) sprites.Add(sp);
            }

            // Fallback: search by pattern if any were missing
            if (sprites.Count < 6)
            {
                var guids = AssetDatabase.FindAssets("stakeout_fuel t:Sprite");
                var found = new List<(string path, Sprite sp)>();
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sp != null)
                        found.Add((path, sp));
                }
                // Order by t1..t6 in name
                var ordered = found
                    .OrderBy(x => TierRankFromName(x.path))
                    .Select(x => x.sp)
                    .ToList();

                sprites = ordered;
            }

            pop.stakeoutFuelTierIcons = sprites.ToArray();
            pop.stakeoutFuelTierNames = new[]
            {
                "Paper Cup", "Hot Coffee", "Coffee + Donut", "Burger", "Combo", "Feast Caddy"
            };

            EditorUtility.SetDirty(pop);

            // 8) Populate the demo leads immediately
            pop.PopulateNow();

            Debug.Log("[AutoWireLeadsBar] Wired and populated demo leads.");
        }

        // ---------- helpers ----------

        private static Transform FindChildByName(Transform parent, string name)
        {
            if (!parent) return null;
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static Sprite LoadSpriteAtPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Returns 1..6 if name contains t1..t6; otherwise a large number to push to end.
        private static int TierRankFromName(string path)
        {
            var lower = path.ToLowerInvariant();
            for (int i = 1; i <= 6; i++)
            {
                if (lower.Contains($"_t{i}_")) return i;
            }
            // Also accept suffix variant like ..._t6.png
            for (int i = 1; i <= 6; i++)
            {
                if (lower.Contains($"_t{i}.")) return i;
            }
            return 999;
        }
    }
}
#endif
