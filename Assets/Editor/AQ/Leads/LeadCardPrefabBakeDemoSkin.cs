#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    // Re-bakes lead card prefabs to the target 360x220 layout and sane TMP settings.
    public static class LeadCardPrefabBakeDemoSkin
    {
        const int CARD_W = 360;
        const int CARD_H = 220;
        const int PAD    = 16;

        [MenuItem("AQ/Leads/Bake Demo Skin (Runtime Prefabs)")]
        public static void Bake()
        {
            var guids = new List<string>();
            guids.AddRange(AssetDatabase.FindAssets("t:Prefab LeadCard_Blank"));
            guids.AddRange(AssetDatabase.FindAssets("t:Prefab LeadCard_Runtime"));

            int updated = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var stage = PrefabUtility.LoadPrefabContents(path);
                bool changed = ReSkin(stage);
                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(stage, path);
                    updated++;
                }
                PrefabUtility.UnloadPrefabContents(stage);
            }

            Debug.Log($"LeadCardPrefabBakeDemoSkin: Updated {updated} prefab(s).");
        }

        static bool ReSkin(GameObject root)
        {
            bool changed = false;

            var rt = root.transform as RectTransform;
            if (rt != null)
            {
                if (rt.sizeDelta.x != CARD_W || rt.sizeDelta.y != CARD_H)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot     = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(CARD_W, CARD_H);
                    changed = true;
                }
            }

            // Known children from the demo runtime card structure.
            var tPortrait    = Find<RectTransform>(root, "Portrait");
            var tTitle       = Find<TMP_Text>(root, "Text_Title");
            var tActionTag   = Find<TMP_Text>(root, "Text_ActionTag");
            var tOneLiner    = Find<TMP_Text>(root, "Text_OneLiner");
            var tCost        = Find<TMP_Text>(root, "Text_Cost");
            var tProceedBtn  = Find<Button>(root, "Button_Proceed");
            var tReqRow      = Find<RectTransform>(root, "Requirements");
            var tBadgesRow   = Find<RectTransform>(root, "Badges");
            var tActorAnchor = Find<RectTransform>(root, "ActorAnchor");

            // Clamp and place portrait as a soft background block (no slicing needed here).
            if (tPortrait != null)
            {
                tPortrait.anchorMin = new Vector2(0, 1);
                tPortrait.anchorMax = new Vector2(0, 1);
                tPortrait.pivot     = new Vector2(0, 1);
                tPortrait.anchoredPosition = new Vector2(PAD, -PAD);
                tPortrait.sizeDelta = new Vector2(156, 156);
                var img = tPortrait.GetComponent<Image>();
                if (img != null)
                {
                    img.raycastTarget = false;
                    img.type = Image.Type.Simple;
                    var c = img.color; c.a = 0.12f; img.color = c;
                }
                changed = true;
            }

            // Title
            if (tTitle != null)
            {
                tTitle.fontSize = 24f;
                tTitle.enableAutoSizing = false;
                tTitle.textWrappingMode = TextWrappingModes.NoWrap;
                tTitle.overflowMode = TextOverflowModes.Ellipsis;
                tTitle.alignment = TextAlignmentOptions.TopLeft;
                tTitle.raycastTarget = false;
                changed = true;
            }

            // Action tag (category)
            if (tActionTag != null)
            {
                tActionTag.fontSize = 18f;
                tActionTag.enableAutoSizing = false;
                tActionTag.textWrappingMode = TextWrappingModes.NoWrap;
                tActionTag.overflowMode = TextOverflowModes.Ellipsis;
                tActionTag.alignment = TextAlignmentOptions.TopLeft;
                tActionTag.raycastTarget = false;
                tActionTag.color = new Color(0.75f, 0.85f, 1f, 1f);
                changed = true;
            }

            // One-liner
            if (tOneLiner != null)
            {
                tOneLiner.fontSize = 18f;
                tOneLiner.enableAutoSizing = false;
                tOneLiner.textWrappingMode = TextWrappingModes.Normal;
                tOneLiner.overflowMode = TextOverflowModes.Overflow;
                tOneLiner.alignment = TextAlignmentOptions.TopLeft;
                tOneLiner.raycastTarget = false;
                changed = true;
            }

            // Cost
            if (tCost != null)
            {
                tCost.fontSize = 18f;
                tCost.enableAutoSizing = false;
                tCost.textWrappingMode = TextWrappingModes.NoWrap;
                tCost.overflowMode = TextOverflowModes.Ellipsis;
                tCost.alignment = TextAlignmentOptions.MidlineRight;
                tCost.raycastTarget = false;
                changed = true;
            }

            // Proceed button (teal)
            if (tProceedBtn != null)
            {
                var img = tProceedBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.09f, 0.60f, 0.61f, 1f);
                changed = true;
            }

            // Requirements row height (tiny chips)
            if (tReqRow != null)
            {
                var le = tReqRow.GetComponent<LayoutElement>();
                if (le == null) le = tReqRow.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 28;
                le.minHeight = 28;
                changed = true;
            }

            // Badges row height
            if (tBadgesRow != null)
            {
                var le = tBadgesRow.GetComponent<LayoutElement>();
                if (le == null) le = tBadgesRow.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 24;
                le.minHeight = 24;
                changed = true;
            }

            // Actor anchor — pin at top-left gutter so the “float badge” version can parent here if needed
            if (tActorAnchor != null)
            {
                tActorAnchor.anchorMin = new Vector2(0f, 1f);
                tActorAnchor.anchorMax = new Vector2(0f, 1f);
                tActorAnchor.pivot     = new Vector2(0f, 1f);
                tActorAnchor.anchoredPosition = new Vector2(PAD, -PAD);
                tActorAnchor.sizeDelta = Vector2.zero;
                changed = true;
            }

            // Purge legacy bottom-duplicated labels if present (Title/Action/OneLiner/Proceed clones)
            changed |= TryDestroyChild(root, "Title");
            changed |= TryDestroyChild(root, "Action");
            changed |= TryDestroyChild(root, "OneLiner");
            changed |= TryDestroyChild(root, "Proceed");

            return changed;
        }

        static T Find<T>(GameObject root, string name) where T : Component
        {
            var tr = FindTransform(root.transform, name);
            return tr != null ? tr.GetComponent<T>() : null;
        }

        static Transform FindTransform(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var hit = FindTransform(root.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        static bool TryDestroyChild(GameObject root, string childName)
        {
            var tr = FindTransform(root.transform, childName);
            if (tr == null) return false;
            Object.DestroyImmediate(tr.gameObject);
            return true;
        }
    }
}
#endif
