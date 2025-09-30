#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.UI.Leads
{
    public static class ApplyLeadCardStyleFromExample
    {
        private const string ExampleName = "LeadCardExample";
        private const string OverlayName = "IndexCardOverlay";
        private const string OverlaySearch = "Indexcard_transparent t:sprite"; // from art/ui/leads/Indexcard_transparent.png

        [MenuItem("AQ/UI/Leads/Apply style from LeadCardExample to sibling demo cards")]
        public static void Run()
        {
            // 1) Find the example card in scene
            var example = Resources.FindObjectsOfTypeAll<RectTransform>()
                .FirstOrDefault(t => t && t.gameObject.scene.IsValid() && t.name == ExampleName);
            if (!example)
            {
                Debug.LogWarning("[AQ ApplyStyle] Couldn’t find 'LeadCardExample' in the open scene.");
                return;
            }

            // Gather sibling cards (demo clones)
            var parent = example.parent;
            if (!parent)
            {
                Debug.LogWarning("[AQ ApplyStyle] Example has no parent, aborting.");
                return;
            }
            var targets = parent.GetComponentsInChildren<RectTransform>(true)
                .Where(t => t.parent == parent && t.name.StartsWith("LeadCard") && t.name != ExampleName)
                .ToArray();

            if (targets.Length == 0)
            {
                Debug.Log("[AQ ApplyStyle] No other LeadCard siblings found to update.");
                return;
            }

            // Pull values from the example
            // Title
            var exTitle = example.Find("Text_Title") as RectTransform;
            var exTitleTMP = exTitle ? exTitle.GetComponent<TMP_Text>() : null;

            // Actor
            var exActor = example.Find("ActorAnchor") as RectTransform;
            var exActorRing = exActor ? exActor.GetComponent<Image>() : null;
            var exActorImage = exActor ? exActor.Find("Image")?.GetComponent<RectTransform>() : null;

            // Requirements row
            var exReqRow = example.Find("RequirementsRow") as RectTransform;

            // Requirement sample (Req_1)
            RectTransform exReqSlot = null, exReqIcon = null;
            Image exReqSlotImg = null;
            if (exReqRow)
            {
                exReqSlot = exReqRow.Find("Req_1") as RectTransform;
                exReqSlotImg = exReqSlot ? exReqSlot.GetComponent<Image>() : null;
                exReqIcon = exReqSlot ? exReqSlot.Find("Icon") as RectTransform : null;
            }

            // Overlay sprite
            var overlaySprite = FindOverlaySprite();

            int updated = 0;
            foreach (var card in targets)
            {
                // --- Index card overlay (create if missing) ---
                var overlay = card.Find(OverlayName)?.GetComponent<RectTransform>();
                if (!overlay)
                {
                    var go = new GameObject(OverlayName, typeof(RectTransform), typeof(Image));
                    overlay = go.GetComponent<RectTransform>();
                    overlay.SetParent(card, false);
                    overlay.SetSiblingIndex(0); // behind everything
                }
                var overlayImg = overlay.GetComponent<Image>();
                if (overlayImg)
                {
                    overlayImg.sprite = overlaySprite;
                    overlayImg.type = Image.Type.Simple;
                    overlayImg.preserveAspect = false;
                    overlayImg.color = new Color32(255, 255, 255, 0x4C); // #FFFFFF4C like the example
                    overlayImg.maskable = false;
                    overlayImg.raycastTarget = false;
                }
                // Match the example overlay rect if it exists, otherwise keep sensible defaults
                var exOverlay = example.Find(OverlayName) as RectTransform;
                if (exOverlay)
                    CopyRect(exOverlay, overlay);
                else
                {
                    // safe fallback (centered, stretches visually across the card)
                    overlay.anchorMin = overlay.anchorMax = new Vector2(0.5f, 0.5f);
                    overlay.pivot = new Vector2(0.5f, 0.5f);
                    overlay.anchoredPosition = new Vector2(3f, 0f);
                    overlay.sizeDelta = new Vector2(585f, 410f);
                }

                // --- Title ---
                var title = card.Find("Text_Title") as RectTransform;
                var titleTMP = title ? title.GetComponent<TMP_Text>() : null;
                if (title && exTitle) { CopyRect(exTitle, title); }
                if (titleTMP && exTitleTMP)
                {
                    titleTMP.fontSize = exTitleTMP.fontSize;    // 32
                    titleTMP.alignment = exTitleTMP.alignment;  // Left
                    // force bold + dark color as requested
                    titleTMP.fontStyle |= FontStyles.Bold;
                    titleTMP.color = new Color32(0x11, 0x11, 0x11, 0xFF);
                }

                // --- Actor anchor + portrait ---
                var actor = card.Find("ActorAnchor") as RectTransform;
                if (actor && exActor) { CopyRect(exActor, actor); }
                var actorRing = actor ? actor.GetComponent<Image>() : null;
                if (actorRing)
                {
                    actorRing.color = new Color32(255, 255, 255, 0); // alpha 0
                    actorRing.maskable = false;
                    actorRing.raycastTarget = false;
                }
                var actorImage = actor ? actor.Find("Image") as RectTransform : null;
                if (actorImage && exActorImage) { CopyRect(exActorImage, actorImage); }
                actor?.SetAsLastSibling(); // make sure it’s over the overlay/background

                // --- Requirements row ---
                var reqRow = card.Find("RequirementsRow") as RectTransform;
                if (reqRow && exReqRow) { CopyRect(exReqRow, reqRow); }

                // normalize each slot
                for (int i = 1; i <= 3; i++)
                {
                    var req = reqRow ? reqRow.Find($"Req_{i}") as RectTransform : null;
                    if (!req) continue;

                    if (exReqSlot) CopyRect(exReqSlot, req);

                    var reqImg = req.GetComponent<Image>();
                    if (reqImg)
                    {
                        // transparent background (alpha 0), keep other channels
                        reqImg.color = new Color(reqImg.color.r, reqImg.color.g, reqImg.color.b, 0f);
                    }

                    var tick = req.Find("Tick")?.gameObject;
                    if (tick) tick.SetActive(false);

                    var label = req.Find("Label")?.GetComponent<TMP_Text>();
                    if (label)
                    {
                        label.alignment = TextAlignmentOptions.Center;
                        // leave text & color alone (you may style later)
                    }

                    var icon = req.Find("Icon") as RectTransform;
                    if (icon && exReqIcon) { CopyRect(exReqIcon, icon); }
                    // Make sure the icon is 132×132 even if the example changes later
                    if (icon) icon.sizeDelta = new Vector2(132, 132);
                    var iconImg = icon ? icon.GetComponent<Image>() : null;
                    if (iconImg)
                    {
                        iconImg.maskable = false;
                        iconImg.raycastTarget = false;
                        iconImg.preserveAspect = false;
                    }
                }

                updated++;
            }

            if (updated > 0)
                EditorSceneManager.MarkSceneDirty(example.gameObject.scene);

            Debug.Log($"[AQ ApplyStyle] Updated {updated} demo cards from '{ExampleName}'. " +
                      $"Background colors were preserved; requirement icons set to 132×132; overlay ensured.");
        }

        // ----- helpers -----
        static void CopyRect(RectTransform src, RectTransform dst)
        {
            dst.anchorMin = src.anchorMin;
            dst.anchorMax = src.anchorMax;
            dst.pivot     = src.pivot;
            dst.anchoredPosition = src.anchoredPosition;
            dst.sizeDelta = src.sizeDelta;
            dst.localRotation = src.localRotation;
            dst.localScale = src.localScale;
        }

        static Sprite FindOverlaySprite()
        {
            string guid = AssetDatabase.FindAssets(OverlaySearch).FirstOrDefault();
            if (string.IsNullOrEmpty(guid)) return null;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
