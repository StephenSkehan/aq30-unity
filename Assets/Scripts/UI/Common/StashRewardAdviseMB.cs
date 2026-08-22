using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.Overflow;
using AQ.App.UI.Board;

namespace AQ.App.UI.Common
{
    /// <summary>
    /// Stash reward advisory (Stephen-ruled 2026-08-22): rewards were slipping
    /// into the Stash unremarked. Any announced reward push now queues a small
    /// popup on the board screen — icon, name, "Sent to your Stash." — and on
    /// close the icon flies from the popup to the Stash button, so the player
    /// SEES where their prize went. Queued rewards wait out dialogues and
    /// full-screen overlays; multiples show one after another.
    /// </summary>
    public sealed class StashRewardAdviseMB : MonoBehaviour
    {
        private static StashRewardAdviseMB _inst;
        private readonly Queue<OverflowTileData> _queue = new();
        private GameObject _popup;
        private RectTransform _iconRt;
        private Sprite _iconSprite;
        private bool _dialogueOpen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Install();
            SceneManager.sceneLoaded += (_, __) => Install();
        }

        private static void Install()
        {
            if (_inst != null) return;
            var go = new GameObject("__StashRewardAdvise");
            DontDestroyOnLoad(go);
            _inst = go.AddComponent<StashRewardAdviseMB>();
        }

        private void OnEnable()
        {
            OverflowBucketService.RewardArrived += OnReward;
            DialogueRunner.DialogueOpened += OnDialogueOpened;
            DialogueRunner.DialogueClosed += OnDialogueClosed;
        }

        private void OnDisable()
        {
            OverflowBucketService.RewardArrived -= OnReward;
            DialogueRunner.DialogueOpened -= OnDialogueOpened;
            DialogueRunner.DialogueClosed -= OnDialogueClosed;
            if (_inst == this) _inst = null;
        }

        private void OnReward(OverflowTileData tile) => _queue.Enqueue(tile);
        private void OnDialogueOpened(CaseGraph _) => _dialogueOpen = true;
        private void OnDialogueClosed() => _dialogueOpen = false;

        private void Update()
        {
            if (_popup != null || _queue.Count == 0) return;
            if (_dialogueOpen || AQ.App.UI.StudioSplashMB.Showing) return;
            if (EvidenceBoard.EvidenceBoardScreen.IsOpen || LockerScreen.IsOpen) return;
            ShowNext(_queue.Dequeue());
        }

        private void ShowNext(OverflowTileData tile)
        {
            var board = Object.FindFirstObjectByType<MergeBoardController>();
            _iconSprite = ResolveSprite(board, tile);
            string name = ResolveName(board, tile);

            _popup = new GameObject("__StashRewardPopup", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = _popup.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9500;
            var scaler = _popup.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(_popup.transform, false);
            var drt = (RectTransform)dim.transform;
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.offsetMin = drt.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = AQTheme.Scrim;

            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_popup.transform, false);
            var prt = (RectTransform)panel.transform;
            AQTheme.StylePanel(prt);
            AQTheme.PopIn(prt);
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(640f, 620f);

            // Copy DRAFT pending ruling (title + caption).
            AQTheme.TitleBar(prt, "TO THE STASH", () => StartCoroutine(CloseWithFlight()));

            if (_iconSprite != null)
            {
                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(prt, false);
                _iconRt = (RectTransform)icon.transform;
                _iconRt.anchorMin = _iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                _iconRt.sizeDelta = new Vector2(190f, 190f);
                _iconRt.anchoredPosition = new Vector2(0f, 60f);
                var img = icon.GetComponent<Image>();
                img.sprite = _iconSprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
            }

            AddLabel(prt, name, 40f, AQTheme.Paper, new Vector2(0f, -80f), new Vector2(560f, 60f), display: true);
            AddLabel(prt, "Sent to your Stash. Place it when there is room.",
                     28f, AQTheme.PaperDim, new Vector2(0f, -140f), new Vector2(540f, 70f), display: false);

            var btnGo = new GameObject("OkBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(prt, false);
            var brt = (RectTransform)btnGo.transform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 0f);
            brt.sizeDelta = new Vector2(320f, 96f);
            brt.anchoredPosition = new Vector2(0f, 28f);
            AQTheme.StyleButton(btnGo.GetComponent<Image>(), AQTheme.Teal);
            btnGo.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseWithFlight()));

            var bl = new GameObject("Label", typeof(RectTransform));
            bl.transform.SetParent(brt, false);
            var blrt = (RectTransform)bl.transform;
            blrt.anchorMin = Vector2.zero;
            blrt.anchorMax = Vector2.one;
            blrt.offsetMin = blrt.offsetMax = Vector2.zero;
            var btext = bl.AddComponent<TextMeshProUGUI>();
            btext.text = "OK";
            btext.fontSize = 38f;
            btext.alignment = TextAlignmentOptions.Center;
            AQTheme.StyleText(btext, display: true);
        }

        private IEnumerator CloseWithFlight()
        {
            // Capture flight start before tearing the popup down.
            Vector3 start = _iconRt != null ? _iconRt.position : Vector3.zero;
            var sprite = _iconSprite;
            var size = _iconRt != null ? _iconRt.rect.size * GetScale(_iconRt) : new Vector2(120f, 120f);
            if (_popup != null) Destroy(_popup);
            _popup = null;
            _iconRt = null;
            if (sprite == null) yield break;

            var stash = GameObject.Find("BucketRoot");
            if (stash == null) yield break;
            Vector3 end = stash.transform.position;

            var go = new GameObject("__StashFlight", typeof(Canvas));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9600;
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var rt = (RectTransform)iconGo.transform;
            rt.sizeDelta = size;
            rt.position = start;
            var img = iconGo.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;

            const float dur = 0.5f;
            for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
            {
                if (rt == null) break;
                float k = Mathf.SmoothStep(0f, 1f, t / dur);
                rt.position = Vector3.Lerp(start, end, k);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.45f, k); // shrink into the bucket
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        private static float GetScale(RectTransform rt)
        {
            var c = rt.GetComponentInParent<Canvas>();
            return c != null && c.scaleFactor > 0f ? c.scaleFactor : 1f;
        }

        private static Sprite ResolveSprite(MergeBoardController board, OverflowTileData tile)
        {
            if (board == null) return null;
            if (tile.kind == OverflowKind.Generator)
            {
                var so = board.FindGeneratorType(tile.family);
                return so != null ? so.SpriteForTier(tile.tier) : board.generatorSprite;
            }
            return board.SpriteForItem(tile.family, tile.tier);
        }

        private static string ResolveName(MergeBoardController board, OverflowTileData tile)
        {
            if (board != null)
            {
                if (tile.kind == OverflowKind.Generator)
                {
                    var so = board.FindGeneratorType(tile.family);
                    if (so != null && !string.IsNullOrEmpty(so.displayName)) return so.displayName;
                }
                else
                {
                    foreach (var d in board.ItemDefinitions)
                        if (d != null && d.family == tile.family && d.tier == tile.tier &&
                            !string.IsNullOrEmpty(d.displayName))
                            return d.displayName;
                }
            }
            return tile.family != null ? tile.family.Replace('_', ' ') : "Reward";
        }

        private static void AddLabel(RectTransform parent, string text, float size, Color color,
                                     Vector2 pos, Vector2 dims, bool display)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = dims;
            rt.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            AQTheme.StyleText(tmp, display: display);
        }
    }
}
