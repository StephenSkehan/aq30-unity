// LeadsBarRuntimeBinder.cs
// Robust binder for LeadsBarView that tolerates different repo/bus APIs.
// - Tries to hook repo/bus events via reflection (no hard deps).
// - Falls back to a periodic refresh if no events are found.

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AQ.App.Leads
{
    [DisallowMultipleComponent]
    public sealed class LeadsBarRuntimeBinder : MonoBehaviour
    {
        [Tooltip("If null, taken from the same GameObject.")]
        public LeadsBarView view;

        [Header("Fallback refresh (used if no events found)")]
        public bool fallbackAutoRefresh = true;
        [Range(0.1f, 3f)] public float fallbackInterval = 0.75f;

        object _repoInstance;
        EventInfo _evRefreshed;
        EventInfo _evStateChanged;
        Delegate _hRefreshed;
        Delegate _hStateChanged;

        MethodInfo _findFirstObjGeneric; // UnityEngine.Object.FindFirstObjectByType<T>()

        void Awake()
        {
            if (view == null) view = GetComponent<LeadsBarView>();
            // Cache generic method via reflection to avoid obsolete warnings.
            _findFirstObjGeneric = typeof(UnityEngine.Object)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "FindFirstObjectByType" && m.IsGenericMethod);
        }

        void OnEnable()
        {
            TryHookRepository();
            TryHookRuntimeBus();
            // Always do an initial draw
            SafeRebuild("initial");
            // Start fallback only if we didn’t hook any events
            if (!HasAnyHook() && fallbackAutoRefresh)
                InvokeRepeating(nameof(FallbackTick), fallbackInterval, fallbackInterval);
        }

        void OnDisable()
        {
            Unhook();
            CancelInvoke(nameof(FallbackTick));
        }

        bool HasAnyHook() => _hRefreshed != null || _hStateChanged != null;

        void FallbackTick() => SafeRebuild("fallback");

        void SafeRebuild(string reason)
        {
            if (!view) return;
            try { view.Rebuild(); }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LeadsBinder] Rebuild failed ({reason}): {ex.Message}", this);
            }
        }

        #region Hook Repository (reflection)
        void TryHookRepository()
        {
            var repoType = Type.GetType("AQ.App.Leads.LeadsRepository, Assembly-CSharp");
            if (repoType == null) return;

            // Try to find a scene instance without using obsolete APIs.
            if (_findFirstObjGeneric != null)
            {
                var m = _findFirstObjGeneric.MakeGenericMethod(repoType);
                _repoInstance = m.Invoke(null, null);
            }

            if (_repoInstance == null)
            {
                // Last resort: Resources (works in-editor too).
                _repoInstance = Resources.FindObjectsOfTypeAll(repoType).FirstOrDefault();
            }
            if (_repoInstance == null) return;

            // Try common event names
            _evRefreshed   = repoType.GetEvent("LeadsRefreshed",   BindingFlags.Public | BindingFlags.Instance);
            _evStateChanged= repoType.GetEvent("LeadStateChanged",  BindingFlags.Public | BindingFlags.Instance);

            _hRefreshed    = TryAttachHandler(_repoInstance, _evRefreshed,    nameof(OnRepoEvent));
            _hStateChanged = TryAttachHandler(_repoInstance, _evStateChanged, nameof(OnRepoEvent));

            if (HasAnyHook())
                Debug.Log("[LeadsBinder] Hooked repository events.", this);
        }

        void OnRepoEvent()                         => SafeRebuild("repo evt (no args)");
        void OnRepoEvent(object _a)               => SafeRebuild("repo evt (1 arg)");
        void OnRepoEvent(object _a, object _b)    => SafeRebuild("repo evt (2 args)");

        #endregion

        #region Hook Runtime Bus (optional, reflection)
        void TryHookRuntimeBus()
        {
            // If your codebase has a static bus with events, we’ll hook those too.
            var busType = Type.GetType("AQ.App.Leads.LeadsRuntimeBus, Assembly-CSharp");
            if (busType == null) return;

            // Try common static events on a bus
            var evAll   = busType.GetEvent("LeadsRefreshed", BindingFlags.Public | BindingFlags.Static)
                       ?? busType.GetEvent("All",            BindingFlags.Public | BindingFlags.Static);
            var evState = busType.GetEvent("LeadStateChanged", BindingFlags.Public | BindingFlags.Static)
                       ?? busType.GetEvent("State",            BindingFlags.Public | BindingFlags.Static);

            // Note: for static events, target instance is null.
            var hAll   = TryAttachHandler(null, evAll,   nameof(OnBusEvent));
            var hState = TryAttachHandler(null, evState, nameof(OnBusEvent));
            if (hAll != null || hState != null)
                Debug.Log("[LeadsBinder] Hooked runtime bus events.", this);
        }

        void OnBusEvent()                       => SafeRebuild("bus evt (no args)");
        void OnBusEvent(object _a)              => SafeRebuild("bus evt (1 arg)");
        void OnBusEvent(object _a, object _b)   => SafeRebuild("bus evt (2 args)");
        #endregion

        #region Helpers
        Delegate TryAttachHandler(object target, EventInfo ev, string handlerName)
        {
            if (ev == null) return null;
            var handlerType = ev.EventHandlerType;
            if (handlerType == null) return null;

            // Try to bind to one of our overloads (0/1/2 params)
            var method = GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance,
                                             null, Type.EmptyTypes, null)
                      ?? GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance,
                                             null, new[] { typeof(object) }, null)
                      ?? GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance,
                                             null, new[] { typeof(object), typeof(object) }, null);
            if (method == null) return null;

            try
            {
                var del = Delegate.CreateDelegate(handlerType, this, method, throwOnBindFailure: false);
                if (del != null)
                {
                    ev.AddEventHandler(target, del);
                    return del;
                }
            }
            catch { /* ignore and fall back */ }
            return null;
        }

        void Unhook()
        {
            if (_evRefreshed != null && _hRefreshed != null)
            {
                try { _evRefreshed.RemoveEventHandler(_repoInstance, _hRefreshed); } catch {}
            }
            if (_evStateChanged != null && _hStateChanged != null)
            {
                try { _evStateChanged.RemoveEventHandler(_repoInstance, _hStateChanged); } catch {}
            }
            _hRefreshed = _hStateChanged = null;
            _evRefreshed = _evStateChanged = null;
            _repoInstance = null;
        }
        #endregion
    }
}
