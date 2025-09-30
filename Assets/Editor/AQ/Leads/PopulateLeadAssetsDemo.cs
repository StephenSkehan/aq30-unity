#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools.Leads
{
    /// <summary>
    /// Fills missing visuals on Lead_* ScriptableObjects:
    ///  - Assigns actorSprite from Assets/Art/UI/Leads/Actors (Actor1*, Actor2*, Actor3*...)
    ///  - If a requirement entry has an empty "tierSprites" array, assigns up to 6 sprites
    ///    from a "stakeout_fuel" folder (best-effort search).
    /// Safe/idempotent: only fills when null/empty.
    /// </summary>
    public static class PopulateLeadAssetsDemo
    {
        private static readonly string[] ActorFolders = { "Assets/Art/UI/Leads/Actors" };
        private static readonly string[] TierFoldersCandidates =
        {
            "Assets/Art/UI/Icons/Leads/stakeout_fuel",
            "Assets/Art/UI/Icons/UI/Leads/stakeout_fuel",
            "Assets/Art/UI/Icons/stakeout_fuel",
            "Assets/Art/UI/Icons"
        };

        [MenuItem("AQ/Leads/Populate Demo Visuals/Assign Missing actorSprite")]
        public static void AssignActors()
        {
            var actors = LoadSprites(ActorFolders, "Actor");
            if (actors.Count == 0)
            {
                Debug.LogWarning("[AQ PopulateLeads] No Actor* sprites found.");
                return;
            }

            var leads = FindLeadAssets();
            int changed = 0, untouched = 0, idx = 0;

            foreach (var so in leads)
            {
                var actorProp = so.FindProperty("actorSprite");
                if (actorProp != null && actorProp.objectReferenceValue == null)
                {
                    actorProp.objectReferenceValue = actors[idx % actors.Count];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    changed++;
                    idx++;
                }
                else untouched++;
            }

            Debug.Log($"[AQ PopulateLeads] actorSprite assigned: {changed}; already set: {untouched}.");
        }

        [MenuItem("AQ/Leads/Populate Demo Visuals/Fill Empty Requirement Tier Sprites")]
        public static void FillTierSprites()
        {
            var tierSprites = LoadTierSet();
            if (tierSprites.Count == 0)
            {
                Debug.LogWarning("[AQ PopulateLeads] No stakeout_fuel sprite set found.");
                return;
            }

            var leads = FindLeadAssets();
            int groupsFilled = 0, groupsSkipped = 0;

            foreach (var so in leads)
            {
                var reqs = so.FindProperty("requirements");
                if (reqs == null) continue;

                for (int i = 0; i < reqs.arraySize; i++)
                {
                    var elem = reqs.GetArrayElementAtIndex(i);
                    var tierArr = elem.FindPropertyRelative("tierSprites");
                    int count = tierArr != null ? tierArr.arraySize : 0;

                    if (tierArr == null) continue;
                    if (count > 0) { groupsSkipped++; continue; }

                    tierArr.arraySize = Mathf.Min(6, tierSprites.Count);
                    for (int k = 0; k < tierArr.arraySize; k++)
                        tierArr.GetArrayElementAtIndex(k).objectReferenceValue = tierSprites[k];

                    // If there is a selectedTierIndex and isSatisfied bool, clear them to demo defaults
                    var sel = elem.FindPropertyRelative("selectedTierIndex");
                    if (sel != null) sel.intValue = 0;
                    var sat = elem.FindPropertyRelative("isSatisfied");
                    if (sat != null) sat.boolValue = false;

                    groupsFilled++;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Debug.Log($"[AQ PopulateLeads] Requirement groups filled: {groupsFilled}; already had sprites: {groupsSkipped}.");
        }

        // ----- helpers -----
        private static List<SerializedObject> FindLeadAssets()
        {
            var guids = AssetDatabase.FindAssets("Lead_ t:ScriptableObject", new[] { "Assets" });
            var list = new List<SerializedObject>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null) list.Add(new SerializedObject(obj));
            }
            return list;
        }

        private static List<Sprite> LoadSprites(string[] folders, string namePrefix = null)
        {
            var results = new List<Sprite>();
            foreach (var folder in folders)
            {
                var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (s != null && (string.IsNullOrEmpty(namePrefix) || s.name.StartsWith(namePrefix)))
                        results.Add(s);
                }
            }
            return results.OrderBy(s => s.name).ToList();
        }

        private static List<Sprite> LoadTierSet()
        {
            foreach (var folder in TierFoldersCandidates)
            {
                var list = LoadSprites(new[] { folder });
                // Heuristic: prefer folders that have 6–8 sprites, not actor heads
                var filtered = list.Where(s => !s.name.StartsWith("Actor")).ToList();
                if (filtered.Count >= 6) return filtered.OrderBy(s => s.name).Take(6).ToList();
            }
            return new List<Sprite>();
        }
    }
}
#endif
