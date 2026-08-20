using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AQ.App.Audio;

namespace AQ.App
{
    /// <summary>
    /// Extended DialogueRunner with support for:
    /// - Character portraits and emotions
    /// - Voice acting with optional blocking
    /// - Conditional nodes (flag requirements)
    /// - Flag setting on node visit
    /// - Dialogue history with back button
    /// </summary>
    public class DialogueRunner : MonoBehaviour
    {
        [Header("Core")]
        public CaseGraph Graph;
        public DialogueController Panel;

        [Header("Audio")]
        [Tooltip("AudioSource for voice acting (optional - will be auto-created if null)")]
        public AudioSource voiceSource;
        [Tooltip("Background music AudioSource — will be ducked during voice playback")]
        public AudioSource musicSource;

        [Header("Debug")]
        public bool verboseLogging = false;

        public event System.Action DialogueEnded;

        // Global stage hooks: every dialogue entry point (lead proceed, evidence
        // board replay, dev tools) funnels through BootWithGraph/End, so these
        // fire for all of them without each caller needing wiring.
        public static event System.Action<CaseGraph> DialogueOpened;
        public static event System.Action DialogueClosed;

        // State
        private string _currentId;
        private Coroutine _musicFadeRoutine;
        private Coroutine _voiceRestoreRoutine;
        private DialogueTyper _bodyTyper;
        private DialogueTyper _speakerTyper;
        private bool _booted = false;
        private bool _waitingForAudio = false;

        // Filtered choices for the currently displayed node.
        // OnChoice indexes into this, not the raw choices array.
        private CaseGraph.Choice[] _filteredChoices = System.Array.Empty<CaseGraph.Choice>();

        // Long nodes paginate at sentence boundaries instead of overflowing the
        // strip — pagination retired 2026-08-14, one page per node always.
        private string[] _pages = System.Array.Empty<string>();
        private int _pageIx;

        // History for back button (capped at 50)
        private Stack<string> _history = new Stack<string>();
        public bool CanGoBack => _history.Count > 0;

        // Node-range overrides: play a sub-span of a graph without splitting the
        // asset (FTUE first-merge choreography plays N1–N3, then resumes at N4).
        private string _startOverrideId;
        private string _endAfterNodeId;

        // Replay boots (evidence-board scene replays) must not write flags: a
        // replayed story choice re-offered both branches and could set BOTH
        // mutually exclusive truth flags, corrupting the finale's branch gating.
        // A replay is a memory, not a decision. Per-boot; cleared in End().
        private bool _suppressFlagWrites;

        // Crash-RECOVERY boots (interrupted resolution dialogue replayed at next
        // launch) are the opposite: they exist to land the flags a kill skipped,
        // so writes stay ON — but a choice the player already made must not be
        // re-offered (setsFlag is NOT final-node-only in shipping content: the
        // Del Cruz truth flags sit on mid-graph branch nodes, and re-answering
        // would set BOTH mutually exclusive flags). Sticky choices: a choice
        // whose target node's setsFlag is already down is auto-followed.
        private bool _recoveryAutoChoice;

        void Start()
        {
            if (!_booted && Graph != null) InternalBoot(Graph);
        }

        /// <summary>
        /// Boot with an addressable-loaded graph.
        /// </summary>
        public void BootWithGraph(CaseGraph g)
        {
            _startOverrideId = null;
            _endAfterNodeId  = null;
            _suppressFlagWrites = false;
            _recoveryAutoChoice = false;
            BootCore(g);
        }

        /// <summary>
        /// Boot a graph as CRASH RECOVERY: flag writes stay ON (landing the flags
        /// the interrupted run never reached is the whole point) but choices whose
        /// outcome is already on disk are auto-followed instead of re-offered —
        /// the player's original decision stands.
        /// </summary>
        public void BootWithGraphForRecovery(CaseGraph g)
        {
            _startOverrideId = null;
            _endAfterNodeId  = null;
            _suppressFlagWrites = false;
            _recoveryAutoChoice = true;
            BootCore(g);
        }

        /// <summary>
        /// Boot a graph as a REPLAY: plays normally but never writes node/choice
        /// flags. Use for evidence-board scene replays — the player's original
        /// decisions must stand.
        /// </summary>
        public void BootWithGraphForReplay(CaseGraph g)
        {
            _startOverrideId = null;
            _endAfterNodeId  = null;
            _suppressFlagWrites = true;
            _recoveryAutoChoice = false;
            BootCore(g);
        }

