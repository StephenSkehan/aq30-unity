#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class LeadCard_FloatActors
    {
        private const string PrefabPath = "Assets/UI/Prefabs/LeadCardView.prefab";

        [MenuItem("AQ/Leads/Retrofit/Convert Actor Badge to Floating")]
        public static void Convert()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (!prefab) { Debug.LogError($"❌ Prefab not found: {PrefabPath}"); return; }

            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var badge = root.transform.Find("ActorBadge") as RectTransform;
                if (!badge)
                {
                    Debug.LogError("❌ ActorBadge not found on LeadCardView prefab.");
                    return;
                }

                // Remove legacy visuals that block the background
                DestroyIfExists(badge, "Backplate");
                DestroyIfExists(badge, "Frame");

                // Ensure portrait hierarchy exists
                var mask = badge.Find("PortraitMask") as RectTransform;
                if (!mask)
                {
                    var go = new GameObject("PortraitMask", typeof(RectTransform), typeof(Image), typeof(Mask));
                    go.transform.SetParent(badge, false);
                    mask = go.GetComponent<RectTransform>();
                    mask.sizeDelta = new Vector2(140, 140);
                    var img = go.GetComponent<Image>(); img.color = Color.white; // mask carrier, hidden by Mask
                    var mk = go.GetComponent<Mask>(); mk.showMaskGraphic = false;
                }

                var portrait = mask.Find("Portrait") as RectTransform;
                if (!portrait)
                {
                    var go = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(mask, false);
                    portrait = go.GetComponent<RectTransform>();
                    portrait.sizeDelta = new Vector2(140, 140);
                }
                var pImg = portrait.GetComponent<Image>();
                pImg.preserveAspect = true;
                pImg.raycastTarget = true; // tap target

                // Add a subtle halo via Outline to help on busy backgrounds
                var outline = portrait.GetComponent<Outline>() ?? portrait.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.13f, 0.71f, 0.71f, 0.25f); // #21B6B6 @ 25%
                outline.effectDistance = new Vector2(1f, -1f); // ~1–2 px halo

                // Keep badge anchor & overhang
                badge.anchorMin = new Vector2(0.5f, 1f);
                badge.anchorMax = new Vector2(0.5f, 1f);
                badge.pivot     = new Vector2(0.5f, 0.5f);
                badge.sizeDelta = new Vector2(156, 156);
                badge.anchoredPosition = new Vector2(0, -78);

                // Ensure it's clickable
                if (!badge.GetComponent<Button>()) badge.gameObject.AddComponent<Button>();

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("✅ LeadCardView: Actor badge converted to floating (transparent) with outline halo.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void DestroyIfExists(Transform parent, string childName)
        {
            var t = parent.Find(childName);
            if (t) Object.DestroyImmediate(t.gameObject, true);
        }
    }
}
#endif
