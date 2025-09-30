#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.UI.Leads
{
    public static class EnsureIndexCardOverlay
    {
        // ---- spec ----
        const float ActorPosX     = -115f;
        const float ActorSize     = 132f;
        const float TitlePosY     = -15f;
        const float ReqRowPosY    = -108f;
        const float ReqSlotSize   = 96f;    // transparent holder
        const float ReqIconSize   = 132f;   // visible icon
        const string OverlayName  = "IndexCardOverlay";
        const string IndexCardPathExact = "Assets/Art/UI/Leads/Indexcard_transparent.png";

        [MenuItem("AQ/UI/Leads/IndexCard ▸ Apply to Live (Scene Only)")]
        public static void ApplyLive()
        {
            int cards=0, overlays=0, actors=0, titles=0, rows=0, reqIcons=0;

            foreach (var rt in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (!rt || !rt.gameObject.scene.IsValid()) continue;
                if (!rt.name.StartsWith("LeadCard")) continue; // LeadCard, LeadCard_201, LeadCard(Clone)…

                PatchCard(rt, ref overlays, ref actors, ref titles, ref rows, ref reqIcons);
                cards++;
            }

            if (!Application.isPlaying)
            {
                var s = EditorSceneManager.GetActiveScene();
                if (s.IsValid()) EditorSceneManager.MarkSceneDirty(s);
            }

            Debug.Log($"[AQ IndexCard] live cards={cards} | overlays={overlays} | actors={actors} | titles={titles} | reqRows={rows} | reqIcons={reqIcons}.");
        }

        [MenuItem("AQ/UI/Leads/IndexCard ▸ Apply to LeadCard Prefab(s)")]
        public static void ApplyPrefabs()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab LeadCardView");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:Prefab LeadCard");

            int done = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var root = PrefabUtility.LoadPrefabContents(path);
                if (!root) continue;

                int overlays=0, actors=0, titles=0, rows=0, reqIcons=0;
                PatchCard(root.transform as RectTransform, ref overlays, ref actors, ref titles, ref rows, ref reqIcons);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
                done++;
            }
            Debug.Log($"[AQ IndexCard] prefabs updated={done}.");
        }

        // --------------- core ---------------
        static void PatchCard(RectTransform card,
                              ref int overlays, ref int actors, ref int titles, ref int rows, ref int reqIcons)
        {
            // 1) Keep the root Image tint exactly as-is; add a child overlay for the index-card.
            EnsureOverlay(card); overlays++;

            // 2) ActorAnchor: move X and size; keep current Y (you manually set it before).
            var actor = FindDeep(card, "ActorAnchor");
            if (actor)
            {
                var art = actor.GetComponent<RectTransform>();
                if (art)
                {
                    art.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ActorSize);
                    art.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   ActorSize);
                    var p = art.anchoredPosition; p.x = ActorPosX; art.anchoredPosition = p;
                }
                var aimg = actor.GetComponent<Image>();
                if (aimg) { var c = aimg.color; c.a = 0f; aimg.color = c; aimg.maskable = false; aimg.raycastTarget = false; }
                actors++;
            }

            // 3) Title
            var title = FindDeep(card, "Text_Title");
            if (title)
            {
                var trt = title.GetComponent<RectTransform>();
                if (trt) { var p = trt.anchoredPosition; p.y = TitlePosY; trt.anchoredPosition = p; }
                var tmp = title.GetComponent<TMP_Text>();
                if (tmp) tmp.fontStyle = FontStyles.Bold;
                titles++;
            }

            // 4) Requirements row & icons
            var row = FindDeep(card, "RequirementsRow");
            if (row)
            {
                var rrt = row.GetComponent<RectTransform>();
                if (rrt) { var p = rrt.anchoredPosition; p.y = ReqRowPosY; rrt.anchoredPosition = p; }
                rows++;

                for (int i = 1; i <= 3; i++)
                {
                    var req = FindDeep(row, $"Req_{i}");
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
                        reqIcons++;
                    }
                }
            }
        }

        static void EnsureOverlay(RectTransform card)
        {
            var sprite = LoadIndexCardSprite();
            if (!sprite) return;

            var child = card.Find(OverlayName);
            if (!child)
            {
                var go = new GameObject(OverlayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(card, false);
                child = go.transform;
            }

            var rt = child as RectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = child.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;     // stretch full card
            img.color = Color.white;        // do not kill the card tint beneath
            img.maskable = false;
            img.raycastTarget = false;

            // Render behind other content, but above the root Image (parent draws first)
            rt.SetSiblingIndex(0);
        }

        static Transform FindDeep(Transform root, string n)
            => root ? root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t && t.name == n) : null;

        static Sprite LoadIndexCardSprite()
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(IndexCardPathExact);
            if (s) return s;
            var guid = AssetDatabase.FindAssets("t:Sprite Indexcard_transparent").FirstOrDefault();
            return string.IsNullOrEmpty(guid) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
#endif
