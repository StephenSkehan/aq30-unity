using AQ.App.Economy;
using AQ.App.UI.Board;
using AQ.SharedKernel.Economy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameResetMB : MonoBehaviour, IPointerClickHandler
{
    private const float TapWindow   = 0.8f;
    private const int  TapsRequired = 3;

    private int   _tapCount;
    private float _lastTapTime;
    private bool  _dialogOpen;
    private GameObject _dialogRoot;

    // ── Triple-tap detection ──────────────────────────────────────────────

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        if (now - _lastTapTime > TapWindow) _tapCount = 0;
        _tapCount++;
        _lastTapTime = now;

        if (_tapCount >= TapsRequired)
        {
            _tapCount = 0;
            ShowDialog();
        }
    }

    // ── Dialog ────────────────────────────────────────────────────────────

    private void ShowDialog()
    {
        if (_dialogOpen) return;
        _dialogOpen = true;

        _dialogRoot = new GameObject("__ResetDialog", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(_dialogRoot);

        var canvas = _dialogRoot.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var scaler = _dialogRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight  = 0.5f;

        // Dim
        var dim = MakeRect("Dim", _dialogRoot.transform);
        var dimImg = dim.gameObject.AddComponent<Image>();
        dimImg.color  = new Color(0f, 0f, 0f, 0.75f);
        dim.anchorMin = Vector2.zero;
        dim.anchorMax = Vector2.one;
        dim.offsetMin = dim.offsetMax = Vector2.zero;

        // Panel
        var panel = MakeRect("Panel", _dialogRoot.transform);
        panel.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
        panel.anchorMin        = new Vector2(0.5f, 0.5f);
        panel.anchorMax        = new Vector2(0.5f, 0.5f);
        panel.pivot            = new Vector2(0.5f, 0.5f);
        panel.sizeDelta        = new Vector2(700f, 380f);
        panel.anchoredPosition = Vector2.zero;

        // Title
        AddTMP("Reset Game?", panel, new Vector2(0f, 0.55f), new Vector2(1f, 1f), 52f, Color.white,
               new Vector2(40f, -20f), new Vector2(-40f, 0f));

        // Body
        AddTMP("All progress will be lost.", panel, new Vector2(0f, 0.35f), new Vector2(1f, 0.58f), 32f,
               new Color(0.7f, 0.7f, 0.7f), new Vector2(40f, 0f), new Vector2(-40f, 0f));

        // Buttons
        var cancel = MakeButton("Cancel", panel, new Color(0.3f, 0.3f, 0.3f), new Vector2(-190f, -120f));
        cancel.onClick.AddListener(CloseDialog);

        var reset = MakeButton("Reset", panel, new Color(0.8f, 0.15f, 0.15f), new Vector2(190f, -120f));
        reset.onClick.AddListener(DoReset);
    }

    private void CloseDialog()
    {
        if (_dialogRoot != null) Destroy(_dialogRoot);
        _dialogOpen = false;
    }

    private void DoReset()
    {
        // Zero the in-memory wallet — it survives LoadScene as DontDestroyOnLoad
        // so FTUE would otherwise grant on top of the existing balance.
        var wallet = WalletLocator.Instance;
        if (wallet != null)
        {
            wallet.TrySpend(Currency.Soft,    wallet.Get(Currency.Soft),    "reset");
            wallet.TrySpend(Currency.Premium, wallet.Get(Currency.Premium), "reset");
            wallet.TrySpend(Currency.Energy,  wallet.Get(Currency.Energy),  "reset");
        }

        JsonSaveService.Clear();
        BoardSaveSystem.ClearSave();
        PlayerPrefs.DeleteAll();
        Destroy(_dialogRoot);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ── UI helpers ────────────────────────────────────────────────────────

    private static RectTransform MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void AddTMP(string text, RectTransform parent,
                                Vector2 anchorMin, Vector2 anchorMax,
                                float fontSize, Color color,
                                Vector2 offsetMin, Vector2 offsetMax)
    {
        var rt        = MakeRect(text, parent);
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        var tmp            = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text           = text;
        tmp.fontSize       = fontSize;
        tmp.color          = color;
        tmp.alignment      = TextAlignmentOptions.Center;
        tmp.raycastTarget  = false;
    }

    private static Button MakeButton(string label, RectTransform parent, Color color, Vector2 position)
    {
        var go = new GameObject(label + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt             = go.GetComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0f);
        rt.anchorMax       = new Vector2(0.5f, 0f);
        rt.pivot           = new Vector2(0.5f, 0.5f);
        rt.sizeDelta       = new Vector2(280f, 90f);
        rt.anchoredPosition = position;
        go.GetComponent<Image>().color = color;

        var lbl       = MakeRect("Label", rt);
        lbl.anchorMin = Vector2.zero;
        lbl.anchorMax = Vector2.one;
        lbl.offsetMin = lbl.offsetMax = Vector2.zero;
        var tmp            = lbl.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text           = label;
        tmp.fontSize       = 36f;
        tmp.color          = Color.white;
        tmp.alignment      = TextAlignmentOptions.Center;
        tmp.raycastTarget  = false;

        return go.GetComponent<Button>();
    }
}
