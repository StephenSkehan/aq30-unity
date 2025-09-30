#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class AttachActorBadges
    {
        private const string Root = "LeadsBar/ScrollLeads/Viewport/Content_Leads";
        private const string AnchorName = "ActorAnchor";
        private const string BadgeName  = "ActorBadge_Float";

        [MenuItem("AQ/Leads/Attach Floating Actor Badges (placeholder)")]
        public static void Run()
        {
            var content = GameObject.Find(Root)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found. Run Leads → Verify + Repair first."); return; }

            // Try to load a portrait sprite; you can swap this path when art lands.
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/TopBar/ui_top_avatar_portrait_02.png");
            if (!sprite) { Debug.LogWarning("⚠️ Placeholder portrait not found; badges will use a white square."); }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            int attached = 0;
            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                var anchor = card.Find(AnchorName) as RectTransform;
                if (!anchor) continue;

                var existing = anchor.Find(BadgeName);
                if (existing) continue; // already attached

                // Create badge root (transparent container)
                var badgeGO = new GameObject(BadgeName, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(badgeGO, "Create ActorBadge_Float");
                var badgeRT = badgeGO.GetComponent<RectTransform>();
                badgeRT.SetParent(anchor, false);
                badgeRT.sizeDelta = new Vector2(156, 156);
                badgeRT.pivot = new Vector2(0.5f, 0f);
                badgeRT.anchoredPosition = Vector2.zero;

                // Soft shadow (optional oval)
                var shadow = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
                var shadowRT = shadow.GetComponent<RectTransform>();
                shadowRT.SetParent(badgeRT, false);
                shadowRT.anchorMin = new Vector2(0.5f, 0f);
                shadowRT.anchorMax = new Vector2(0.5f, 0f);
                shadowRT.pivot     = new Vector2(0.5f, 0.5f);
                shadowRT.anchoredPosition = new Vector2(0, -8);
                shadowRT.sizeDelta = new Vector2(180, 24);
                var sImg = shadow.GetComponent<Image>();
                sImg.color = new Color(0,0,0,0.25f);
                sImg.raycastTarget = false;

                // Portrait image
                var portrait = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
                var pRT = portrait.GetComponent<RectTransform>();
                pRT.SetParent(badgeRT, false);
                pRT.anchorMin = new Vector2(0.5f, 0f);
                pRT.anchorMax = new Vector2(0.5f, 0f);
                pRT.pivot     = new Vector2(0.5f, 0f);
                pRT.sizeDelta = new Vector2(156, 156);
                var pImg = portrait.GetComponent<Image>();
                pImg.sprite = sprite;
                pImg.color  = Color.white;
                pImg.preserveAspect = true;
                pImg.raycastTarget = false;

                // Optional: add a subtle rim (rounded square feel) by using a mask + background, if desired later.

                attached++;
            }

            Undo.CollapseUndoOperations(group);
            Debug.Log(attached == 0 ? "ℹ️ No ActorAnchors found or all badges already attached." : $"✅ Attached {attached} actor badge(s).");
        }
    }
}
#endif
