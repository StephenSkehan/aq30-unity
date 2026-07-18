using System.Collections.Generic;
using AQ.App;
using AQ.App.CaseFlow;
using AQ.App.Leads;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AQ.App.UI.EvidenceBoard
{
    public static class EvidenceBoardScreen
    {
        private static GameObject    _root;
        private static CanvasGroup   _cg;
        private static CanvasGroup   _btnCg;
        private static RectTransform _boardContent;
        private static LeadsRepository _repo;
        private static DialogueRunner  _dialogueRunner;
        private static bool            _isOpen;

        private const float BoardW         = 2160f;
        private const float BoardH         = 3840f;
        private const float DefaultScale   = 0.65f;
        private const float MinZoom        = 0.4f;
        private const float MaxZoom        = 2.5f;
        private const float CardColSpacing = 500f;
        private const float CardRowSpacing = 380f;
        private const float PhotoSpacing   = 300f;

        private static readonly List<CaseFlowDebugOverlayMB> _pausedOverlays = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            Build();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _repo           = null;
            _dialogueRunner = null;
            if (_isOpen) Close();
        }

        // ---- Build canvas once ----

        private static void Build()
        {
            if (_root != null) return;

            _root = new GameObject("__EvidenceBoard",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Object.DontDestroyOnLoad(_root);

            var canvas            = _root.GetComponent<Canvas>();
            canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
            // Above HUD widgets (OverflowBucketView 200, CaseResolutionScreen 200),
            // below dialogs/modals at 9999.
            canvas.sortingOrder   = 300;

            var scaler                 = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight  = 0.5f;

            _cg                  = _root.GetComponent<CanvasGroup>();
            _cg.alpha            = 0f;
            _cg.blocksRaycasts   = false;
            _cg.interactable     = false;

            // Cork board background (uses real photo; fallback to flat colour)
            var bg    = MakeStretch("BG", _root.transform);
            var bgImg = bg.gameObject.AddComponent<Image>();
            var corkSprite = Resources.Load<Sprite>("App/UI/EvidenceBoard/cork_board");
            if (corkSprite != null)
                bgImg.sprite = corkSprite;
            else
                bgImg.color = new Color(0.76f, 0.60f, 0.42f, 1f);

            // Viewport mask — keeps pins off the cork frame, the title, and the screen edges
            var viewport = MakeStretch("Viewport", _root.transform);
            viewport.offsetMin = new Vector2(70f, 140f);
            viewport.offsetMax = new Vector2(-70f, -135f);
            viewport.gameObject.AddComponent<RectMask2D>();

            // Board content — panned and zoomed
            var boardGo       = new GameObject("BoardContent", typeof(RectTransform));
            boardGo.transform.SetParent(viewport, false);
            _boardContent               = boardGo.GetComponent<RectTransform>();
            _boardContent.anchorMin     = new Vector2(0.5f, 0.5f);
            _boardContent.anchorMax     = new Vector2(0.5f, 0.5f);
            _boardContent.pivot         = new Vector2(0.5f, 0.5f);
            _boardContent.sizeDelta     = new Vector2(BoardW, BoardH);
            _boardContent.anchoredPosition = Vector2.zero;
            _boardContent.localScale    = Vector3.one * DefaultScale;

            var zp = boardGo.AddComponent<EvidenceBoardZoomPan>();
            zp.Init(_boardContent, MinZoom, MaxZoom, new Vector2(BoardW, BoardH));

            // Title label
            var titleGo  = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(_root.transform, false);
            var titleRt              = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin        = new Vector2(0f, 1f);
            titleRt.anchorMax        = new Vector2(1f, 1f);
            titleRt.pivot            = new Vector2(0.5f, 1f);
            titleRt.sizeDelta        = new Vector2(0f, 80f);
            titleRt.anchoredPosition = new Vector2(0f, -12f);
            var titleTmp             = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text            = "EVIDENCE BOARD";
            titleTmp.fontSize        = 36f;
            titleTmp.fontStyle       = FontStyles.Bold;
            titleTmp.color           = new Color(0.20f, 0.10f, 0.05f, 0.75f);
            titleTmp.alignment       = TextAlignmentOptions.Center;
            titleTmp.raycastTarget   = false;

            // Close button — top-right
            var closeBtnGo = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(_root.transform, false);
            var closeRt              = closeBtnGo.GetComponent<RectTransform>();
            closeRt.anchorMin        = new Vector2(1f, 1f);
            closeRt.anchorMax        = new Vector2(1f, 1f);
            closeRt.pivot            = new Vector2(1f, 1f);
            closeRt.sizeDelta        = new Vector2(110f, 110f);
            closeRt.anchoredPosition = new Vector2(-20f, -20f);
            closeBtnGo.GetComponent<Image>().color = new Color(0.25f, 0.12f, 0.08f, 0.90f);
            closeBtnGo.GetComponent<Button>().onClick.AddListener(Close);

            var xLbl       = new GameObject("X", typeof(RectTransform));
            xLbl.transform.SetParent(closeRt, false);
            var xRt        = xLbl.GetComponent<RectTransform>();
            xRt.anchorMin  = Vector2.zero;
            xRt.anchorMax  = Vector2.one;
            xRt.offsetMin  = xRt.offsetMax = Vector2.zero;
            var xTmp             = xLbl.AddComponent<TextMeshProUGUI>();
            xTmp.text            = "X";
            xTmp.fontSize        = 48f;
            xTmp.fontStyle       = FontStyles.Bold;
            xTmp.color           = new Color(0.95f, 0.90f, 0.85f, 1f);
            xTmp.alignment       = TextAlignmentOptions.Center;
            xTmp.raycastTarget   = false;

            BuildHudButton();
        }

        private static void BuildHudButton()
        {
            var btnRoot = new GameObject("__EvidBoardBtn",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            Object.DontDestroyOnLoad(btnRoot);

            var c           = btnRoot.GetComponent<Canvas>();
            c.renderMode    = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder  = 5;

            var sc                 = btnRoot.GetComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080f, 1920f);
            sc.matchWidthOrHeight  = 0.5f;

            _btnCg               = btnRoot.GetComponent<CanvasGroup>();
            _btnCg.alpha         = 1f;
            _btnCg.blocksRaycasts = true;
            _btnCg.interactable  = true;

            // Square button — bottom-right, level with the locker button on the
            // left (both centers at y≈264, over the background — Stephen-ruled
            // 2026-07-18; both get proper icon art when the kit sprites land).
            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(btnRoot.transform, false);
            var btnRt              = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin        = new Vector2(1f, 0f);
            btnRt.anchorMax        = new Vector2(1f, 0f);
            btnRt.pivot            = new Vector2(1f, 0f);
            btnRt.sizeDelta        = new Vector2(90f, 90f);
            btnRt.anchoredPosition = new Vector2(-24f, 219f);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = Color.white;
            // Proper icon art (2026-07-18); old cork thumbnail as fallback.
            var btnSprite = Resources.Load<Sprite>("App/UI/Icons/ui_btn_evidence_board");
            if (btnSprite == null)
                btnSprite = Resources.Load<Sprite>("App/UI/EvidenceBoard/evid_board_btn");
            if (btnSprite != null)
            {
                btnImg.sprite         = btnSprite;
                btnImg.preserveAspect = true;
            }
            else
            {
                btnImg.color = new Color(0.76f, 0.60f, 0.42f, 1f);
            }

            // Remove the default button transition so the icon always looks crisp
            var btn = btnGo.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(Open);
        }

        // ---- Public API ----

        public static void Open()
        {
            if (_root == null) Build();
            if (_isOpen) return;

            PopulateBoard();

            // IMGUI debug overlays draw above every canvas; pause them while open.
            _pausedOverlays.Clear();
            foreach (var overlay in Object.FindObjectsByType<CaseFlowDebugOverlayMB>(FindObjectsSortMode.None))
            {
                if (!overlay.show) continue;
                overlay.show = false;
                _pausedOverlays.Add(overlay);
            }

            _cg.alpha           = 1f;
            _cg.blocksRaycasts  = true;
            _cg.interactable    = true;
            _isOpen             = true;

            if (_btnCg != null)
            {
                _btnCg.alpha          = 0f;
                _btnCg.blocksRaycasts = false;
                _btnCg.interactable   = false;
            }
        }

        public static void Close()
        {
            if (_cg == null || !_isOpen) return;
            _cg.alpha          = 0f;
            _cg.blocksRaycasts = false;
            _cg.interactable   = false;
            _isOpen            = false;

            foreach (var overlay in _pausedOverlays)
                if (overlay != null) overlay.show = true;
            _pausedOverlays.Clear();

            if (_btnCg != null)
            {
                _btnCg.alpha          = 1f;
                _btnCg.blocksRaycasts = true;
                _btnCg.interactable   = true;
            }
        }

        // ---- Population ----

        private static void PopulateBoard()
        {
            ClearPins();

            if (_repo == null)
                _repo = Object.FindAnyObjectByType<LeadsRepository>();

            if (_repo == null || _repo.database == null)
            {
                Debug.LogWarning("[EvidenceBoardScreen] No LeadsRepository or database found.");
                return;
            }

            var resolvedIds   = new HashSet<string>();
            var resolvedLeads = new List<LeadData>();

            foreach (var lead in _repo.database.Leads)
            {
                if (lead == null || lead.boardPhase <= 0) continue; // repeatables/teasers stay off the board
                if (DialogueFlags.Has("aq.lead." + lead.leadId + ".seen"))
                {
                    resolvedIds.Add(lead.leadId);
                    resolvedLeads.Add(lead);
                }
            }

            if (resolvedLeads.Count == 0) return;

            var cardRts    = new Dictionary<string, RectTransform>();
            var tackSprite = Resources.Load<Sprite>("App/UI/EvidenceBoard/push_pin");

            float y = BoardH / 2f - 500f;

            // Cast row — one photo per unique portrait
            var seenPortraits = new HashSet<Sprite>();
            var cast = new List<LeadData>();
            foreach (var lead in resolvedLeads)
                if (lead.actorPortrait != null && seenPortraits.Add(lead.actorPortrait))
                    cast.Add(lead);

            for (int i = 0; i < cast.Count; i++)
            {
                float x = (i - (cast.Count - 1) / 2f) * PhotoSpacing;
                CharacterPhotoPin.Create(_boardContent, cast[i], resolvedLeads,
                    new Vector2(x, y), OnReplayLeadDialogue, tackSprite, CharacterNameFor(cast[i]));
            }
            if (cast.Count > 0) y -= 420f;

            // Lead cards clustered by phase, in database order within each phase
            var phases = new SortedDictionary<int, List<LeadData>>();
            foreach (var lead in resolvedLeads)
            {
                int p = Mathf.Max(1, lead.boardPhase);
                if (!phases.TryGetValue(p, out var list)) phases[p] = list = new List<LeadData>();
                list.Add(lead);
            }

            const int cols = 3;
            foreach (var kv in phases)
            {
                CreatePhaseLabel("PHASE " + kv.Key, new Vector2(0f, y));
                y -= 200f;

                var leads = kv.Value;
                int rows  = (leads.Count + cols - 1) / cols;
                for (int i = 0; i < leads.Count; i++)
                {
                    int row   = i / cols;
                    int inRow = Mathf.Min(cols, leads.Count - row * cols);
                    var rng   = new System.Random(leads[i].leadId.GetHashCode());
                    float jx  = (float)(rng.NextDouble() * 50.0 - 25.0);
                    float jy  = (float)(rng.NextDouble() * 36.0 - 18.0);
                    float x   = (i % cols - (inRow - 1) / 2f) * CardColSpacing + jx;

                    var cardRt = LeadCardPin.Create(_boardContent, leads[i],
                        new Vector2(x, y - row * CardRowSpacing + jy), OnLeadCardTapped, tackSprite);
                    cardRts[leads[i].leadId] = cardRt;
                }
                y -= rows * CardRowSpacing + 60f;
            }

            // String connections — draw after all pins so RTs are valid, behind everything
            foreach (var lead in resolvedLeads)
            {
                if (lead.boardConnections == null || lead.boardConnections.Length == 0) continue;
                if (!cardRts.TryGetValue(lead.leadId, out var fromRt)) continue;

                foreach (var toId in lead.boardConnections)
                {
                    if (!resolvedIds.Contains(toId)) continue;
                    if (!cardRts.TryGetValue(toId, out var toRt)) continue;
                    StringConnectionLine.Create(_boardContent, fromRt, toRt);
                }
            }
        }

        private static void ClearPins()
        {
            if (_boardContent == null) return;
            for (int i = _boardContent.childCount - 1; i >= 0; i--)
                Object.Destroy(_boardContent.GetChild(i).gameObject);
        }

        // ---- Cluster helpers ----

        private static string CharacterNameFor(LeadData lead)
        {
            // Portrait art is the only character identity on LeadData today;
            // map the known sprite families, fall back to the lead title.
            var n = lead.actorPortrait != null ? lead.actorPortrait.name.ToLowerInvariant() : string.Empty;
            if (n.Contains("ally"))   return "Ally Quinn";
            if (n.Contains("gerald")) return "Gerald Quinn";
            return lead.title;
        }

        private static void CreatePhaseLabel(string text, Vector2 pos)
        {
            var go = new GameObject("PhaseLabel_" + text, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(_boardContent, false);
            rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = new Vector2(560f, 110f);
            rt.anchoredPosition = pos;
            rt.localRotation    = Quaternion.Euler(0f, 0f, -1.5f);

            var img           = go.GetComponent<Image>();
            img.color         = new Color(0.94f, 0.90f, 0.78f, 1f);
            img.raycastTarget = false;

            var lblGo = new GameObject("Text", typeof(RectTransform));
            lblGo.transform.SetParent(rt, false);
            var lblRt       = (RectTransform)lblGo.transform;
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            var tmp           = lblGo.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = 52f;
            tmp.fontStyle     = FontStyles.Bold;
            tmp.color         = new Color(0.20f, 0.10f, 0.05f, 0.9f);
            tmp.alignment     = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        // ---- Dialogue replay ----

        private static void OnLeadCardTapped(LeadData lead) => OnReplayLeadDialogue(lead);

        private static void OnReplayLeadDialogue(LeadData lead)
        {
            if (lead == null || lead.resolutionDialogue == null) return;

            Close();

            if (_dialogueRunner == null)
                _dialogueRunner = Object.FindAnyObjectByType<DialogueRunner>();

            if (_dialogueRunner == null)
            {
                Debug.LogWarning("[EvidenceBoardScreen] No DialogueRunner in scene — cannot replay.");
                Open();
                return;
            }

            _dialogueRunner.DialogueEnded += OnDialogueEndedReopen;
            _dialogueRunner.BootWithGraph(lead.resolutionDialogue);
        }

        private static void OnDialogueEndedReopen()
        {
            if (_dialogueRunner != null)
                _dialogueRunner.DialogueEnded -= OnDialogueEndedReopen;
            Open();
        }

        // ---- UI helpers ----

        private static RectTransform MakeStretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt        = go.GetComponent<RectTransform>();
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = rt.offsetMax = Vector2.zero;
            return rt;
        }

    }
}
