using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AQ.EditorTools
{
    /// <summary>
    /// Logs the full UI raycast stack at a few board-area screen positions —
    /// the top entry is whatever is eating clicks. Play mode only.
    /// </summary>
    public static class RaycastProbe
    {
        static void FLog(string msg)
        {
            Debug.Log(msg);
            System.IO.File.AppendAllText("Screenshots/probe_log.txt", msg + System.Environment.NewLine);
        }

        [MenuItem("AQ/Dev/Raycast Probe (board area)")]
        public static void Probe()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Probe] enter play mode first."); return; }
            var es = EventSystem.current;
            if (es == null) { FLog("[Probe] ERROR NO EventSystem in scene — that alone kills all clicks."); return; }
            FLog($"[Probe] EventSystem: {es.gameObject.name}, module: {es.currentInputModule?.GetType().Name ?? "NULL (no input module!)"}");

            var points = new[] {
                new Vector2(Screen.width * 0.5f,  Screen.height * 0.45f), // board centre
                new Vector2(Screen.width * 0.25f, Screen.height * 0.35f), // board lower-left
                new Vector2(Screen.width * 0.5f,  Screen.height * 0.90f), // HUD (control)
            };
            foreach (var p in points)
            {
                var data = new PointerEventData(es) { position = p };
                var hits = new List<RaycastResult>();
                es.RaycastAll(data, hits);
                FLog($"[Probe] at {p}: {hits.Count} hits");
                for (int i = 0; i < Mathf.Min(hits.Count, 6); i++)
                {
                    var h = hits[i];
                    var canvas = h.gameObject.GetComponentInParent<Canvas>();
                    FLog($"[Probe]   {i}: {Path(h.gameObject.transform)} (canvas '{canvas?.name}' order {canvas?.sortingOrder})");
                }
            }
        }

        static string Path(Transform t)
        {
            var s = t.name;
            while (t.parent != null && s.Length < 90) { t = t.parent; s = t.name + "/" + s; }
            return s;
        }

        [MenuItem("AQ/Dev/Simulate Proceed On Ready Lead")]
        public static void SimulateProceed()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Probe] enter play mode first."); return; }
            // Txt_ProceedHint was retired 2026-07-20 — find the first lead card with a Button
            var bar = GameObject.Find("LeadsBarRuntime");
            var btnAny = bar != null ? bar.GetComponentInChildren<UnityEngine.UI.Button>(true) : null;
            GameObject card = btnAny != null ? btnAny.gameObject : null;
            if (card == null) { FLog("[Probe] ERROR no ready lead card found"); return; }
            var btn = card.GetComponentInChildren<UnityEngine.UI.Button>(true);
            if (btn == null) { FLog("[Probe] ERROR no button on card " + card.name); return; }
            FLog($"[Probe] invoking proceed on '{btn.gameObject.name}' (card '{card.name}')");
            btn.onClick.Invoke();
            FLog("[Probe] proceed invoked.");
        }

        [MenuItem("AQ/Dev/Dump Dialogue State")]
        public static void DumpDialogue()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Probe] enter play mode first."); return; }
            var runner = Object.FindAnyObjectByType<AQ.App.DialogueRunner>(FindObjectsInactive.Include);
            if (runner == null) { FLog("[Probe] no DialogueRunner"); return; }
            var t = runner.GetType();
            var panel = runner.Panel;
            FLog($"[Probe] runner active={runner.isActiveAndEnabled} panel={(panel ? panel.gameObject.name : "null")} panelActive={(panel && panel.gameObject.activeInHierarchy)}");
            foreach (var fname in new[] { "_currentId", "_booted", "_waitingForAudio", "_filteredChoices" })
            {
                var f = t.GetField(fname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                object v = f?.GetValue(runner);
                if (v is System.Array arr) v = $"array[{arr.Length}]";
                FLog($"[Probe]   {fname} = {v ?? "null"}");
            }
            // try to advance exactly the way a tap would
            var adv = t.GetMethod("OnAdvance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            FLog($"[Probe]   OnAdvance method: {(adv != null ? "found — invoking" : "NOT FOUND")}");
            if (adv != null && panel != null && panel.gameObject.activeInHierarchy)
            {
                try { adv.Invoke(runner, adv.GetParameters().Length == 0 ? null : new object[adv.GetParameters().Length]); FLog("[Probe]   OnAdvance invoked OK"); }
                catch (System.Exception e) { FLog("[Probe]   OnAdvance THREW: " + e.InnerException?.Message); }
            }
        }

        [MenuItem("AQ/Dev/Simulate Click On Generator")]
        public static void SimulateClick()
        {
            if (!Application.isPlaying) { Debug.LogWarning("[Probe] enter play mode first."); return; }
            var es = EventSystem.current;

            // find a generator slot: any BoardTileView-bearing slot with a visible item image
            GameObject target = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (mb.GetType().Name != "BoardTileView") continue;
                var kindProp = mb.GetType().GetProperty("Kind");
                if (kindProp != null && kindProp.GetValue(mb)?.ToString() == "Generator") { target = mb.gameObject; break; }
            }
            if (target == null) { FLog("[Probe] ERROR no generator tile found"); return; }

            var handlers = target.GetComponents<MonoBehaviour>();
            foreach (var h in handlers)
                FLog($"[Probe] on '{target.name}': {h.GetType().Name} enabled={h.enabled} " +
                          $"(IPointerDown={(h is IPointerDownHandler)}, IPointerUp={(h is IPointerUpHandler)}, IPointerClick={(h is IPointerClickHandler)}, IDrag={(h is IDragHandler)})");

            var pos = RectTransformUtility.WorldToScreenPoint(null, target.transform.position);
            var data = new PointerEventData(es) { position = pos, button = PointerEventData.InputButton.Left };
            FLog($"[Probe] simulating pointer down/up/click on '{target.name}' at {pos}");
            ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(target, data, ExecuteEvents.pointerClickHandler);
            FLog("[Probe] simulation done — check for spawn/tap logs above.");
        }
    }
}
