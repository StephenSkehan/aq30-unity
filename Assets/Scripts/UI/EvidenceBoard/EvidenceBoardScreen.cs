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
        private static RectTransform _closeRt;
        private static EvidenceBoardZoomPan _zoomPan;
        private static LeadsRepository _repo;
        private static DialogueRunner  _dialogueRunner;
        private static bool            _isOpen;

        // Content-driven layout state (rebuilt per populate)
        private static Vector2 _contentSize;
        private static readonly List<RectTransform> _placed = new();
        private static readonly List<(RectTransform rt, System.Action tap)> _tappables = new();

        private const float BoardW         = 2160f;
        private const float BoardH         = 3840f;
        private const float DefaultScale   = 0.65f;
        private const float MinZoom        = 0.4f;
        private const float MaxZoom        = 2.5f;
        private const float CardColSpacing = 500f;
        private const float CardRowSpacing = 380f;
        private const float PhotoSpacing   = 300f;
        // Visible viewport in reference px (1080x1920 minus the frame insets)
        private const float ViewW          = 940f;
        private const float ViewH          = 1640f;

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
            zp.Tapped += OnBoardTapped;
            _zoomPan = zp;

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
            AQTheme.StyleButton(closeBtnGo.GetComponent<Image>(), AQTheme.Steel);
            closeBtnGo.GetComponent<Button>().onClick.AddListener(Close);
            _closeRt = closeRt; // raw-input poll fallback (Close is idempotent)
            AQTheme.AddDrawnX(closeRt, AQTheme.Paper, 34f, 6f);

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
            btnRt.sizeDelta        = new Vector2(142f, 142f); // grid-square parity, see LockerScreen
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

            // Fit-to-content: open with every pin visible, centred on the cork.
            float fit = DefaultScale;
            if (_contentSize.x > 1f && _contentSize.y > 1f)
                fit = Mathf.Min(ViewW / _contentSize.x, ViewH / _contentSize.y);
            _boardContent.localScale       = Vector3.one * Mathf.Clamp(fit, MinZoom, 1f);
            _boardContent.anchoredPosition = Vector2.zero;

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
                // Only the scenes this character actually appears in (Stephen-ruled
                // 2026-08-11) — portrait match or a speaking part in the dialogue.
                var involved = LeadsInvolving(cast[i], resolvedLeads);
                var photoRt = CharacterPhotoPin.Create(_boardContent, cast[i], involved,
                    new Vector2(x, y), OnReplayLeadDialogue, tackSprite, CharacterNameFor(cast[i]));
                _placed.Add(photoRt);
                var photoPin = photoRt.GetComponent<CharacterPhotoPin>();
                _tappables.Add((photoRt, photoPin.Tap));
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
                var label = CreatePhaseLabel("PHASE " + kv.Key, new Vector2(0f, y));
                _placed.Add(label);
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
                    // Brick stagger on odd rows keeps the grid from reading as a spreadsheet.
                    float brick = (row % 2 == 1) ? CardColSpacing * 0.25f : 0f;
                    float x   = (i % cols - (inRow - 1) / 2f) * CardColSpacing + jx + brick;

                    var cardRt = LeadCardPin.Create(_boardContent, leads[i],
                        new Vector2(x, y - row * CardRowSpacing + jy), OnLeadCardTapped, tackSprite);
                    cardRts[leads[i].leadId] = cardRt;
                    _placed.Add(cardRt);
                    var cardPin = cardRt.GetComponent<LeadCardPin>();
                    _tappables.Add((cardRt, cardPin.Tap));
                }
                y -= rows * CardRowSpacing + 60f;
            }

            // Centre the composition: shift everything so the content centroid sits
            // at board origin, then size the board to the content (plus breathing
            // room) so pan clamps and the open-fit zoom track what's actually pinned.
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (var rt in _placed)
            {
                var half = rt.sizeDelta * 0.5f + new Vector2(40f, 40f); // tilt slop
                min = Vector2.Min(min, rt.anchoredPosition - half);
                max = Vector2.Max(max, rt.anchoredPosition + half);
            }
            var centreOff = (min + max) * 0.5f;
            foreach (var rt in _placed) rt.anchoredPosition -= centreOff;
            _contentSize = max - min;

            var boardSize = new Vector2(
                Mathf.Max(_contentSize.x + 500f, 1080f),
                Mathf.Max(_contentSize.y + 500f, 1600f));
            _boardContent.sizeDelta = boardSize;
            _zoomPan.SetBoardSize(boardSize);

            // String connections — after the recentre shift so endpoints are final.
            // Threads run tack-to-tack like a real board, behind the cards.
            foreach (var lead in resolvedLeads)
            {
                if (lead.boardConnections == null || lead.boardConnections.Length == 0) continue;
                if (!cardRts.TryGetValue(lead.leadId, out var fromRt)) continue;

                foreach (var toId in lead.boardConnections)
                {
                    if (!resolvedIds.Contains(toId)) continue;
                    if (!cardRts.TryGetValue(toId, out var toRt)) continue;
                    StringConnectionLine.Create(_boardContent, TackPoint(fromRt), TackPoint(toRt));
                }
            }
        }

        /// <summary>Board-space position of a card's thumbtack (local (0,128) rotated by its tilt).</summary>
        private static Vector2 TackPoint(RectTransform card)
        {
            float a = card.localEulerAngles.z * Mathf.Deg2Rad;
            const float tackY = 128f;
            return card.anchoredPosition + new Vector2(-Mathf.Sin(a) * tackY, Mathf.Cos(a) * tackY);
        }

        private static void ClearPins()
        {
            _placed.Clear();
            _tappables.Clear();
            _contentSize = Vector2.zero;
            if (_boardContent == null) return;
            for (int i = _boardContent.childCount - 1; i >= 0; i--)
                Object.Destroy(_boardContent.GetChild(i).gameObject);
        }

        // ---- Raw-input tap routing (board canvas is boot-created; GR unreliable) ----

        private static void OnBoardTapped(Vector2 screenPos)
        {
            if (!_isOpen) return;
            if (CharacterProfileModal.IsOpen) return; // modal owns input while up

            if (_closeRt != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_closeRt, screenPos, null))
            {
                Close();
                return;
            }

            // Topmost pin wins (later siblings render on top).
            for (int i = _tappables.Count - 1; i >= 0; i--)
            {
                var (rt, tap) = _tappables[i];
                if (rt == null || !rt.gameObject.activeInHierarchy) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null))
                {
                    tap();
                    return;
                }
            }
        }

        // ---- Cluster helpers ----

        private static string CharacterNameFor(LeadData lead)
        {
            // Portrait art is the only character identity on LeadData today.
            // Sprite naming: char_<token>_<emotion>_fNN (e.g. char_del_neutral_f01).
            var token = PortraitToken(lead);
            switch (token)
            {
                case "ally":   return "Ally Quinn";
                case "gerald": return "Gerald Quinn";
                case "del":    return "Del Cruz";
                case "mo":     return "Mo Callahan";
                case "dot":    return "Dot Ellis";
                case "vera":   return "Vera";
                case "benji":  return "Benji Park";
            }
            if (!string.IsNullOrEmpty(token))
                return char.ToUpperInvariant(token[0]) + token.Substring(1);
            return lead.title; // last resort
        }

        private static string PortraitToken(LeadData lead)
        {
            var n = lead.actorPortrait != null ? lead.actorPortrait.name.ToLowerInvariant() : string.Empty;
            if (!n.StartsWith("char_")) return string.Empty;
            int end = n.IndexOf('_', 5);
            return end > 5 ? n.Substring(5, end - 5) : n.Substring(5);
        }

        /// <summary>Leads this character appears in: same portrait, or a speaking
        /// part in the resolution dialogue (speaker contains the portrait token,
        /// e.g. "Del" / "Dot Ellis (voicemail)").</summary>
        private static List<LeadData> LeadsInvolving(LeadData castLead, List<LeadData> resolved)
        {
            var token = PortraitToken(castLead);
            var result = new List<LeadData>();
            foreach (var lead in resolved)
            {
                bool involved = lead.actorPortrait == castLead.actorPortrait;
                if (!involved && !string.IsNullOrEmpty(token) && lead.resolutionDialogue != null &&
                    lead.resolutionDialogue.nodes != null)
                {
                    foreach (var node in lead.resolutionDialogue.nodes)
                        if (node != null && !string.IsNullOrEmpty(node.speaker) &&
                            node.speaker.ToLowerInvariant().Contains(token))
                        { involved = true; break; }
                }
                if (involved) result.Add(lead);
            }
            if (result.Count == 0) result.Add(castLead);
            return result;
        }

        private static RectTransform CreatePhaseLabel(string text, Vector2 pos)
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

            return rt;
        }

        // ---- Dialogue replay ----

        private static void OnLeadCardTapped(LeadData lead) => OnReplayLeadDialogue(lead);

        private static void OnReplayLeadDialogue(LeadData lead)
        {
            if (lead == null || lead.resolutionDialogue == null) return;

            Close();

            // Include inactive: the dialogue panel sleeps between dialogues, and
            // the default finder skips it — the silent replay no-op (2026-08-11).
            if (_dialogueRunner == null)
                _dialogueRunner = Object.FindAnyObjectByType<DialogueRunner>(FindObjectsInactive.Include);

            if (_dialogueRunner == null)
            {
                Debug.LogWarning("[EvidenceBoardScreen] No DialogueRunner in scene — cannot replay.");
                Open();
                return;
            }
            if (!_dialogueRunner.gameObject.activeSelf)
                _dialogueRunner.gameObject.SetActive(true);

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
