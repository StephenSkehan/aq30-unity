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
        // strip — one tap per page, choices/advance only on the last page.
        private const int PageCharLimit = 240;
        private string[] _pages = System.Array.Empty<string>();
        private int _pageIx;

        // History for back button (capped at 50)
        private Stack<string> _history = new Stack<string>();
        public bool CanGoBack => _history.Count > 0;

        // Node-range overrides: play a sub-span of a graph without splitting the
        // asset (FTUE first-merge choreography plays N1–N3, then resumes at N4).
        private string _startOverrideId;
        private string _endAfterNodeId;

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

            if (Panel.transform.Find("_Background") != null) return;

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
            rt.anchorMax = new Vector2(1, 300f / 1920f);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            bg.AddComponent<CanvasRenderer>();
            var img = bg.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.95f);
            img.raycastTarget = true;
            bg.transform.SetAsFirstSibling();

            var trigger = bg.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick
            };
            entry.callback.AddListener((_) => OnAdvance());
            trigger.triggers.Add(entry);
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

            // Subscribe to panel events
            Panel.AdvanceClicked += OnAdvance;
            Panel.ChoiceClicked += OnChoice;
            Panel.BackClicked += OnBack;

            // Start at first node
            _currentId = ResolveStartId(g);

            _booted = true;
            _history.Clear(); // Reset history on boot

            DialogueOpened?.Invoke(g);
            ShowNode(_currentId);
        }

        void OnDestroy()
        {
            if (Panel != null)
            {
                Panel.AdvanceClicked -= OnAdvance;
                Panel.ChoiceClicked -= OnChoice;
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

        void Update()
        {
            if (!_booted) return;

            bool tapped = Input.GetMouseButtonDown(0);
            Vector2 tapPos = Input.mousePosition;
            if (!tapped && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tapped = true;
                tapPos = Input.GetTouch(0).position;
            }
            if (!tapped) return;

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

            // Set flag if specified
            if (!string.IsNullOrEmpty(n.setsFlag))
            {
                DialogueFlags.Set(n.setsFlag);
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
        static string[] BuildPages(string line)
        {
            if (string.IsNullOrEmpty(line) || line.Length <= PageCharLimit)
                return new[] { line ?? string.Empty };

            var sentences = System.Text.RegularExpressions.Regex.Split(line, @"(?<=[.!?…])\s+");
            var pages = new List<string>();
            var current = new System.Text.StringBuilder();
            foreach (var s in sentences)
            {
                if (current.Length > 0 && current.Length + s.Length + 1 > PageCharLimit)
                {
                    pages.Add(current.ToString());
                    current.Clear();
                }
                if (current.Length > 0) current.Append(' ');
                current.Append(s);
            }
            if (current.Length > 0) pages.Add(current.ToString());
            return pages.ToArray();
        }

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

            if (_bodyTyper != null) _bodyTyper.StopTyping();
            if (_speakerTyper != null) _speakerTyper.StopTyping();

            if (voiceSource && voiceSource.isPlaying)
                voiceSource.Stop();

            if (_voiceRestoreRoutine != null) { StopCoroutine(_voiceRestoreRoutine); _voiceRestoreRoutine = null; }
            RestoreMusic();

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