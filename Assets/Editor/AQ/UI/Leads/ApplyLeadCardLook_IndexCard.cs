#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class ApplyLeadCardLook_IndexCard
    {
        // Spec
        const float ActorPosX = -115f;
        const float ActorSize = 132f;
        const float TitlePosY = -15f;
        const float ReqRowPosY = -108f;
        const float ReqSlotSize = 96f;     // transparent holder
        const float ReqIconSize = 132f;    // NEW: bigger visible icon

        [MenuItem("AQ/UI/Leads/Apply IndexCard Look ▸ Live (Scene Only)")]
        public static void StyleLiveCards()
        {
            int cards = 0, bgOK = 0, actorOK = 0, titleOK = 0, reqRowOK = 0, reqOK = 0;

            foreach (var rt in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (!rt || !rt.gameObject.scene.IsValid()) continue;
                if (!rt.name.StartsWith("LeadCard")) continue;  // LeadCard, LeadCard_201, LeadCard(Clone)…

                cards++;

                // 1) Card background: keep tint, stretch sprite
                var cardImg = rt.GetComponent<Image>();
                var sprite = LoadIndexCardSprite();
                if (cardImg && sprite)
                {
                    var oldTint = cardImg.color;          // preserve white/blue/green
                    cardImg.sprite = sprite;
                    cardImg.type = Image.Type.Simple;
                    cardImg.preserveAspect = false;       // NEW: stretch to full rect
                    cardImg.color = oldTint;              // reapply tint
                    bgOK++;
                }

                // 2) ActorAnchor
                var actor = FindDeep(rt, "ActorAnchor");
                if (actor)
                {
                    var aRT = actor.GetComponent<RectTransform>();
                    var aImg = actor.GetComponent<Image>();
                    if (aRT)
                    {
                        var p = aRT.anchoredPosition;
                        p.x = ActorPosX;
                        aRT.anchoredPosition = p;
                        aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActorSize);
                        aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ActorSize);
                    }
                    if (aImg)
                    {
                        var c = aImg.color; c.a = 0f; aImg.color = c; // hide placeholder
                        aImg.raycastTarget = false;
                        aImg.maskable = false;
                    }
                    actorOK++;
                }

                // 3) Title
                var title = FindDeep(rt, "Text_Title");
                if (title)
                {
                    var tRT = title.GetComponent<RectTransform>();
                    if (tRT)
                    {
                        var p = tRT.anchoredPosition; p.y = TitlePosY; tRT.anchoredPosition = p;
                    }
                    var tmp = title.GetComponent<TMP_Text>();
                    if (tmp) tmp.fontStyle = FontStyles.Bold;
                    titleOK++;
                }

                // 4) Requirements row + cells
                var reqRow = FindDeep(rt, "RequirementsRow");
                if (reqRow)
                {
                    var rrRT = reqRow.GetComponent<RectTransform>();
                    if (rrRT)
                    {
                        var p = rrRT.anchoredPosition; p.y = ReqRowPosY; rrRT.anchoredPosition = p;
                    }
                    reqRowOK++;

                    for (int i = 1; i <= 3; i++)
                    {
                        var req = FindDeep(reqRow, $"Req_{i}");
                        if (!req) continue;

                        // Slot holder (transparent, 96x96)
                        var reqRT = req.GetComponent<RectTransform>();
                        if (reqRT)
                        {
                            reqRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ReqSlotSize);
                            reqRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ReqSlotSize);
                        }
                        var reqImg = req.GetComponent<Image>();
                        if (reqImg)
                        {
                            var c = reqImg.color; c.a = 0f; reqImg.color = c;
                        }

                        // Icon (visible, 132x132)
                        var icon = FindDeep(req, "Icon");
                        if (icon)
                        {
                            var iRT = icon.GetComponent<RectTransform>();
                            if (iRT)
                            {
                                iRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ReqIconSize);
                                iRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ReqIconSize);
                                // center it in the (smaller) slot so it can overflow
                                iRT.anchoredPosition = Vector2.zero;
                            }
                            var iImg = icon.GetComponent<Image>();
                            if (iImg) { iImg.maskable = false; } // avoid being clipped by the slot
                        }

                        reqOK++;
                    }
                }
            }

            if (!Application.isPlaying)
            {
                var scene = EditorSceneManager.GetActiveScene();
                if (scene.IsValid()) EditorSceneManager.MarkSceneDirty(scene);
            }

            Debug.Log($"[AQ IndexCardLook] Live cards styled: {cards} | bg={bgOK} actor={actorOK} title={titleOK} reqRow={reqRowOK} reqCells={reqOK}.");
        }

        [MenuItem("AQ/UI/Leads/Apply IndexCard Look ▸ LeadCard Prefab(s)")]
        public static void StylePrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab LeadCardView");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Prefab LeadCard");

            int done = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                var root = PrefabUtility.LoadPrefabContents(path);
                if (!root) continue;

                StyleSingleCard(root.transform);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
                done++;
            }
            Debug.Log($"[AQ IndexCardLook] Prefabs styled: {done}.");
        }

        // ---------- helpers ----------
        static void StyleSingleCard(Transform card)
        {
            var rt = card.GetComponent<RectTransform>();
            var img = card.GetComponent<Image>();
            var sprite = LoadIndexCardSprite();
            if (rt && img && sprite)
            {
                var oldTint = img.color;
                img.sprite = sprite;
                img.type = Image.Type.Simple;
                img.preserveAspect = false; // stretch
                img.color = oldTint;        // keep tint
            }

            var actor = FindDeep(card, "ActorAnchor");
            if (actor)
            {
                var aRT = actor.GetComponent<RectTransform>();
                var aImg = actor.GetComponent<Image>();
                if (aRT)
                {
                    var p = aRT.anchoredPosition; p.x = ActorPosX; aRT.anchoredPosition = p;
                    aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActorSize);
                    aRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ActorSize);
                }
                if (aImg) { var c = aImg.color; c.a = 0f; aImg.color = c; aImg.raycastTarget = false; aImg.maskable = false; }
            }

            var title = FindDeep(card, "Text_Title");
            if (title)
            {
                var tRT = title.GetComponent<RectTransform>();
                if (tRT) { var p = tRT.anchoredPosition; p.y = TitlePosY; tRT.anchoredPosition = p; }
                var tmp = title.GetComponent<TMP_Text>();
                if (tmp) tmp.fontStyle = FontStyles.Bold;
            }

            var reqRow = FindDeep(card, "RequirementsRow");
            if (reqRow)
            {
                var rrRT = reqRow.GetComponent<RectTransform>();
                if (rrRT) { var p = rrRT.anchoredPosition; p.y = ReqRowPosY; rrRT.anchoredPosition = p; }

                for (int i = 1; i <= 3; i++)
                {
                    var req = FindDeep(reqRow, $"Req_{i}");
                    if (!req) continue;

                    var reqRT = req.GetComponent<RectTransform>();
                    if (reqRT)
                    {
                        reqRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ReqSlotSize);
                        reqRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ReqSlotSize);
                    }

                    var reqImg = req.GetComponent<Image>();
                    if (reqImg) { var c = reqImg.color; c.a = 0f; reqImg.color = c; }

                    var icon = FindDeep(req, "Icon");
                    if (icon)
                    {
                        var iRT = icon.GetComponent<RectTransform>();
                        if (iRT)
                        {
                            iRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ReqIconSize);
                            iRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ReqIconSize);
                            iRT.anchoredPosition = Vector2.zero;
                        }
                        var iImg = icon.GetComponent<Image>();
                        if (iImg) iImg.maskable = false;
                    }
                }
            }
        }

        static Transform FindDeep(Transform root, string name)
            => root ? root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t && t.name == name) : null;

        static Sprite LoadIndexCardSprite()
        {
            const string exact = "Assets/Art/UI/Leads/Indexcard_transparent.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(exact);
            if (sprite) return sprite;

            var guid = AssetDatabase.FindAssets("t:Sprite Indexcard_transparent").FirstOrDefault();
            return string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
#endif
