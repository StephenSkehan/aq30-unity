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

    // RETIRED (Stephen-ruled 2026-08-22): the guided case loop's proceed step
    // (green-card pulse + Gerald banner) replaced this arrow, and with the
    // card's PROCEED button gone its anchor floated mid-board as an orphan
    // yellow triangle. EnsureInstalled stays as a no-op for its callers.
    public static void EnsureInstalled() { }

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

        if (_label == null)
        {
            _label = gameObject.AddComponent<TextMeshProUGUI>();
            _label.text = "▼";
            _label.fontSize = 52f;
            _label.color = new Color(1f, 0.85f, 0.1f, 1f);
            _label.alignment = TextAlignmentOptions.Center;
            _label.raycastTarget = false;
        }
        _label.enabled = true;

        _target  = btnRect;
        _showing = true;
    }

    float _nextScanAt;

    void Update()
    {
        if (!_showing)
        {
            // CardBecameReady only fires on the false→true transition — a card
            // that went Ready while the hint was suppressed (FTUE choreography)
            // or before install has already spent its event. Scan for one.
            if (Time.unscaledTime >= _nextScanAt)
            {
                _nextScanAt = Time.unscaledTime + 0.5f;
                TryAttachToReadyCard();
            }
            return;
        }

        if (_target == null || _label == null)
        {
            // Card view rebuilt/destroyed under us — hide and rescan.
            _showing = false;
            if (_label != null) _label.enabled = false;
            return;
        }

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

    void TryAttachToReadyCard()
    {
        var cards = Object.FindObjectsByType<LeadCardView>(FindObjectsSortMode.None);
        foreach (var card in cards)
        {
            if (card == null || !card.IsReadyNow) continue;
            OnCardReady(card);
            if (_showing) return;
        }
    }

    void OnLeadActivated(LeadData _)
    {
        // Burn the one-shot flag only if the arrow actually taught. Activation can
        // arrive while the hint never displayed (card went Ready during the FTUE
        // suppression window) — burning then killed the tutorial for the whole
        // save without it ever appearing once. If we weren't showing, stay armed
        // for the next Ready card.
        if (!_showing) return;
        NarrativeFlags.Set(FtueFlag);
        Destroy(gameObject);
    }
}
