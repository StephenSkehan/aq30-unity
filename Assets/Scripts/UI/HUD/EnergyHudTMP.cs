// Assets/Scripts/UI/HUD/EnergyHudTMP.cs
using System;
using TMPro;
using UnityEngine;
using AQ.App.Config;
using AQ.App.Services;

namespace AQ.App.UI.HUD
{
    /// <summary>
    /// Simple TMP HUD: "current/cap  (+1 in m:ss)".
    /// Attach to a TextMeshProUGUI object.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class EnergyHudTMP : MonoBehaviour
    {
        [Tooltip("Optional explicit label; defaults to this object's TMP component.")]
        public TextMeshProUGUI label;

        [Tooltip("Update period in seconds for the text (unscaled time).")]
        [Min(0.03f)]
        public float refreshSeconds = 0.1f;

        private float _nextAt;

        private void Awake()
        {
            if (!label) label = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextAt) return;
            _nextAt = Time.unscaledTime + refreshSeconds;

            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem)
            {
                if (label) label.text = string.Empty;
                return;
            }

            var cfg = EnergyRuntime.Config;
            if (cfg == null)
            {
                if (label) label.text = "energy: (no config)";
                return;
            }

            var mgr = EnergyRuntime.Manager;
            if (mgr == null)
            {
                // Create a baseline manager if something else hasn't already.
                mgr = EnergyRuntime.Manager = new EnergyManager(cfg.Start, cfg.Cap, DateTime.UtcNow);
            }

            mgr.TickNow(cfg.RegenSecondsPerPoint, DateTime.UtcNow);

            string text;
            string text2;
            if (mgr.Current >= cfg.Cap)
            {
                text = $"{mgr.Current}";
            }
            else
            {
                int sp = Mathf.Max(1, cfg.RegenSecondsPerPoint);
                int since = Mathf.Max(0, (int)(DateTime.UtcNow - mgr.LastTickUtc).TotalSeconds);
                int until = sp - (since % sp);
                int m = until / 60;
                int s = until % 60;
                text = $"{mgr.Current}"; 
                text2 = $"(+1 in {m}:{s:00})";

            }

            if (label) label.text = text;
        }
    }
}