        /// <summary>
        /// Boot a sub-span of a graph: start at <paramref name="startNodeId"/>
        /// (null/empty = the graph's own startId) and end the dialogue after the
        /// node with id <paramref name="endAfterNodeId"/> (null/empty = play to
        /// the graph's natural end). Overrides apply to this boot only.
        /// </summary>
        public void BootWithGraph(CaseGraph g, string startNodeId, string endAfterNodeId)
        {
            _startOverrideId = string.IsNullOrEmpty(startNodeId) ? null : startNodeId;
            _endAfterNodeId  = string.IsNullOrEmpty(endAfterNodeId) ? null : endAfterNodeId;
            _suppressFlagWrites = false;
            _recoveryAutoChoice = false;
            BootCore(g);
        }

        private void BootCore(CaseGraph g)
        {
            Graph = g;
            if (!_booted)
                InternalBoot(g);
            else
            {
                DialogueOpened?.Invoke(g);
                JumpTo(ResolveStartId(g));
            }
        }

        private string ResolveStartId(CaseGraph g)
        {
            if (!string.IsNullOrEmpty(_startOverrideId)) return _startOverrideId;
            if (!string.IsNullOrEmpty(g.startId)) return g.startId;
            return g.nodes != null && g.nodes.Length > 0 ? g.nodes[0].id : null;
        }

        /// <summary>
        /// Boot a single-line filler dialogue without needing a CaseGraph asset.
        /// Tap to dismiss — fires DialogueEnded normally.
        /// </summary>
        public void BootWithText(string speaker, string line)
        {
            var g = ScriptableObject.CreateInstance<CaseGraph>();
            g.startId = "filler";
            g.nodes   = new[] { new CaseGraph.Node { id = "filler", speaker = speaker, line = line } };
            BootWithGraph(g);
        }

        void SetupLayoutPanel()
        {
            Panel.EnsureRuntimeChoiceUI();
            Panel.ApplyStageLayout();

            // Height must track DialogueController's strip constant even for a
            // panel whose background was built under the old value.
            var existingBg = Panel.transform.Find("_Background");
            if (existingBg != null)
            {
                ((RectTransform)existingBg).anchorMax = new Vector2(1, 420f / 1920f);
                return;
            }

            var scaler = gameObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            var bg = new GameObject("_Background");
            bg.transform.SetParent(Panel.transform, false);
            var rt = bg.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 420f / 1920f);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            bg.AddComponent<CanvasRenderer>();
            var img = bg.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.95f);
            img.raycastTarget = true;
            bg.transform.SetAsFirstSibling();

