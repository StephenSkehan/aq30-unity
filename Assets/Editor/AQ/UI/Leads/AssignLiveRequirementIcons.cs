#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Assigns sprites from your stakeout_fuel set to Req_1..Req_3/Icon on the live cards.
    /// Looks in the exact folder shown in your screenshot, then falls back to broader searches.
    /// Scene-only, no asset or prefab edits.
    /// </summary>
    public static class AssignLiveRequirementIcons
    {
        private static readonly string[] PreferredFolders = new[]
        {
            // exact path from your screenshot:
            "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master",
            // reasonable fallbacks if you move the set later:
            "Assets/Art/UI/Icons/MergeChains/stakeout_fuel",
            "Assets/Art/UI/Icons/MergeChains",
            "Assets/Art/UI/Icons"
        };

        [MenuItem("AQ/UI/Leads/Assign Live -> Requirement Icons (Stakeout Fuel)")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content) { Debug.LogWarning("[AQ ReqLive] Content_Leads not found."); return; }

            var tierSprites = LoadTierSprites();
            if (tierSprites.Count == 0) { Debug.LogWarning("[AQ ReqLive] Could not locate stakeout_fuel icons."); return; }

            int cards = 0, iconsSet = 0;
            foreach (Transform card in content)
            {
                if (!card.Find("RequirementsRow")) continue;
                cards++;

                iconsSet += SetIcon(card, "Req_1", tierSprites[0 % tierSprites.Count]);
                iconsSet += SetIcon(card, "Req_2", tierSprites[2 % tierSprites.Count]);
                iconsSet += SetIcon(card, "Req_3", tierSprites[4 % tierSprites.Count]);
            }

            Debug.Log($"[AQ ReqLive] Cards updated: {cards}; icons assigned: {iconsSet}.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private static int SetIcon(Transform card, string reqName, Sprite sprite)
        {
            var icon = card.Find($"RequirementsRow/{reqName}/Icon")?.GetComponent<Image>();
            if (!icon) return 0;
            icon.sprite = sprite;
            icon.enabled = true;
            icon.raycastTarget = false;
            return 1;
        }

        private static List<Sprite> LoadTierSprites()
        {
            foreach (var folder in PreferredFolders)
            {
                var guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
                var list = guids.Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                                .Where(s => s != null && !s.name.StartsWith("Actor"))
                                .OrderBy(s => s.name).ToList();
                if (list.Count >= 3) return list; // we only need 3 for the demo
            }
            return new List<Sprite>();
        }
    }
}
#endif
