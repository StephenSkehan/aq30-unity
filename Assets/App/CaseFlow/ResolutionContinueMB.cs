using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using AQ.SharedKernel.Economy;   // IWallet, Reward, WalletService
using AQ.App.Analytics;          // AnalyticsLocator, IAnalytics, DebugLogAnalytics (if public)

namespace AQ.App.CaseFlow
{
    /// <summary>Handles the "Continue" action on the Resolution overlay:
    /// - Applies rewards (if a wallet is discoverable)
    /// - Emits analytics (best-effort; optional)
    /// - Hides the overlay (fade via CanvasGroup, else deactivate)
    /// Persistence is handled by BoardSaveSystem's change-triggered autosave.
    /// </summary>
    public class ResolutionContinueMB : MonoBehaviour
    {
        [Header("UI (optional but recommended)")]
        [Tooltip("CanvasGroup on the ResolutionRoot for a nice fade-out. If null, we'll simply deactivate the root.")]
        public CanvasGroup group;

        [Tooltip("Root GameObject to hide after pressing Continue (usually 'ResolutionRoot'). If null, we'll use this GameObject.")]
        public GameObject rootToHide;

        [Tooltip("Seconds to fade the CanvasGroup when hiding.")]
        public float fadeDuration = 0.25f;

        [Header("Button (optional)")]
        [Tooltip("Resolve/Continue button. If assigned, we'll auto-wire its onClick.")]
        public Button resolveButton;

        [Header("Variant/Design Inputs")]
        [Tooltip("Rewards to grant on continue. Edit-time defaults can be set here; Variant A/B/C can also drive these.")]
        public int soft = 500;
        public int energy = 10;
        public int premium = 0;

        // Cache (runtime)
        IWallet _wallet;          // discovered wallet
        bool    _triedDiscover;   // avoid re-reflection spam

        void Awake()
        {
            if (!rootToHide) rootToHide = gameObject;
        }

        void OnEnable()
        {
            if (resolveButton)
            {
                resolveButton.onClick.RemoveListener(OnResolve);
                resolveButton.onClick.AddListener(OnResolve);
            }
        }

        /// <summary>
        /// Main entry from the Resolve/Continue button.
        /// Order: 1) rewards, 2) analytics, 3) hide.
        /// </summary>
        public void OnResolve()
        {
            var rewards = BuildRewards();
            TryApplyRewards(rewards);
            TryAnalytics(rewards);
            HideOverlay();
        }

        Reward[] BuildRewards()
        {
            var list = new List<Reward>(3);
            if (soft    > 0) list.Add(Reward.Soft(soft));
            if (energy  > 0) list.Add(Reward.Energy(energy));
            if (premium > 0) list.Add(Reward.Premium(premium));
            return list.ToArray();
        }

        void TryApplyRewards(Reward[] rewards)
        {
            if (rewards == null || rewards.Length == 0) return;

            var wallet = GetOrDiscoverWallet();
            if (wallet == null)
            {
                Debug.LogWarning("[ResolutionContinueMB] WalletService not found; rewards not applied.");
                return;
            }

            wallet.Grant("grant", rewards);

            int s = 0, e = 0, p = 0;
            foreach (var r in rewards)
            {
                switch (r.Currency)
                {
                    case Currency.Soft:    s += r.Amount; break;
                    case Currency.Energy:  e += r.Amount; break;
                    case Currency.Premium: p += r.Amount; break;
                }
            }
            Debug.Log($"[ResolutionContinueMB] Granted rewards → soft={s}, energy={e}, premium={p}");
        }

        void TryAnalytics(Reward[] rewards)
        {
            // Ensure an analytics backend exists; if not, set one.
            IAnalytics analytics = AnalyticsLocator.Instance;
            if (analytics == null)
            {
                try
                {
                    analytics = new DebugLogAnalytics();
                    AnalyticsLocator.Set(analytics);
                }
                catch
                {
                    analytics = new FallbackAnalytics();
                    AnalyticsLocator.Set(analytics);
                }
            }

            if (analytics == null)
            {
                Debug.LogWarning("[ResolutionContinueMB] IAnalytics not found; skipping 'resolution_continue' emit.");
                return;
            }

            var payload = new Dictionary<string, object>
            {
                { "soft", soft },
                { "energy", energy },
                { "premium", premium },
                { "rewards_count", rewards?.Length ?? 0 }
            };

            analytics.LogEvent("resolution_continue", payload);
        }

        void HideOverlay()
        {
            if (group != null && fadeDuration > 0f && isActiveAndEnabled)
            {
                StartCoroutine(FadeAndDisable(group, rootToHide));
            }
            else
            {
                if (rootToHide) rootToHide.SetActive(false);
            }
        }

        IEnumerator FadeAndDisable(CanvasGroup cg, GameObject go)
        {
            float t = 0f;
            float start = cg.alpha;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            cg.alpha = 0f;
            if (go) go.SetActive(false);
        }

        // --------- discovery helpers ---------

        IWallet GetOrDiscoverWallet()
        {
            if (_wallet != null) return _wallet;
            if (_triedDiscover)  return null;

            _triedDiscover = true;

            // 1) Look for an existing IWallet/WalletService exposed by any MB (includes inactive)
#if UNITY_2022_2_OR_NEWER
            var mbs = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
#else
            var mbs = FindObjectsOfType<MonoBehaviour>(includeInactive: true);
#endif
            foreach (var mb in mbs)
            {
                if (!mb) continue;
                var type = mb.GetType();

                // fields
                foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (TryWalletFromObject(f.GetValue(mb), out _wallet)) return _wallet;
                }
                // properties
                foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!p.CanRead) continue;
                    object val;
                    try { val = p.GetValue(mb, null); } catch { continue; }
                    if (TryWalletFromObject(val, out _wallet)) return _wallet;
                }
            }

            // 2) Dev fallback: create a local wallet so the flow works end-to-end
            try
            {
                _wallet = new WalletService();
                Debug.Log("[ResolutionContinueMB] Created local WalletService (dev fallback).");
            }
            catch
            {
                _wallet = null;
            }
            return _wallet;
        }

        static bool TryWalletFromObject(object obj, out IWallet wallet)
        {
            wallet = null;
            if (obj == null) return false;

            if (obj is IWallet w) { wallet = w; return true; }
            if (obj is WalletService ws) { wallet = ws; return true; }

            return false;
        }

        /// <summary>
        /// Minimal debug analytics used if no backend is registered and DebugLogAnalytics isn't constructible.
        /// Must fulfill the IAnalytics interface.
        /// </summary>
        private sealed class FallbackAnalytics : IAnalytics
        {
            public void LogEvent(string name, IDictionary<string, object> fields = null)
            {
                if (fields == null || fields.Count == 0)
                {
                    Debug.Log("[Analytics] " + name);
                    return;
                }
                var parts = fields.Select(kv => kv.Key + ": " + (kv.Value ?? "null"));
                Debug.Log("[Analytics] " + name + " { " + string.Join(", ", parts) + " }");
            }

            public void SetUserProperty(string key, string value)
            {
                Debug.Log("[Analytics] set_user_property " + key + "=" + (value ?? "null"));
            }
        }
    }
}
