using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AQ.App;
using AQ.App.UI;
using AQ.App.UI.Board;

/// <summary>
/// Cinematic dialogue stage: while any dialogue runs, fades out the board and
/// HUD, deepens the background scrim and optionally swaps the scene backdrop
/// (CaseGraph.stageBackground) so the actor and story carry the screen.
/// Self-installing; CanvasGroups are added at runtime so the scene file is
/// never mutated. Lives in Assembly-CSharp to reach both AQ.App (dialogue,
/// scrim) and the board controller.
/// </summary>
public sealed class DialogueStageMB : MonoBehaviour
{
    const float FadeSeconds = 0.25f;
    const float StageScrimOpacity = 0.75f;

    static DialogueStageMB _instance;

    CanvasGroup _board;
    CanvasGroup _hud;
    BackgroundScrimMB _scrim;
    Image _background;

    float _baseScrimOpacity;
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
        _board = null;
        _hud = null;
        _scrim = null;
        _background = null;
        _open = false;
    }

    void OnDialogueOpened(CaseGraph graph)
    {
        if (!FindStage()) return;

        if (!_open)
        {
            _open = true;
            _baseScrimOpacity = _scrim != null ? _scrim.opacity : 0f;
            _baseBackground = _background != null ? _background.sprite : null;
            SetInteractive(false);
        }

        if (_background != null)
        {
            var stageSprite = graph != null ? graph.stageBackground : null;
            _background.sprite = stageSprite != null ? stageSprite : _baseBackground;
        }

        StartFade(0f, StageScrimOpacity);
    }

    void OnDialogueClosed()
    {
        if (!_open) return;
        _open = false;

        if (_background != null) _background.sprite = _baseBackground;
        SetInteractive(true);
        StartFade(1f, _baseScrimOpacity);
    }

    bool FindStage()
    {
        if (_scrim == null) _scrim = FindAnyObjectByType<BackgroundScrimMB>();
        var gameRoot = _scrim != null ? _scrim.transform.parent : null;

        if (_background == null && gameRoot != null)
        {
            var bg = gameRoot.Find("Background");
            if (bg != null) _background = bg.GetComponent<Image>();
        }

        if (_board == null)
        {
            var controller = FindAnyObjectByType<MergeBoardController>();
            var canvas = controller != null ? controller.GetComponentInParent<Canvas>() : null;
            if (canvas != null) _board = EnsureGroup(canvas.gameObject);
        }

        if (_hud == null && gameRoot != null)
        {
            var safe = gameRoot.Find("SafeAreaRoot");
            if (safe != null) _hud = EnsureGroup(safe.gameObject);
        }

        return _board != null || _hud != null || _scrim != null;
    }

    static CanvasGroup EnsureGroup(GameObject go)
    {
        var group = go.GetComponent<CanvasGroup>();
        return group != null ? group : go.AddComponent<CanvasGroup>();
    }

    void SetInteractive(bool value)
    {
        if (_board != null) { _board.blocksRaycasts = value; _board.interactable = value; }
        if (_hud != null) { _hud.blocksRaycasts = value; _hud.interactable = value; }
    }

    void StartFade(float groupTarget, float scrimTarget)
    {
        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(Fade(groupTarget, scrimTarget));
    }

    IEnumerator Fade(float groupTarget, float scrimTarget)
    {
        float boardStart = _board != null ? _board.alpha : groupTarget;
        float hudStart = _hud != null ? _hud.alpha : groupTarget;
        float scrimStart = _scrim != null ? _scrim.opacity : scrimTarget;

        float t = 0f;
        while (t < FadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / FadeSeconds);
            ApplyStage(
                Mathf.Lerp(boardStart, groupTarget, k),
                Mathf.Lerp(hudStart, groupTarget, k),
                Mathf.Lerp(scrimStart, scrimTarget, k));
            yield return null;
        }

        ApplyStage(groupTarget, groupTarget, scrimTarget);
        _fade = null;
    }

    void ApplyStage(float boardAlpha, float hudAlpha, float scrimOpacity)
    {
        if (_board != null) _board.alpha = boardAlpha;
        if (_hud != null) _hud.alpha = hudAlpha;
        if (_scrim != null) _scrim.SetOpacity(scrimOpacity);
    }
}
