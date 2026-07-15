using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.UI;
using AQ.App.UI.Board;

/// <summary>
/// Cinematic dialogue stage: while any dialogue runs, fades out the board, HUD
/// and stray overlay canvases (evidence-board button), deepens the background
/// scrim and optionally swaps the scene backdrop (CaseGraph.stageBackground)
/// so the actor and story carry the screen. Self-installing; CanvasGroups are
/// added at runtime so the scene file is never mutated. Each group's alpha and
/// interactivity are captured at open and restored exactly — other systems
/// (e.g. EvidenceBoardScreen hiding its own button) keep their state. Lives in
/// Assembly-CSharp to reach both AQ.App (dialogue, scrim) and the board
/// controller.
/// </summary>
public sealed class DialogueStageMB : MonoBehaviour
{
    const float FadeSeconds = 0.25f;
    // Deep scrim when dialogue plays over the default board backdrop; light
    // when the graph brings its own stageBackground (the backdrop IS the stage).
    const float StageScrimDark = 0.75f;
    const float StageScrimLit = 0.35f;

    struct StagedGroup
    {
        public CanvasGroup Group;
        public float BaseAlpha;
        public bool BaseInteractable;
        public bool BaseBlocksRaycasts;
    }

    static DialogueStageMB _instance;

    readonly List<StagedGroup> _groups = new List<StagedGroup>();
    BackgroundScrimMB _scrim;
    Image _background;

    float _baseScrimOpacity;
    float _stageScrim = StageScrimDark;
    Sprite _baseBackground;
    bool _open;
    Coroutine _fade;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        if (_instance != null) return;
        var go = new GameObject("DialogueStage");
        _instance = go.AddComponent<DialogueStageMB>();
        DontDestroyOnLoad(go);
    }

    void OnEnable()
    {
        DialogueRunner.DialogueOpened += OnDialogueOpened;
        DialogueRunner.DialogueClosed += OnDialogueClosed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        DialogueRunner.DialogueOpened -= OnDialogueOpened;
        DialogueRunner.DialogueClosed -= OnDialogueClosed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Scene reload destroys the staged objects — drop all cached state.
        if (_fade != null) StopCoroutine(_fade);
        _fade = null;
        _groups.Clear();
        _scrim = null;
        _background = null;
        _open = false;
    }

    void OnDialogueOpened(CaseGraph graph)
    {
        if (!_open)
        {
            if (!FindStage()) return;
            _open = true;
            _baseScrimOpacity = _scrim != null ? _scrim.opacity : 0f;
            _baseBackground = _background != null ? _background.sprite : null;
            for (int i = 0; i < _groups.Count; i++)
            {
                var g = _groups[i].Group;
                if (g == null) continue;
                g.interactable = false;
                g.blocksRaycasts = false;
            }
        }

        var stageSprite = graph != null ? graph.stageBackground : null;
        if (_background != null)
            _background.sprite = stageSprite != null ? stageSprite : _baseBackground;
        _stageScrim = stageSprite != null ? StageScrimLit : StageScrimDark;

        StartFade(toStage: true);
    }

    void OnDialogueClosed()
    {
        if (!_open) return;
        _open = false;

        if (_background != null) _background.sprite = _baseBackground;
        for (int i = 0; i < _groups.Count; i++)
        {
            var staged = _groups[i];
            if (staged.Group == null) continue;
            staged.Group.interactable = staged.BaseInteractable;
            staged.Group.blocksRaycasts = staged.BaseBlocksRaycasts;
        }

        StartFade(toStage: false);
    }

    bool FindStage()
    {
        _groups.Clear();

        if (_scrim == null) _scrim = FindAnyObjectByType<BackgroundScrimMB>();
        var gameRoot = _scrim != null ? _scrim.transform.parent : null;

        if (_background == null && gameRoot != null)
        {
            var bg = gameRoot.Find("Background");
            if (bg != null) _background = bg.GetComponent<Image>();
        }

        var controller = FindAnyObjectByType<MergeBoardController>();
        var boardCanvas = controller != null ? controller.GetComponentInParent<Canvas>() : null;
        if (boardCanvas != null) Stage(EnsureGroup(boardCanvas.gameObject));

        if (gameRoot != null)
        {
            var safe = gameRoot.Find("SafeAreaRoot");
            if (safe != null) Stage(EnsureGroup(safe.gameObject));
        }

        // These auto-install onto their own overlay canvases, so the board/HUD
        // groups never cover them.
        var evidBtn = GameObject.Find("__EvidBoardBtn");
        if (evidBtn != null) Stage(evidBtn.GetComponent<CanvasGroup>());
        var overflow = GameObject.Find("OverflowCanvas");
        if (overflow != null) Stage(EnsureGroup(overflow));

        return _groups.Count > 0 || _scrim != null;
    }

    void Stage(CanvasGroup group)
    {
        if (group == null) return;
        _groups.Add(new StagedGroup
        {
            Group = group,
            BaseAlpha = group.alpha,
            BaseInteractable = group.interactable,
            BaseBlocksRaycasts = group.blocksRaycasts
        });
    }

    static CanvasGroup EnsureGroup(GameObject go)
    {
        var group = go.GetComponent<CanvasGroup>();
        return group != null ? group : go.AddComponent<CanvasGroup>();
    }

    void StartFade(bool toStage)
    {
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(Fade(toStage));
    }

    IEnumerator Fade(bool toStage)
    {
        int count = _groups.Count;
        var starts = new float[count];
        for (int i = 0; i < count; i++)
            starts[i] = _groups[i].Group != null ? _groups[i].Group.alpha : 0f;
        float scrimStart = _scrim != null ? _scrim.opacity : 0f;
        float scrimTarget = toStage ? _stageScrim : _baseScrimOpacity;

        float t = 0f;
        while (t < FadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / FadeSeconds);
            ApplyStage(starts, toStage, k, scrimStart, scrimTarget);
            yield return null;
        }

        ApplyStage(starts, toStage, 1f, scrimStart, scrimTarget);
        _fade = null;
    }

    void ApplyStage(float[] starts, bool toStage, float k, float scrimStart, float scrimTarget)
    {
        for (int i = 0; i < _groups.Count && i < starts.Length; i++)
        {
            var staged = _groups[i];
            if (staged.Group == null) continue;
            float target = toStage ? 0f : staged.BaseAlpha;
            staged.Group.alpha = Mathf.Lerp(starts[i], target, k);
        }
        if (_scrim != null) _scrim.SetOpacity(Mathf.Lerp(scrimStart, scrimTarget, k));
    }
}
