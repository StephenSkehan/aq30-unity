#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsAttachActors_FloatLayer
    {
        private const string LeadsBarPath = "LeadsBar";
        private const string ViewportPath = "LeadsBar/Viewport";
        private const string ContentPath  = "LeadsBar/Viewport/Content_Leads";
        private const string FloatLayerName = "FloatLayer_Actors";

        private const string ActorFolder = "Assets/Art/UI/Leads/Actors";
        private const string FallbackSilhouette = "Assets/Art/UI/Leads/actor_silhouette_round.png";

        [MenuItem("AQ/Leads/Actors • Attach (float above cards)")]
        public static void Attach()
        {
            if (!TryFind(out var leadsBar, out var viewport, out var content)) return;

            var floatLayer = EnsureFloatLayer(leadsBar);

            // collect sprites
            var actorGuids = AssetDatabase.FindAssets("t:Sprite", new[] { ActorFolder });
            var actorSprites = actorGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null)
                .ToArray();
            var fallback = AssetDatabase.LoadAssetAtPath<Sprite>(FallbackSilhouette);

            Undo.IncrementCurrentGroup();
            int made = 0;

            // nuke old badges
            for (int i = floatLayer.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(floatLayer.GetChild(i).gameObject);

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // pick sprite
                Sprite chosen = (actorSprites.Length > 0) ? actorSprites[i % actorSprites.Length] : null;
                if (!chosen) chosen = fallback;
                if (!chosen)
                {
                    Debug.LogWarning($"⚠️ No actor sprites in '{ActorFolder}' and fallback not found. Skipping.");
                    continue;
                }

                // create badge under float layer
                var go = new GameObject($"ActorBadge_{i}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                Undo.RegisterCreatedObjectUndo(go, "Create ActorBadge");
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(floatLayer, false);
                rt.sizeDelta = new Vector2(156, 156);
                rt.pivot = new Vector2(0.5f, 0f);              // bottom center of portrait sits on top edge of card
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); // we’ll set anchoredPosition explicitly

                var img = go.GetComponent<Image>();
                img.sprite = chosen;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
                img.raycastTarget = false;

                var cg = go.GetComponent<CanvasGroup>();
                cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false;

                PositionBadgeOverCard(rt, card, leadsBar);
                made++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"✅ Attached {made} floating actor badge(s) to {FloatLayerName} (outside mask).");
        }

        [MenuItem("AQ/Leads/Actors • Reposition (float layer)")]
        public static void Reposition()
        {
            if (!TryFind(out var leadsBar, out var viewport, out var content)) return;
            var floatLayer = leadsBar.Find(FloatLayerName) as RectTransform;
            if (!floatLayer) { Debug.LogWarning("⚠️ FloatLayer not found. Run 'Attach' first."); return; }

            int moved = 0;
            for (int i = 0; i < Mathf.Min(floatLayer.childCount, content.childCount); i++)
            {
                var badge = floatLayer.GetChild(i) as RectTransform;
                var card  = content.GetChild(i) as RectTransform;
                if (!badge || !card) continue;
                PositionBadgeOverCard(badge, card, leadsBar);
                moved++;
            }
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"↻ Repositioned {moved} badge(s).");
        }

        // ---------- helpers ----------
        private static bool TryFind(out RectTransform leadsBar, out RectTransform viewport, out RectTransform content)
        {
            leadsBar = GameObject.Find(LeadsBarPath)?.transform as RectTransform;
            viewport = GameObject.Find(ViewportPath)?.transform as RectTransform;
            content  = GameObject.Find(ContentPath )?.transform as RectTransform;

            if (!leadsBar || !viewport || !content)
            {
                Debug.LogError("❌ LeadsBar/Viewport/Content_Leads not found. Run AQ → Leads → Verify/Ensure first.");
                return false;
            }
            return true;
        }

        private static RectTransform EnsureFloatLayer(RectTransform leadsBar)
        {
            var t = leadsBar.Find(FloatLayerName) as RectTransform;
            if (!t)
            {
                var go = new GameObject(FloatLayerName, typeof(RectTransform), typeof(CanvasGroup));
                Undo.RegisterCreatedObjectUndo(go, "Create FloatLayer");
                t = go.GetComponent<RectTransform>();
                t.SetParent(leadsBar, false);
                t.anchorMin = new Vector2(0, 0);
                t.anchorMax = new Vector2(1, 1);
                t.offsetMin = Vector2.zero;
                t.offsetMax = Vector2.zero;
                t.pivot = new Vector2(0.5f, 0.5f);
                t.SetAsLastSibling(); // render above Viewport
                var cg = t.GetComponent<CanvasGroup>(); cg.blocksRaycasts = false; cg.interactable = false;
            }
            else
            {
                t.SetAsLastSibling(); // ensure on top
            }
            return t;
        }

        private static void PositionBadgeOverCard(RectTransform badge, RectTransform card, RectTransform leadsBar)
        {
            // world position of card's top-center
            var worldTopCenter = card.TransformPoint(new Vector3(card.rect.width * 0.5f, card.rect.height, 0f));
            // convert to LeadsBar local
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(leadsBar, 
                RectTransformUtility.WorldToScreenPoint(null, worldTopCenter),
                null, out local);
            // anchor badge by local point (we use anchorMin/Max (0,1) and pivot (0.5,0))
            badge.anchoredPosition = local + new Vector2(0f, 6f); // small lift
        }
    }
}
#endif
