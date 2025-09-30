#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace AQ.Editor.UI
{
    /// <summary>
    /// Repairs LeadCard.prefab by removing missing scripts, ensuring a LeadCardPresenter
    /// exists on the root and is wired, then calls AutoWireLeadsBar to populate the HUD.
    /// </summary>
    public static class FixLeadCardPrefabAndWire
    {
        private const string LeadCardPrefabPath  = "Assets/UI/Prefabs/LeadCard.prefab";
        private const string TierPopupPrefabPath = "Assets/UI/Prefabs/TierSetPopup.prefab";

        [MenuItem("AQ/UI/Fix LeadCard.prefab (repair) + Wire LeadsBar")]
        public static void FixAndWire()
        {
            if (!System.IO.File.Exists(LeadCardPrefabPath))
            {
                Debug.LogError($"[FixLeadCard] Missing prefab at {LeadCardPrefabPath}");
                return;
            }

            // 1) Ensure presenter on prefab and wiring is valid
            if (!EnsurePresenterOnPrefab(LeadCardPrefabPath))
            {
                Debug.LogError("[FixLeadCard] Failed to repair LeadCard.prefab. See errors above.");
                return;
            }

            // 2) Re-run the auto-wire to place cards
            AutoWireLeadsBar.WireLeadsBar();
        }

        // ---------- internal helpers ----------

        private static bool EnsurePresenterOnPrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                Debug.LogError("[FixLeadCard] Could not load prefab contents.");
                return false;
            }

            // (A) Remove ALL missing scripts (root + children)
            RemoveMissingScriptsRecursive(root);

            // (B) Ensure LeadCardPresenter on ROOT
            var presenter = root.GetComponent<AQ.UI.Leads.LeadCardPresenter>();
            if (!presenter)
                presenter = root.AddComponent<AQ.UI.Leads.LeadCardPresenter>();

            // (C) Wire fields by common-name heuristics
            presenter.background     = FindImage(root.transform, "Background", fallbackFirstOnRoot: true);
            presenter.titleText      = FindTMP(root.transform, "Title");
            presenter.objectiveText  = FindTMP(root.transform, "Objective", "Body", "OneLiner");
            presenter.leadIdText     = FindTMP(root.transform, "LeadId", "ID");
            presenter.actorAnchor    = FindImage(root.transform, "ActorAnchor");

            var reqRow = FindRect(root.transform, "RequirementsRow", "Requirements", "ReqRow");
            if (reqRow) presenter.requirementsRow = reqRow;

            // (D) Recreate/ensure RequirementSlotView components on Req_* children
            var slotRoots = FindPossibleRequirementSlots(root.transform);
            var slots = new List<AQ.UI.Leads.RequirementSlotView>();
            foreach (var tr in slotRoots)
            {
                // Ensure no missing scripts on the slot object
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(tr.gameObject);

                var slot = tr.GetComponent<AQ.UI.Leads.RequirementSlotView>();
                if (!slot) slot = tr.gameObject.AddComponent<AQ.UI.Leads.RequirementSlotView>();
                slots.Add(slot);
            }
            presenter.slots = slots.OrderBy(s => s.name).ToArray();

            presenter.rewardsRow = FindRect(root.transform, "RewardsRow", "Rewards");

            // Prefer a Button on root; otherwise first found
            presenter.wholeCardButton =
                root.GetComponent<Button>() ??
                root.GetComponentsInChildren<Button>(true).FirstOrDefault();

            EditorUtility.SetDirty(presenter);

            // (E) Save back to asset (will fail if any missing scripts remain)
            var ok = PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            // Verify the saved asset indeed has the presenter on ROOT
            var assetGO = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var check   = assetGO ? assetGO.GetComponent<AQ.UI.Leads.LeadCardPresenter>() : null;
            if (!check)
            {
                Debug.LogError("[FixLeadCard] Presenter still not found on prefab root after save.");
                return false;
            }

            Debug.Log("[FixLeadCard] LeadCardPresenter present and wired on prefab root.");
            return true;
        }

        private static void RemoveMissingScriptsRecursive(GameObject root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }

        private static Image FindImage(Transform root, string contains, bool fallbackFirstOnRoot = false)
        {
            var img = root.GetComponentsInChildren<Image>(true)
                          .FirstOrDefault(i => i.name.IndexOf(contains, System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (!img && fallbackFirstOnRoot)
                img = root.GetComponent<Image>();
            return img;
        }

        private static TMP_Text FindTMP(Transform root, params string[] containsAny)
        {
            var tmps = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var key in containsAny)
            {
                var hit = tmps.FirstOrDefault(t => t.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0);
                if (hit) return hit;
            }
            return null;
        }

        private static RectTransform FindRect(Transform root, params string[] containsAny)
        {
            var rects = root.GetComponentsInChildren<RectTransform>(true);
            foreach (var key in containsAny)
            {
                var hit = rects.FirstOrDefault(r => r.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0);
                if (hit) return hit;
            }
            return null;
        }

        private static IEnumerable<Transform> FindPossibleRequirementSlots(Transform root)
        {
            // Any child named like Req_0/Req_1/Req_2 or RequirementSlot_*
            var all = root.GetComponentsInChildren<Transform>(true);
            return all.Where(t =>
            {
                var n = t.name.ToLowerInvariant();
                return n.StartsWith("req_") || n.StartsWith("req ") || n.StartsWith("requirement")
                       || n.Contains("slot") && n.Contains("req");
            });
        }
    }
}
#endif
