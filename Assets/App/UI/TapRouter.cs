using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.App.UI
{
    /// <summary>
    /// THE raw-input tap path (2026-08-18). House lesson: EventSystem clicks are
    /// unreliable on boot-created canvases, so surfaces poll Input directly — but
    /// each bespoke poll re-implemented hit-testing blind to UI stacking, which
    /// shipped a whole family of bugs (stash taps through the locker dim, a
    /// dialogue-dismiss tap firing an evidence-board pin, taps on invisible
    /// buttons). The router restores the two EventSystem guarantees raw polling
    /// lost:
    ///   1. STACKING — regions declare a layer (canvas sorting-order semantics);
    ///      the topmost enabled region containing the tap wins. An EventSystem
    ///      raycast surface ABOVE the winner (popup scrim, panel) blocks it, so
    ///      Button-based UI automatically shields raw regions beneath.
    ///   2. CLAIMING — exactly one consumer per physical tap, ever.
    /// Register a region instead of reading Input in Update. Taps are pointer
    /// DOWN events (touch Began / mouse down). Gesture surfaces (drag, pinch —
    /// e.g. EvidenceBoardZoomPan) stay bespoke; this router is taps only.
    /// </summary>
    public static class TapRouter
    {
        public sealed class Region
        {
            public string Name;
            /// <summary>Canvas sorting-order semantics. Highest containing region
            /// wins; an EventSystem hit STRICTLY above this order blocks it
            /// (equal order = the region's own raycastable graphics).</summary>
            public int Layer;
            /// <summary>Null = always enabled. Exceptions count as disabled.</summary>
            public Func<bool> Enabled;
            public Func<Vector2, bool> Contains;
            public Action<Vector2> OnTap;
        }

        private static readonly List<Region> _regions = new();
        private static readonly List<RaycastResult> _hits = new();

        // Statics survive play sessions when domain reload is off; a stale region
        // holds closures over destroyed objects (StudioSplashMB lesson).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _regions.Clear();

        public static Region Register(string name, int layer,
            Func<Vector2, bool> contains, Action<Vector2> onTap, Func<bool> enabled = null)
        {
            var r = new Region { Name = name, Layer = layer, Contains = contains, OnTap = onTap, Enabled = enabled };
            _regions.Add(r);
            return r;
        }

        public static void Unregister(Region region)
        {
            if (region != null) _regions.Remove(region);
        }

        /// <summary>
        /// Pure resolution — which region (if any) receives a tap at
        /// <paramref name="pos"/>. Highest layer wins; first-registered wins ties;
        /// a topmost EventSystem raycast order STRICTLY above the winner blocks
        /// delivery entirely (that surface owns the tap). Public and side-effect
        /// free so the routing contract is unit-testable without a scene.
        /// </summary>
        public static Region Resolve(Vector2 pos, IReadOnlyList<Region> regions, int topmostRaycastOrder)
        {
            Region best = null;
            for (int i = 0; i < regions.Count; i++)
            {
                var r = regions[i];
                if (r == null || r.Contains == null || r.OnTap == null) continue;

                bool eligible;
                try { eligible = (r.Enabled == null || r.Enabled()) && r.Contains(pos); }
                catch (Exception e)
                {
                    // A broken region must not take the whole input system with it.
                    Debug.LogWarning($"[TapRouter] Region '{r.Name}' threw during resolve; skipped. {e.Message}");
                    continue;
                }

                if (eligible && (best == null || r.Layer > best.Layer)) best = r;
            }

            if (best == null) return null;
            if (topmostRaycastOrder > best.Layer) return null; // EventSystem surface above owns this tap
            return best;
        }

        internal static void Pump()
        {
            Vector2 pos;
            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                if (t.phase != TouchPhase.Began) return;
                pos = t.position;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                pos = Input.mousePosition;
            }
            else return;

            var target = Resolve(pos, _regions, TopmostRaycastOrder(pos));
            if (target == null) return;

            try { target.OnTap(pos); }
            catch (Exception e) { Debug.LogError($"[TapRouter] Region '{target.Name}' handler threw: {e}"); }
        }

        private static int TopmostRaycastOrder(Vector2 pos)
        {
            var es = EventSystem.current;
            if (es == null) return int.MinValue;
            var ped = new PointerEventData(es) { position = pos };
            _hits.Clear();
            es.RaycastAll(ped, _hits);
            return _hits.Count > 0 ? _hits[0].sortingOrder : int.MinValue;
        }
    }

    /// <summary>Self-installing per-frame pump for the router (house idiom).</summary>
    [DefaultExecutionOrder(-100)]
    internal sealed class TapRouterPumpMB : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (UnityEngine.Object.FindAnyObjectByType<TapRouterPumpMB>() != null) return;
            var go = new GameObject("[TapRouter]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<TapRouterPumpMB>();
        }

        private void Update() => TapRouter.Pump();
    }
}
