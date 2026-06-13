using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using AQ.App;
using AQ.App.Leads;

/// <summary>
/// One-time FTUE hint: pulsing gold arrow above the Proceed button on the first
/// fulfilled lead card. Self-installs via RuntimeInitialize. Dismisses when any
/// lead is activated and sets a NarrativeFlag so it never shows again.
/// </summary>
public class ProceedHintMB : MonoBehaviour
{
    const string FtueFlag = "aq.ftue.tap_proceed.seen";

    TextMeshProUGUI _label;
    RectTransform   _rt;
    RectTransform   _target;
    bool            _showing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        Install();
        SceneManager.sceneLoaded += (_, __) => Install();
    }

    static void Install()
    {
        if (NarrativeFlags.Has(FtueFlag)) return;
        var go = new GameObject("ProceedHint");
        go.AddComponent<RectTransform>();
        go.AddComponent<ProceedHintMB>();
    }

    void Start()
    {
        if (NarrativeFlags.Has(FtueFlag)) { Destroy(gameObject); return; }
        LeadCardView.CardBecameReady    += OnCardReady;
        LeadsRuntimeBus.OnLeadActivated += OnLeadActivated;
    }

    void OnDestroy()
    {
        LeadCardView.CardBecameReady    -= OnCardReady;
        LeadsRuntimeBus.OnLeadActivated -= OnLeadActivated;
    }

    void OnCardReady(LeadCardView card)
    {
        if (_showing) return; // already tracking a card

        var btnRect = card.ProceedButtonRect;
        if (btnRect == null) return;

        var canvas = btnRect.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _rt = GetComponent<RectTransform>();
        transform.SetParent(canvas.transform, false);
        _rt.sizeDelta = new Vector2(80f, 60f);
        transform.SetAsLastSibling();

        _label = gameObject.AddComponent<TextMeshProUGUI>();
        _label.text = "▼";
        _label.fontSize = 52f;
        _label.color = new Color(1f, 0.85f, 0.1f, 1f);
        _label.alignment = TextAlignmentOptions.Center;
        _label.raycastTarget = false;

        _target  = btnRect;
        _showing = true;
    }

    void Update()
    {
        if (!_showing || _label == null || _target == null) return;

        // Hover above the Proceed button in world space
        var pos = _target.position;
        pos.y += _target.rect.height * _target.lossyScale.y * 0.8f;
        transform.position = pos;

        // Pulse scale + alpha
        float t = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 1.5f) + 1f) * 0.5f;
        transform.localScale = Vector3.one * (1f + t * 0.25f);
        var c = _label.color;
        c.a = Mathf.Lerp(0.4f, 1f, t);
        _label.color = c;
    }

    void OnLeadActivated(LeadData _)
    {
        NarrativeFlags.Set(FtueFlag);
        Destroy(gameObject);
    }
}
