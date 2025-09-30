#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    /// <summary>
    /// Scene-only (safe in Play Mode):
    /// - Force-activates RequirementsRow and Req_1..Req_3.
    /// - Assigns actor portrait sprites from Assets/Art/UI/Leads/Actors.
    /// - Assigns requirement icons from your stakeout_fuel set.
    /// - Ensures Images are enabled, maskable, default UI material.
    /// - Brings ActorAnchor and Req icons to the front for visibility.
    /// Run this AFTER "Push All → Runtime".
    /// </summary>
    public static class AssignLive_EnablePaintFront
    {
        // Adjust only if you move art
        private const string ActorsFolder = "Assets/Art/UI/Leads/Actors";
        private static readonly string[] TierFolders =
        {
            "Assets/Art/UI/Icons/MergeChains/stakeout_fuel/master",
            "Assets/Art/UI/Icons/MergeChains/stakeout_fuel",
            "Assets/Art/UI/Icons/MergeChains",
            "Assets/Art/UI/Icons"
        };

        [MenuItem("AQ/UI/Leads/Assign Live → Enable + Paint + Front")]
        public static void Run()
        {
            var content = GameObject.Find("Content_Leads")?.transform;
            if (!content) { Debug.LogWarning("[AQ LiveAssign] Content_Leads not found."); return; }

            // Load sprites
            var actorSprites = AssetDatabase.FindAssets("t:Sprite", new[] { ActorsFolder })
                                            .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                                            .Where(s => s != null).OrderBy(s => s.name).ToArray();

            var tierSprites = LoadTierSprites();
            if (actorSprites.Length == 0) Debug.LogWarning("[AQ LiveAssign] No actor sprites found in " + ActorsFolder);
            if (tierSprites.Length < 3) Debug.LogWarning("[AQ LiveAssign] Could not find ≥3 stakeout_fuel sprites; icons will be skipped.");

            int cards=0, actors=0, reqIcons=0, rowsActivated=0;

            foreach (Transform card in content)
            {
                bool looksLikeCard = card.Find("Text_Title") || card.Find("RequirementsRow");
                if (!looksLikeCard) continue;
                cards++;

                // --- RequirementsRow: force active hierarchy ---
                var row = card.Find("RequirementsRow");
                if (row && !row.gameObject.activeSelf)
                {
                    row.gameObject.SetActive(true);
                    rowsActivated++;
                }
                // Also force children active
                for (int i = 1; i <= 3; i++)
                {
                    var slot = row ? row.Find($"Req_{i}") : null;
                    if (slot && !slot.gameObject.activeSelf) slot.gameObject.SetActive(true);
                }

                // --- Actor sprite ---
                var anchor = card.Find("ActorAnchor");
                if (anchor)
                {
                    var imgTf = anchor.Find("Image");
                    if (!imgTf)
                    {
                        var go = new GameObject("Image", typeof(RectTransform), typeof(Image));
                        var rt = go.GetComponent<RectTransform>();
                        rt.SetParent(anchor, false);
                        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.sizeDelta = new Vector2(96, 96);
                        imgTf = rt;
                    }
                    var img = imgTf.GetComponent<Image>();
                    if (actorSprites.Length > 0)
                    {
                        img.sprite = actorSprites[(cards-1) % actorSprites.Length];
                        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                        img.enabled = true;
                        img.material = null;
                        img.maskable = true;
                        img.raycastTarget = false;
                        anchor.SetAsLastSibling();
                        img.transform.SetAsLastSibling();
                        actors++;
                    }
                }

                // --- Requirement icons ---
                if (tierSprites.Length >= 3 && row)
                {
                    reqIcons += PaintReq(row, "Req_1", tierSprites[0]);
                    reqIcons += PaintReq(row, "Req_2", tierSprites[2]);
                    reqIcons += PaintReq(row, "Req_3", tierSprites[4 % tierSprites.Length]);
                    // bring row and icons to front within card
                    row.SetAsLastSibling();
                    for (int i = 1; i <= 3; i++)
                    {
                        var icon = row.Find($"Req_{i}/Icon");
                        if (icon) icon.SetAsLastSibling();
                    }
                }
            }

            Debug.Log($"[AQ LiveAssign] Cards={cards} | Rows activated={rowsActivated} | Actors assigned={actors} | Req icons assigned={reqIcons}. (Scene-only)");
        }

        private static int PaintReq(Transform row, string reqName, Sprite sprite)
        {
            var slot = row.Find(reqName);
            if (!slot) return 0;
            slot.gameObject.SetActive(true);
            var icon = slot.Find("Icon")?.GetComponent<Image>();
            if (!icon) return 0;
            icon.sprite = sprite;
            icon.enabled = true;
            icon.material = null;
            icon.maskable = true;
            icon.raycastTarget = false;
            return 1;
        }

        private static Sprite[] LoadTierSprites()
        {
            foreach (var folder in TierFolders)
            {
                var list = AssetDatabase.FindAssets("t:Sprite", new[] { folder })
                                        .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                                        .Where(s => s != null && !s.name.StartsWith("Actor"))
                                        .OrderBy(s => s.name).ToArray();
                if (list.Length >= 3) return list;
            }
            return new Sprite[0];
        }
    }
}
#endif