            // No PointerClick EventTrigger here: the raw-input poll in Update()
            // already sees every tap. A click path on top of it made one physical
            // tap advance TWICE (down via the poll, up via the click), cutting the
            // next node's VO the moment it started.
        }

        void InternalBoot(CaseGraph g)
        {
            if (Panel == null) Panel = GetComponent<DialogueController>();
            if (g == null || Panel == null)
            {
                Debug.LogWarning("[DialogueRunner] Missing Graph or Panel.");
                return;
            }

            SetupLayoutPanel();

            // Setup voice source
            if (voiceSource == null)
            {
                voiceSource = GetComponent<AudioSource>();
                if (voiceSource == null)
                    voiceSource = gameObject.AddComponent<AudioSource>();
            }

            // Attach typers (or reuse existing)
            if (Panel.Body)
            {
                Panel.Body.gameObject.SetActive(true);
                _bodyTyper = Panel.Body.GetComponent<DialogueTyper>();
                if (_bodyTyper == null)
                    _bodyTyper = Panel.Body.gameObject.AddComponent<DialogueTyper>();
            }
            else { Debug.LogWarning("[DialogueRunner.InternalBoot] Panel.Body is null"); }

            if (Panel.Speaker)
            {
                Panel.Speaker.gameObject.SetActive(true);
                _speakerTyper = Panel.Speaker.GetComponent<DialogueTyper>();
                if (_speakerTyper == null)
                    _speakerTyper = Panel.Speaker.gameObject.AddComponent<DialogueTyper>();
            }

            // Configure typing speeds
            if (_bodyTyper != null) _bodyTyper.charsPerSecond = 45f;
            if (_speakerTyper != null) _speakerTyper.charsPerSecond = 60f;

            // Subscribe to panel events. Advance and choice taps are handled
            // EXCLUSIVELY by the raw-input poll in Update() — it sees every tap
            // (down-phase, whole screen) regardless of EventSystem state, so the
            // Button/onClick paths were duplicates: one tap fired both the poll
            // (pointer down) and onClick (pointer up), double-advancing and
            // stopping freshly started VO. Only Back keeps its Button (the poll
            // has no back hit-test).
            Panel.BackClicked += OnBack;

            // Start at first node
            _currentId = ResolveStartId(g);

            _booted = true;
            _history.Clear(); // Reset history on boot

            if (_tapRegion == null)
            {
                // Full-screen region, live only while the runner's GameObject is
                // active (End() deactivates it). Layer floored high: pre-router
                // behaviour was advance-on-ANY-tap, so only true topmost modals
                // (ConfirmPopup at 9999+) may outrank the open dialogue — never
                // incidental HUD graphics whose canvases sort above this one.
                var canvas = GetComponent<Canvas>();
                int layer = Mathf.Max(canvas != null ? canvas.sortingOrder : 0, 9000);
                _tapRegion = UI.TapRouter.Register("dialogue-advance", layer,
                    contains: _ => true,
                    onTap:    HandleRoutedTap,
                    enabled:  () => this != null && _booted && isActiveAndEnabled);
            }

            DialogueOpened?.Invoke(g);
            ShowNode(_currentId);
        }

        void OnDestroy()
        {
            UI.TapRouter.Unregister(_tapRegion);
            _tapRegion = null;
            if (Panel != null)
            {
                Panel.BackClicked -= OnBack;
            }
        }

        public string GetCurrentNodeId() => _currentId;

        /// <summary>
        /// Jump to a specific node by ID (useful for save/load or branching).
        /// </summary>
        public void JumpTo(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _history.Clear(); // Clear history on explicit jump
                ShowNode(id);
            }
        }

        // Registered on first boot; the dialogue owns the whole screen while its
        // GameObject is active. Routing through TapRouter (2026-08-18) keeps the
        // raw-input reliability (EventSystem clicks are flaky on this panel) but
        // adds the stacking/claiming guarantees: a popup above the dialogue now
        // blocks advance, and the tap that advances can never ALSO reach a
        // surface underneath (the evidence-board fallthrough family).
        private UI.TapRouter.Region _tapRegion;

        void HandleRoutedTap(Vector2 tapPos)
        {
            // Choices use the same raw-input path as advance (Button.onClick is
            // unreliable on this panel — see EnsureRuntimeChoiceUI).
            if (_filteredChoices != null && _filteredChoices.Length > 0)
            {
                int idx = Panel != null ? Panel.ChoiceIndexAtScreenPoint(tapPos) : -1;
                if (idx >= 0) OnChoice(idx);
                return;
            }

            OnAdvance();
        }

        void OnAdvance()
        {
            // If waiting for audio to complete, allow skip by stopping audio
            if (_waitingForAudio)
            {
                if (voiceSource && voiceSource.isPlaying)
                {
                    voiceSource.Stop();
                    _waitingForAudio = false;
                    if (_voiceRestoreRoutine != null) { StopCoroutine(_voiceRestoreRoutine); _voiceRestoreRoutine = null; }
                    RestoreMusic();
                }
                return;
            }

            // Skip typing if active
            if (_bodyTyper != null && _bodyTyper.IsTyping)
            {
                _bodyTyper.Skip();
                return;
            }

            if (_speakerTyper != null && _speakerTyper.IsTyping)
            {
                _speakerTyper.Skip();
                return;
            }

            // Get current node
            var n = Graph.Get(_currentId);
            if (n == null) return;

            // More pages of this node first
            if (_pageIx < _pages.Length - 1)
            {
                _pageIx++;
                ShowPage(n);
                return;
            }

            // If node has VISIBLE choices, don't auto-advance (wait for choice).
            // Checks the filtered set: if flags hid every choice, fall through to
            // linear progression instead of soft-locking.
            if (_filteredChoices != null && _filteredChoices.Length > 0) return;

            // Node-range boot: this span ends here even though the graph continues.
            if (_endAfterNodeId != null && n.id == _endAfterNodeId)
            {
                End();
                return;
            }

            // Linear progression
            if (!string.IsNullOrEmpty(n.nextId))
                ShowNode(n.nextId);
            else
                End();
        }

        void OnChoice(int idx)
        {
            // Skip typing if active
            if (_bodyTyper != null && _bodyTyper.IsTyping)
            {
                _bodyTyper.Skip();
                return;
            }

            // Index into the filtered set — idx is a button index, not a raw choices index
            if (idx < 0 || idx >= _filteredChoices.Length) return;

            var choice = _filteredChoices[idx];
            if (_endAfterNodeId != null && _currentId == _endAfterNodeId)
            {
                End();
                return;
            }
            if (!string.IsNullOrEmpty(choice.nextId))
                ShowNode(choice.nextId);
            else
                End();
        }

        /// <summary>
        /// NEW: Go back to previous node in history.
        /// </summary>
        void OnBack()
        {
            if (_history.Count == 0)
            {
                if (verboseLogging)
                    Debug.Log("[DialogueRunner] No history to go back to");
                return;
            }

            // Pop the previous node ID
            string previousId = _history.Pop();

            // Important: Don't add to history when going back
            string tempCurrent = _currentId;
            _currentId = previousId;

            var n = Graph.Get(_currentId);
            if (n == null)
            {
                // Restore current if previous node invalid
                _currentId = tempCurrent;
                _history.Push(previousId); // Restore history
                return;
            }

            // Stop any audio
            if (voiceSource && voiceSource.isPlaying)
                voiceSource.Stop();
            _waitingForAudio = false;

            // Display node WITHOUT adding to history
            DisplayNodeContent(n, addToHistory: false);

            if (verboseLogging)
                Debug.Log($"[DialogueRunner] Back to node: {_currentId}");
        }

        void ShowNode(string id)
        {
            // Save current to history before advancing (cap at 50 entries)
            if (!string.IsNullOrEmpty(_currentId) && _currentId != id && _history.Count < 50)
                _history.Push(_currentId);

            _currentId = id;
            var n = Graph.Get(id);

            if (n == null)
            {
                Debug.LogWarning($"[DialogueRunner] ShowNode: node '{id}' not found in graph — ending.");
                End();
                return;
            }

            // Check flag requirement
            if (!string.IsNullOrEmpty(n.requiresFlag))
            {
                if (!DialogueFlags.Has(n.requiresFlag))
                {
                    if (verboseLogging)
                        Debug.LogWarning($"[DialogueRunner] Node {id} requires flag: {n.requiresFlag}");

                    if (n.skipIfFlagMissing && !string.IsNullOrEmpty(n.nextId))
                    {
                        // Skip this node, move to next
                        ShowNode(n.nextId);
                        return;
                    }
                    else
                    {
                        // Block progression - don't show node
                        Debug.LogWarning($"[DialogueRunner] Blocked at node {id} - missing flag: {n.requiresFlag}");
                        return;
                    }
                }
            }

            // Set flag if specified (never during a replay boot — the original
            // playthrough's flags must stand)
            if (!_suppressFlagWrites && !string.IsNullOrEmpty(n.setsFlag))
            {
                DialogueFlags.Set(n.setsFlag);
            }

            // Recovery boot, sticky choices: if any choice leads directly to a
            // node whose setsFlag is already down, the player answered this in
            // the interrupted run — follow their answer instead of re-asking
            // (re-answering could set BOTH mutually exclusive branch flags).
            if (_recoveryAutoChoice && n.choices != null)
            {
                foreach (var c in n.choices)
                {
                    if (c == null || string.IsNullOrEmpty(c.nextId)) continue;
                    var target = Graph.Get(c.nextId);
                    if (target != null && !string.IsNullOrEmpty(target.setsFlag) &&
                        DialogueFlags.Has(target.setsFlag))
                    {
                        if (verboseLogging)
                            Debug.Log($"[DialogueRunner] Recovery: auto-following decided choice → {c.nextId}");
                        ShowNode(c.nextId);
                        return;
                    }
                }
            }

            // Display the node
            DisplayNodeContent(n, addToHistory: true);

            if (verboseLogging)
                Debug.Log($"[DialogueRunner] Showing node: {id}");
        }

        void DisplayNodeContent(CaseGraph.Node n, bool addToHistory)
        {
            // Stop previous audio if any
            if (voiceSource && voiceSource.isPlaying)
            {
                voiceSource.Stop();
            }
            if (_voiceRestoreRoutine != null) { StopCoroutine(_voiceRestoreRoutine); _voiceRestoreRoutine = null; }
            _waitingForAudio = false;

            // Filter choices once — ChoiceFilter is the single place for flag logic
            _filteredChoices = ChoiceFilter.GetAvailable(n.choices);

            _pages = BuildPages(n.line);
            _pageIx = 0;
            ShowPage(n);

            // Update back button visibility
            Panel.UpdateBackButton(CanGoBack);

            // Play voice clip if present (spans all pages of the node)
            if (n.voiceClip && voiceSource)
            {
                voiceSource.volume = AudioSettingsService.DialogueVolume;
                voiceSource.clip = n.voiceClip;
                voiceSource.Play();
                DuckMusic();

                if (n.waitForAudio)
                    _waitingForAudio = true;

                _voiceRestoreRoutine = StartCoroutine(WaitForAudioComplete(n.voiceClip.length));
            }
        }

        void ShowPage(CaseGraph.Node n)
        {
            bool lastPage = _pageIx >= _pages.Length - 1;
            string page = _pages.Length > 0 ? _pages[_pageIx] : n.line;

            // Choices only surface on the final page.
            Panel.BindNode(n, lastPage ? _filteredChoices : System.Array.Empty<CaseGraph.Choice>());
            if (Panel.Body) Panel.Body.text = string.Empty; // BindNode wrote the full line

            if (_speakerTyper != null)
                _speakerTyper.SetInstant(n.speaker);
            else if (Panel.Speaker)
                Panel.Speaker.text = n.speaker;

            if (_bodyTyper != null)
                _bodyTyper.StartTyping(page);
            else if (Panel.Body)
                Panel.Body.text = page;
        }

        /// <summary>
        /// Splits a long line into strip-sized pages at sentence boundaries.
        /// Short lines come back as a single page; sentences are never broken.
        /// </summary>
        // Pagination retired (Stephen-ruled 2026-08-14): the 420 strip shows
        // every Ep1 node whole (longest = 355 chars, capacity ~440) and the
        // on-screen text must match the playing VO clip. The page machinery
        // stays; a single page is always the last page, so choices and
        // waitForAudio behave unchanged.
        static string[] BuildPages(string line) => new[] { line ?? string.Empty };

        IEnumerator WaitForAudioComplete(float duration)
        {
            yield return new WaitForSeconds(duration);
            _waitingForAudio = false;
            _voiceRestoreRoutine = null;
            RestoreMusic();

            if (verboseLogging)
                Debug.Log("[DialogueRunner] Audio playback complete");
        }

        void DuckMusic()
        {
            if (musicSource == null) return;
            if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = StartCoroutine(FadeMusicVolume(AudioSettingsService.MusicVolume * 0.15f, 0.4f));
        }

        void RestoreMusic()
        {
            if (musicSource == null) return;
            if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = StartCoroutine(FadeMusicVolume(AudioSettingsService.MusicVolume, 0.5f));
        }

        IEnumerator FadeMusicVolume(float target, float duration)
        {
            float start = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            musicSource.volume = target;
        }

        void End()
        {
            // Overrides are per-boot; never leak into the next dialogue.
            _startOverrideId = null;
            _endAfterNodeId  = null;
            _suppressFlagWrites = false;
            _recoveryAutoChoice = false;

            if (_bodyTyper != null) _bodyTyper.StopTyping();
            if (_speakerTyper != null) _speakerTyper.StopTyping();

            if (voiceSource && voiceSource.isPlaying)
                voiceSource.Stop();

            if (_voiceRestoreRoutine != null) { StopCoroutine(_voiceRestoreRoutine); _voiceRestoreRoutine = null; }

            // SNAP the music back, no fade: Panel deactivates below and Panel
            // lives on this same GameObject, so a RestoreMusic() coroutine here
            // died after one frame and left the music stuck ducked at ~15%.
            if (_musicFadeRoutine != null) { StopCoroutine(_musicFadeRoutine); _musicFadeRoutine = null; }
            if (musicSource != null) musicSource.volume = AudioSettingsService.MusicVolume;

            if (verboseLogging)
                Debug.Log("[DialogueRunner] End of graph");

            if (Panel)
                Panel.gameObject.SetActive(false);

            DialogueClosed?.Invoke();
            DialogueEnded?.Invoke();
        }

        /// <summary>
        /// Clear dialogue history (useful for new conversations).
        /// </summary>
        public void ClearHistory()
        {
            _history.Clear();
            if (Panel) Panel.UpdateBackButton(false);
        }
    }
}