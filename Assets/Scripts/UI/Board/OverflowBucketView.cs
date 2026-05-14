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

        private const float SIZE   = 100f;
        private const float MARGIN = 24f;

        private void Awake()
        {
            BuildHUD();
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

        private void BuildHUD()
        {
            var canvasGO = new GameObject("OverflowCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Bucket root — bottom-right corner
            var bucketGO = new GameObject("BucketRoot");
            bucketGO.transform.SetParent(canvasGO.transform, false);
            _root = bucketGO.AddComponent<RectTransform>();
            _root.sizeDelta = new Vector2(SIZE, SIZE);
            _root.anchorMin = _root.anchorMax = new Vector2(1f, 0f);
            _root.pivot = new Vector2(1f, 0f);
            _root.anchoredPosition = new Vector2(-MARGIN, MARGIN);

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
            var badgeBg = badgeGO.AddComponent<Image>();
            badgeBg.color = new Color(0.85f, 0.15f, 0.15f, 1f);
            _badge = badgeGO.AddComponent<Text>();
            _badge.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _badge.fontSize = 20;
            _badge.fontStyle = FontStyle.Bold;
            _badge.alignment = TextAnchor.MiddleCenter;
            _badge.color = Color.white;

            // Tap target — transparent Image + Button over the whole bucket
            var hitImg = bucketGO.AddComponent<Image>();
            hitImg.color = Color.clear;
            var btn = bucketGO.AddComponent<Button>();
            btn.targetGraphic = hitImg;
            btn.onClick.AddListener(OnTapped);
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
                OverflowBucketService.Pop();
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
    }
}
