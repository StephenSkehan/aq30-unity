#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace AQ.Editor.Leads
{
    public static class LeadsAttachActorsFromArt
    {
        private const string ContentPath = "LeadsBar/Viewport/Content_Leads";
        private const string ActorFolder = "Assets/Art/UI/Leads/Actors"; // put your portraits here
        private const string FallbackSilhouette = "Assets/Art/UI/Leads/actor_silhouette_round.png";

        [MenuItem("AQ/Leads/Actors • Attach Demo Actors (156px, top-center)")]
        public static void Run()
        {
            var content = GameObject.Find(ContentPath)?.transform as RectTransform;
            if (!content) { Debug.LogError("❌ Content_Leads not found."); return; }

            // gather sprites from folder
            var actorGuids = AssetDatabase.FindAssets("t:Sprite", new[] { ActorFolder });
            var actorSprites = actorGuids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p))
                .Where(s => s != null)
                .ToArray();

            var fallback = AssetDatabase.LoadAssetAtPath<Sprite>(FallbackSilhouette);

            Undo.IncrementCurrentGroup();
            int attached = 0;

            for (int i = 0; i < content.childCount; i++)
            {
                var card = content.GetChild(i) as RectTransform;
                if (!card) continue;

                // ensure anchor
                var anchor = card.Find("ActorAnchor") as RectTransform;
                if (!anchor)
                {
                    var go = new GameObject("ActorAnchor", typeof(RectTransform));
                    Undo.RegisterCreatedObjectUndo(go, "Create ActorAnchor");
                    anchor = go.GetComponent<RectTransform>();
                    anchor.SetParent(card, false);
                    anchor.anchorMin = anchor.anchorMax = new Vector2(0.5f, 1f);
                    anchor.pivot = new Vector2(0.5f, 0f);
                    anchor.anchoredPosition = new Vector2(0f, 8f);
                }

                // badge under anchor (Image)
                var badge = anchor.Find("ActorBadge") as RectTransform;
                if (!badge)
                {
                    var go = new GameObject("ActorBadge", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                    Undo.RegisterCreatedObjectUndo(go, "Create ActorBadge");
                    badge = go.GetComponent<RectTransform>();
                    badge.SetParent(anchor, false);
                }

                badge.sizeDelta = new Vector2(156, 156);
                badge.anchorMin = badge.anchorMax = new Vector2(0.5f, 0f);
                badge.pivot = new Vector2(0.5f, 0f);
                badge.anchoredPosition = Vector2.zero;

                var img = badge.GetComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;
                img.type = Image.Type.Simple;

                // pick a sprite for this index (demo rotation)
                Sprite chosen = null;
                if (actorSprites.Length > 0)
                    chosen = actorSprites[i % actorSprites.Length];

                if (!chosen) chosen = fallback;
                if (!chosen)
                {
                    Debug.LogWarning($"⚠️ No actor sprites found in '{ActorFolder}', and fallback not present. Skipping card {card.name}.");
                    continue;
                }

                img.sprite = chosen;

                // ensure badge renders above the card (local order)
                badge.SetAsLastSibling();

                // subtle fade-in (design preview only)
                var cg = badge.GetComponent<CanvasGroup>();
                if (cg) { cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false; }

                attached++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"✅ Attached {attached} actor badge(s). Place portrait PNGs under '{ActorFolder}'.");
        }
    }
}
#endif
