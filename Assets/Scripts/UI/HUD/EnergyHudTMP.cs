using System;
using System.Collections;
using UnityEngine;
using TMPro;
using AQ.App.Config;
using AQ.App.Services;

namespace AQ.App.UI.HUD
{
    /// <summary>
    /// TextMeshPro version of the Energy HUD.
    /// - Works with TextMeshProUGUI.
    /// - Respects FeatureFlags.EnergySystem.
    /// - Shows "Energy: {current}/{cap} (mm:ss)" countdown when not at cap.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class EnergyHudTMP : MonoBehaviour
    {
        [Tooltip("TextMeshProUGUI to update; defaults to component on this GameObject.")]
        public TextMeshProUGUI target;

        [Tooltip("Update period in seconds.")]
        [Range(0.05f, 2f)] public float updateInterval = 0.25f;

        [Tooltip("Show countdown to next regen point when not at cap.")]
        public bool showCountdown = true;

        [Tooltip("Prefix shown before the numbers.")]
        public string prefix = "Energy: ";

        void Awake()
        {
            if (!target) target = GetComponent<TextMeshProUGUI>();
        }

        void OnEnable()
        {
            StartCoroutine(Run());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator Run()
        {
            var wait = new WaitForSeconds(updateInterval > 0f ? updateInterval : 0.25f);
            while (true)
            {
                UpdateText();
                yield return wait;
            }
        }

        void UpdateText()
        {
            if (!target) return;

            var flags = FeatureFlagsRuntime.Current;
            if (flags == null || !flags.EnergySystem)
            {
                target.text = prefix + "—";
                return;
            }

            var cfg = EnergyRuntime.Config;
            var mgr = EnergyRuntime.Manager;
            if (cfg == null || mgr == null)
            {
                target.text = prefix + "—";
                return;
            }

            var now = DateTime.UtcNow;
            mgr.TickNow(cfg.RegenSecondsPerPoint, now);

            string suffix = "";
            if (showCountdown && mgr.Current < mgr.Cap && cfg.RegenSecondsPerPoint > 0)
            {
                var elapsed = (int)(now - mgr.LastTickUtc).TotalSeconds;
                int rem = cfg.RegenSecondsPerPoint - (elapsed % cfg.RegenSecondsPerPoint);
                if (rem == cfg.RegenSecondsPerPoint) rem = 0;
                int mm = rem / 60;
                int ss = rem % 60;
                suffix = $" ({mm:00}:{ss:00})";
            }

            target.text = $"{prefix}{mgr.Current}/{mgr.Cap}{suffix}";
        }
    }
}
