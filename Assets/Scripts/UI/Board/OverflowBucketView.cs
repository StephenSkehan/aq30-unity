using AQ.App.Audio;
using AQ.App.Generators;
using AQ.App.Overflow;
using AQ.App.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.App.UI.Board
{
    public class OverflowBucketView : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            OverflowBucketService.Load();
            GeneratorFamilyRegistry.Load();

            var go = new GameObject("[OverflowBucketView]");
            DontDestroyOnLoad(go);
            go.AddComponent<OverflowBucketView>();
        }

        private RectTransform _root;
        private Image _itemIcon;
        private Text _badge;

        // Sits beside the locker button in the bottom strip, same size as the
        // locker/evidence buttons (142 = board design cell, grid-square parity
        // Stephen-ruled 2026-07-20; it lived alone on its own row before).
        private const float SIZE = 142f;

        private void Awake()
        {
            try { BuildHUD(); }
            catch (System.Exception e) { Debug.LogError($"[OverflowBucket] BuildHUD failed: {e}"); }
        }

        private void OnEnable()
        {
            OverflowBucketService.BucketChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            OverflowBucketService.BucketChanged -= Refresh;
        }

        private void Update()
        {
            if (OverflowBucketService.IsEmpty || _root == null || !_root.gameObject.activeSelf) return;

            bool tapped = false;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                tapped = RectContains(Input.GetTouch(0).position);
            else if (Input.GetMouseButtonDown(0))
                tapped = RectContains(Input.mousePosition);

            if (tapped) OnTapped();
        }

        private bool RectContains(Vector2 screenPos)
        {
            if (_root == null) return false;
            var corners = new Vector3[4];
            _root.GetWorldCorners(corners);
            return screenPos.x >= corners[0].x && screenPos.x <= corners[2].x &&
                   screenPos.y >= corners[0].y && screenPos.y <= corners[2].y;
        }

        private void BuildHUD()
        {
            var canvasGO = new GameObject("OverflowCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            // Match the locker/evidence button canvases so sizes track together.
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Bucket root — beside the locker button (locker spans x 24..166 at y 219)
            var bucketGO = new GameObject("BucketRoot");
            bucketGO.transform.SetParent(canvasGO.transform, false);
            _root = bucketGO.AddComponent<RectTransform>();
            _root.sizeDelta = new Vector2(SIZE, SIZE);
            _root.anchorMin = _root.anchorMax = new Vector2(0f, 0f);
            _root.pivot = new Vector2(0f, 0f);
            _root.anchoredPosition = new Vector2(178f, 219f);

            // Translucent background
            var bg = MakeImage(_root, "BucketBg");
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            Stretch(bg.rectTransform);

            // Item icon (inset 8px each side)
            _itemIcon = MakeImage(_root, "ItemIcon");
            _itemIcon.color = Color.white;
            _itemIcon.preserveAspect = true;
            _itemIcon.rectTransform.anchorMin = Vector2.zero;
            _itemIcon.rectTransform.anchorMax = Vector2.one;
            _itemIcon.rectTransform.offsetMin = new Vector2(8f, 8f);
            _itemIcon.rectTransform.offsetMax = new Vector2(-8f, -8f);

            // Count badge (top-right corner of bucket)
            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(_root, false);
            var badgeRT = badgeGO.AddComponent<RectTransform>();
            badgeRT.sizeDelta = new Vector2(36f, 36f);
            badgeRT.anchorMin = badgeRT.anchorMax = new Vector2(1f, 1f);
            badgeRT.pivot = new Vector2(1f, 1f);
            badgeRT.anchoredPosition = new Vector2(8f, -8f);

            // Background on a child so Image and Text aren't on the same GO (only one Graphic allowed per GO)
            var badgeBgGO = new GameObject("BadgeBg");
            badgeBgGO.transform.SetParent(badgeGO.transform, false);
            var badgeBgRT = badgeBgGO.AddComponent<RectTransform>();
            Stretch(badgeBgRT);
            var badgeBg = badgeBgGO.AddComponent<Image>();
            badgeBg.color = new Color(0.85f, 0.15f, 0.15f, 1f);

            _badge = badgeGO.AddComponent<Text>();
            _badge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _badge.fontSize = 20;
            _badge.fontStyle = FontStyle.Bold;
            _badge.alignment = TextAnchor.MiddleCenter;
            _badge.color = Color.white;

            // No Button — input handled via raw Update() poll (EventSystem GR unreliable on dynamic overlays)
        }

        private void Refresh()
        {
            if (_root == null) return;

            bool hasContent = !OverflowBucketService.IsEmpty;
            _root.gameObject.SetActive(hasContent);
            if (!hasContent) return;

            var top = OverflowBucketService.Peek();
            if (!top.HasValue) return;

            _itemIcon.sprite = ResolveSprite(top.Value);
            _itemIcon.enabled = _itemIcon.sprite != null;

            int count = OverflowBucketService.Count;
            _badge.gameObject.SetActive(count > 1);
            if (count > 1) _badge.text = $"×{count}";
        }

        private void OnTapped()
        {
            var top = OverflowBucketService.Peek();
            if (!top.HasValue) return;

            var board = FindFirstObjectByType<MergeBoardController>();
            if (board == null) return;

            if (board.PlaceFromOverflow(top.Value))
            {
                OverflowBucketService.Pop();
                UISfxService.PlayOverflowDrop();
            }
            else
                ToastService.Show("board_full", "Board full — free a slot first.", 2f);
        }

        private static Sprite ResolveSprite(OverflowTileData data)
        {
            var board = FindFirstObjectByType<MergeBoardController>();
            if (board == null) return null;

            if (data.kind == OverflowKind.Generator)
            {
                var so = board.FindGeneratorType(data.family);
                return so != null ? so.SpriteForTier(data.tier) : board.generatorSprite;
            }

            return board.SpriteForItemTierPublic(data.tier);
        }

        private static Image MakeImage(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<Image>();
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Clear Stack")]
        private void Debug_ClearStack()
        {
            OverflowBucketService.Clear();
            Debug.Log("[OverflowBucket] Stack cleared.");
        }

        [ContextMenu("Debug: Push gen_junk T0")]
        private void Debug_PushGenJunk()
        {
            Debug.Log($"[OverflowBucket] ContextMenu: Push gen_junk T0. Stack before: {OverflowBucketService.Count}");
            OverflowBucketService.Push(new OverflowTileData { kind = OverflowKind.Generator, family = "gen_junk", tier = 0 });
            Debug.Log($"[OverflowBucket] ContextMenu: Push complete. Stack after: {OverflowBucketService.Count}");
        }

        [ContextMenu("Debug: Push gen_investigation_lab T0")]
        private void Debug_PushInvestigationLab()
        {
            Debug.Log($"[OverflowBucket] ContextMenu: Push gen_investigation_lab T0. Stack before: {OverflowBucketService.Count}");
            OverflowBucketService.Push(new OverflowTileData { kind = OverflowKind.Generator, family = "gen_investigation_lab", tier = 0 });
            Debug.Log($"[OverflowBucket] ContextMenu: Push complete. Stack after: {OverflowBucketService.Count}");
        }
#endif
    }
}
