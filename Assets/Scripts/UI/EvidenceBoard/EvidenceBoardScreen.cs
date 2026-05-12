using System.Collections.Generic;
using AQ.App;
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

        private const float BoardW        = 2160f;
        private const float BoardH        = 3840f;
        private const float DefaultScale  = 0.65f;
        private const float MinZoom       = 0.4f;
        private const float MaxZoom       = 2.5f;
        private const float PinPadding    = 220f;
        private const float MinSeparation = 320f;

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
            canvas.sortingOrder   = 10;

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

            // Board content — panned and zoomed
            var boardGo       = new GameObject("BoardContent", typeof(RectTransform));
            boardGo.transform.SetParent(_root.transform, false);
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

            // Square button — bottom-right corner
            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(btnRoot.transform, false);
            var btnRt              = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin        = new Vector2(1f, 0f);
            btnRt.anchorMax        = new Vector2(1f, 0f);
            btnRt.pivot            = new Vector2(1f, 0f);
            btnRt.sizeDelta        = new Vector2(140f, 140f);
            btnRt.anchoredPosition = new Vector2(-24f, 24f);

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = Color.white;
            var btnSprite = Resources.Load<Sprite>("App/UI/EvidenceBoard/evid_board_btn");
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
                if (lead == null) continue;
                if (DialogueFlags.Has("aq.lead." + lead.leadId + ".seen"))
                {
                    resolvedIds.Add(lead.leadId);
                    resolvedLeads.Add(lead);
                }
            }

            if (resolvedLeads.Count == 0) return;

            var placed    = new List<Vector2>();
            var cardRts   = new Dictionary<string, RectTransform>();
            var tackSprite = Resources.Load<Sprite>("App/UI/EvidenceBoard/push_pin");

            foreach (var lead in resolvedLeads)
            {
                // Index card
                var cardPos = GetDeterministicPosition(lead.leadId + "_card", placed, new Vector2(380f, 300f));
                placed.Add(cardPos);
                var cardRt = LeadCardPin.Create(_boardContent, lead, cardPos, OnLeadCardTapped, tackSprite);
                cardRts[lead.leadId] = cardRt;

                // Character photo
                var photoPos = GetDeterministicPosition(lead.leadId + "_photo", placed, new Vector2(260f, 310f));
                placed.Add(photoPos);
                CharacterPhotoPin.Create(_boardContent, lead, resolvedLeads, photoPos, OnReplayLeadDialogue, tackSprite);

                // Evidence item pins for each satisfied requirement
                if (lead.requirements != null)
                {
                    foreach (var req in lead.requirements)
                    {
                        if (!req.IsSatisfied) continue;
                        string label = req.Label ?? "Evidence";
                        var itemPos  = GetDeterministicPosition(lead.leadId + "_" + label, placed, new Vector2(220f, 220f));
                        placed.Add(itemPos);
                        EvidenceItemPin.Create(_boardContent, label, req.Icon, itemPos, tackSprite);
                    }
                }
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

        // ---- Deterministic placement ----

        private static Vector2 GetDeterministicPosition(string seed, List<Vector2> placed, Vector2 pinSize)
        {
            float halfW = BoardW / 2f - PinPadding - pinSize.x / 2f;
            float halfH = BoardH / 2f - PinPadding - pinSize.y / 2f;
            var   rng   = new System.Random(seed.GetHashCode());

            Vector2 candidate = Vector2.zero;
            for (int attempt = 0; attempt < 20; attempt++)
            {
                float x   = (float)(rng.NextDouble() * halfW * 2.0 - halfW);
                float y   = (float)(rng.NextDouble() * halfH * 2.0 - halfH);
                candidate = new Vector2(x, y);

                bool clear = true;
                foreach (var p in placed)
                {
                    if (Vector2.Distance(candidate, p) < MinSeparation) { clear = false; break; }
                }
                if (clear) return candidate;
            }

            return candidate;
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
