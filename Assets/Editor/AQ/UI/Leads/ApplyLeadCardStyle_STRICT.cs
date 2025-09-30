// Assets/Editor/AQ/UI/Leads/ApplyLeadCardStyle_STRICT.cs
//
// Edit-mode utility that force-applies the exact Lead Card styling you asked for,
// using the values taken from your LeadCardExample audit.
//
// What it does per card (scene & prefab):
//   • Keeps the root Image color (white/blue/green), but CLEARS its sprite.
//   • Ensures a stretched "IndexCardOverlay" image (Indexcard_transparent.png) sits BEHIND content.
//   • Title: top padding = 15, height = 40, bold, dark text, left/right padding = 14.
//   • LeadId: top padding = 10, height = 20, right-aligned, dark text, left/right padding = 14.
//   • Objective: top padding = 54, height = 38, dark text, left/right padding = 14.
//   • ActorAnchor: pos (-115, -18), size 132×132, own Image alpha=0; child "Image" size 132×132.
//   • RequirementsRow: y = -108, HorizontalLayoutGroup set; each Req_x size 96×96,
//     outer Image alpha=0, child "Icon" 132×132, Tick off, Label empty.
//   • Removes the stray child named "Image" under the card root if it’s the old overlay.
//
// Run it from the menu:  AQ ▸ UI ▸ Leads ▸ STRICT: Apply style (scene)
//                        AQ ▸ UI ▸ Leads ▸ STRICT: Apply style to LeadCardView prefab
//
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AQ.EditorTools.UI.Leads
{
    public static class ApplyLeadCardStyle_STRICT
    {
        // ----- Constants from your LeadCardExample dump -----
        private const string OverlayName = "IndexCardOverlay";
        private const string OverlaySpritePath = "Assets/Art/UI/Leads/Indexcard_transparent.png";

        // Title / body bars use left/right padding rather than sizeDelta to avoid drifting.
        private const float LRPad = 14f;

        // Y placement (from top) and heights
        private const float TitleTopPad = 15f;
        private const float TitleHeight = 40f;

        private const float LeadIdTopPad = 10f;
        private const float LeadIdHeight = 20f;

        private const float ObjectiveTopPad = 54f;
        private const float ObjectiveHeight = 38f;

        private static readonly Vector2 ActorAnchorPos = new Vector2(-115, -18);
        private static readonly Vector2 ActorAnchorSize = new Vector2(132, 132);
        private static readonly Vector2 ReqRowPos = new Vector2(0, -108);
        private static readonly Vector2 ReqSize = new Vector2(96, 96);
        private static readonly Vector2 IconSize = new Vector2(132, 132);

        private static readonly Color32 TitleColor = new Color32(0x11, 0x11, 0x11, 0xFF);
        private static readonly Color32 BodyColor = new Color32(0x33, 0x33, 0x33, 0xFF);
        private static readonly Color32 OverlayTint = new Color32(0xFF, 0xFF, 0xFF, 0x4C); // ~30% white

        // ----- Entry points -----
        [MenuItem("AQ/UI/Leads/STRICT: Apply style (scene)")]
        public static void StyleSceneCards()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AQ STRICT] Edit-mode only. Exit Play Mode and run again.");
                return;
            }

            var content = GameObject.Find("Canvas_Board/HUD_Board/LeadsBar/Viewport/Content_Leads")
                          ?? GameObject.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                              .FirstOrDefault(rt => rt.name == "Content_Leads")?.gameObject;

            if (content == null)
            {
                Debug.LogError("[AQ STRICT] Could not find LeadsBar/Viewport/Content_Leads in the open scene.");
                return;
            }

            int changed = 0;
            foreach (var card in content.GetComponentsInChildren<RectTransform>(true))
            {
                if (!card || (!card.name.StartsWith("LeadCard") && card.name != "LeadCardExample"))
                    continue;

                if (ApplyStyleToCard(card))
                    changed++;
            }

            if (changed > 0)
            {
                EditorSceneManager.MarkSceneDirty(content.scene);
                Debug.Log($"[AQ STRICT] Styled {changed} card(s) in scene.");
            }
            else
            {
                Debug.Log("[AQ STRICT] No matching cards were changed.");
            }
        }

        [MenuItem("AQ/UI/Leads/STRICT: Apply style to LeadCardView prefab")]
        public static void StylePrefab()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AQ STRICT] Edit-mode only. Exit Play Mode and run again.");
                return;
            }

            // Look for a likely LeadCardView prefab anywhere in Assets.
            var guids = AssetDatabase.FindAssets("LeadCardView t:Prefab");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("LeadCard t:Prefab");
            if (guids.Length == 0)
            {
                Debug.LogError("[AQ STRICT] Could not find a LeadCardView prefab in the project.");
                return;
            }

            int updated = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                if (!root) continue;

                var card = root.GetComponent<RectTransform>();
                if (card && ApplyStyleToCard(card))
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    updated++;
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            Debug.Log(updated > 0
                ? $"[AQ STRICT] Styled {updated} prefab(s)."
                : "[AQ STRICT] No matching prefabs were changed.");
        }

        // ----- Core styling -----
        private static bool ApplyStyleToCard(RectTransform card)
        {
            bool changed = false;

            // 1) Root Image: keep color, but clear sprite.
            var rootImg = card.GetComponent<Image>();
            if (rootImg != null && rootImg.sprite != null)
            {
                rootImg.sprite = null;
                changed = true;
            }

            // Remove stray child named "Image" if it was the old overlay using our sprite.
            var stray = card.transform.Cast<Transform>().FirstOrDefault(t => t.name == "Image");
            if (stray != null)
            {
                var si = stray.GetComponent<Image>();
                var overlaySprite = AssetDatabase.LoadAssetAtPath<Sprite>(OverlaySpritePath);
                if (si != null && si.sprite == overlaySprite)
                {
                    Object.DestroyImmediate(stray.gameObject);
                    changed = true;
                }
            }

            // 2) Overlay (stretched full, behind everything).
            var overlayGO = EnsureChild(card, OverlayName, out var overlayRT);
            var overlayImg = overlayGO.GetComponent<Image>() ?? overlayGO.gameObject.AddComponent<Image>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OverlaySpritePath);

            overlayImg.sprite = sprite;
            overlayImg.type = Image.Type.Simple;
            overlayImg.preserveAspect = false; // must stretch to the card
            overlayImg.color = OverlayTint;
            overlayImg.maskable = false;
            overlayImg.raycastTarget = false;

            // Stretch to full parent using offsets (robust to pivots).
            SetStretchFull(overlayRT);
            overlayGO.transform.SetSiblingIndex(0); // send behind

            // 3) Title
            var title = card.Find("Text_Title") as RectTransform;
            if (title != null)
            {
                SetTopBarRect(title, TitleTopPad, TitleHeight, LRPad);
                var tmp = title.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = TitleColor;
                    tmp.fontStyle |= FontStyles.Bold;
                    tmp.alignment = TextAlignmentOptions.Left;
                }
            }

            // 4) LeadId (right aligned)
            var leadId = card.Find("Text_LeadId") as RectTransform;
            if (leadId != null)
            {
                SetTopBarRect(leadId, LeadIdTopPad, LeadIdHeight, LRPad);
                var tmp = leadId.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = BodyColor;
                    tmp.alignment = TextAlignmentOptions.Right;
                }
            }

            // 5) Objective
            var objective = card.Find("Text_Objective") as RectTransform;
            if (objective != null)
            {
                SetTopBarRect(objective, ObjectiveTopPad, ObjectiveHeight, LRPad);
                var tmp = objective.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = BodyColor;
                    tmp.alignment = TextAlignmentOptions.Left;
                }
            }

            // 6) ActorAnchor
            var actorAnchor = card.Find("ActorAnchor") as RectTransform;
            if (actorAnchor != null)
            {
                actorAnchor.anchorMin = actorAnchor.anchorMax = new Vector2(0.5f, 1f);
                actorAnchor.pivot = new Vector2(0.5f, 0f);
                actorAnchor.anchoredPosition = ActorAnchorPos;
                actorAnchor.sizeDelta = ActorAnchorSize;

                var anchorImg = actorAnchor.GetComponent<Image>();
                if (anchorImg != null)
                {
                    var c = anchorImg.color; c.a = 0f; anchorImg.color = c;
                    anchorImg.raycastTarget = false;
                }

                var actorImgRT = actorAnchor.Find("Image") as RectTransform;
                if (actorImgRT != null)
                {
                    actorImgRT.anchorMin = actorImgRT.anchorMax = new Vector2(0.5f, 0.5f);
                    actorImgRT.pivot = new Vector2(0.5f, 0.5f);
                    actorImgRT.anchoredPosition = Vector2.zero;
                    actorImgRT.sizeDelta = ActorAnchorSize;

                    var img = actorImgRT.GetComponent<Image>();
                    if (img != null) img.raycastTarget = false;
                }
            }

            // 7) RequirementsRow & children
            var reqRow = card.Find("RequirementsRow") as RectTransform;
            if (reqRow != null)
            {
                reqRow.anchorMin = new Vector2(0f, 1f);
                reqRow.anchorMax = new Vector2(1f, 1f);
                reqRow.pivot = new Vector2(0.5f, 1f);
                reqRow.anchoredPosition = ReqRowPos;

                var hlg = reqRow.GetComponent<HorizontalLayoutGroup>() ?? reqRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 8;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.padding = new RectOffset(0, 0, 0, 0);

                StyleReq(reqRow, "Req_1");
                StyleReq(reqRow, "Req_2");
                StyleReq(reqRow, "Req_3");
            }

            return true; // we set values deterministically
        }

        // ----- Helpers -----

        // Robust "stretch full" regardless of parent's pivot/size—this avoids the overlay not filling.
        private static void SetStretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero; // left,bottom
            rt.offsetMax = Vector2.zero; // -right,-top
        }

        // Places a top-anchored bar with explicit left/right padding and exact height.
        // This fixes the X drift you saw when using sizeDelta with stretch anchors.
        private static void SetTopBarRect(RectTransform rt, float topPadding, float height, float lrPadding)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);

            // With both anchors at Y=1, offsetMax.y is -top, offsetMin.y is -(top+height)
            var offMin = rt.offsetMin;
            var offMax = rt.offsetMax;
            offMin.x = lrPadding;    // left
            offMax.x = -lrPadding;   // right
            offMax.y = -topPadding;                 // top
            offMin.y = -(topPadding + height);      // bottom
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
        }

        private static void StyleReq(Transform reqRow, string childName)
        {
            var cell = reqRow.Find(childName) as RectTransform;
            if (cell == null) return;

            cell.sizeDelta = ReqSize;

            var le = cell.GetComponent<LayoutElement>() ?? cell.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = ReqSize.x;
            le.preferredHeight = ReqSize.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            var cellImg = cell.GetComponent<Image>() ?? cell.gameObject.AddComponent<Image>();
            var cc = cellImg.color; cc.a = 0f; cellImg.color = cc; // invisible hit area
            cellImg.raycastTarget = true;

            var tick = cell.Find("Tick");
            if (tick && tick.gameObject.activeSelf) tick.gameObject.SetActive(false);

            var labelRT = cell.Find("Label") as RectTransform;
            if (labelRT != null)
            {
                labelRT.anchorMin = labelRT.anchorMax = new Vector2(0.5f, 0f);
                labelRT.pivot = new Vector2(0.5f, 0f);
                labelRT.anchoredPosition = new Vector2(0, -16);
                labelRT.sizeDelta = new Vector2(200, 50);

                var tmp = labelRT.GetComponent<TextMeshProUGUI>();
                if (tmp)
                {
                    tmp.text = string.Empty;
                    tmp.color = new Color32(0xCC, 0xE0, 0xF5, 0xFF);
                    tmp.alignment = TextAlignmentOptions.Center;
                }
            }

            var iconRT = cell.Find("Icon") as RectTransform;
            if (iconRT == null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(cell, false);
                iconRT = iconGO.GetComponent<RectTransform>();
            }

            iconRT.anchorMin = iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = IconSize;

            var iconImg = iconRT.GetComponent<Image>();
            if (iconImg)
            {
                iconImg.type = Image.Type.Simple;
                iconImg.preserveAspect = false;
                iconImg.maskable = false;
                iconImg.raycastTarget = false;
            }
        }

        private static GameObject EnsureChild(RectTransform parent, string name, out RectTransform rt)
        {
            var t = parent.Find(name) as RectTransform;
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(parent, false);
                rt = go.GetComponent<RectTransform>();
                return go;
            }
            rt = t;
            return t.gameObject;
        }
    }
}
#endif
