#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    /// <summary>
    /// One-click live probe for CaseFlow gating in the current scene.
    /// Logs whether key gates exist and what their runtime state is.
    /// </summary>
    public static class AQ_GateProbe
    {
        [MenuItem("AQ/CaseFlow/Live Gate Probe")]
        public static void Probe()
        {
            Debug.Log("[GateProbe] ----- Live Gate Probe -----");

            ProbeGateByName("Minigame_Scrub");
            ProbeGateByName("ResolutionRoot");

            Debug.Log("[GateProbe] Done.");
        }

        static void ProbeGateByName(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[GateProbe] {name} not found.");
                return;
            }

            var active = go.activeSelf;
            var activeInHierarchy = go.activeInHierarchy;
            Debug.Log($"[GateProbe] {name} -> activeSelf={active}, activeInHierarchy={activeInHierarchy}");

            // Try to locate a CaseFlowGateMB component by type name (avoids hard assembly coupling)
            var gate = go.GetComponents<MonoBehaviour>()
                         .FirstOrDefault(c => c != null && c.GetType().Name == "CaseFlowGateMB");

            if (gate == null)
            {
                Debug.Log($"[GateProbe] {name}: no CaseFlowGateMB component attached.");
                return;
            }

            var t = gate.GetType();
            var modeField          = t.GetField("mode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var requiredIndexField = t.GetField("requiredIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var targetField        = t.GetField("target", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var pollEveryFrame     = t.GetField("pollEveryFrame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var modeStr    = modeField != null ? (modeField.GetValue(gate)?.ToString() ?? "null") : "unknown";
            var reqIndex   = requiredIndexField != null ? (int)requiredIndexField.GetValue(gate) : -1;
            var targetObj  = targetField != null ? (UnityEngine.Object)targetField.GetValue(gate) : null;
            var pollStr    = pollEveryFrame != null ? ((bool)pollEveryFrame.GetValue(gate)).ToString() : "unknown";

            Debug.Log($"[GateProbe] {name}: gateMode={modeStr} reqIndex={reqIndex} pollEveryFrame={pollStr} target={(targetObj ? targetObj.name : "self")}");

            // If there's a CanvasGroup, print its interactivity
            var cg = (go as GameObject).GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log($"[GateProbe] {name}: CanvasGroup alpha={cg.alpha} interactable={cg.interactable} blocksRaycasts={cg.blocksRaycasts}");
            }
        }
    }
}
#endif
