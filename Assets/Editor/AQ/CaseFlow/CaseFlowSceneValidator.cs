#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools
{
    public static class CaseFlowSceneValidator
    {
        const string MinigameRootName   = "Minigame_Scrub";
        const string ResolutionRootName = "ResolutionRoot";
        const string DialoguePanelName  = "DialoguePanel";

        [MenuItem("AQ/Validate/CaseFlow Scene")]
        public static void ValidateSceneMenu() => ValidateSceneCore(false);

        // CLI: -executeMethod AQ.EditorTools.CaseFlowSceneValidator.Run
        public static void Run() => ValidateSceneCore(true);

        static void ValidateSceneCore(bool fromCli)
        {
            var sb = new StringBuilder();
            void Log(string tag, string msg) { var line = $"{tag} {msg}"; Debug.Log(line); sb.AppendLine(line); }
            string PASS(string m) => $"[OK]   {m}";
            string WARN(string m) => $"[WARN] {m}";
            string FAIL(string m) => $"[FAIL] {m}";

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Log("[INFO]", $"Scene: {scene.name}");

            // Root object lookup (exact names from your screenshot)
            var roots = scene.GetRootGameObjects();
            GameObject FindRoot(string name) =>
                roots.FirstOrDefault(r => r.name == name) ?? GameObject.Find(name);

            var minigameRoot   = FindRoot(MinigameRootName);
            var resolutionRoot = FindRoot(ResolutionRootName);
            var dialoguePanel  = FindRoot(DialoguePanelName);

            Log("[INFO]", $"Find '{MinigameRootName}': {(minigameRoot ? "FOUND" : "MISSING")}");
            Log("[INFO]", $"Find '{ResolutionRootName}': {(resolutionRoot ? "FOUND" : "MISSING")}");
            Log("[INFO]", $"Find '{DialoguePanelName}': {(dialoguePanel ? "FOUND" : "MISSING")}");

            // Types by name (tolerant across versions)
            var gateType = FindTypeByName("CaseFlowGateMB");
            var advType  = FindTypeByName("CaseFlowAdvanceOnEventMB");
            var orchType = FindTypeByName("CaseFlowOrchestratorMB");

            if (gateType == null) Log("[INFO]", WARN("Type 'CaseFlowGateMB' not found."));
            if (advType  == null) Log("[INFO]", WARN("Type 'CaseFlowAdvanceOnEventMB' not found."));
            if (orchType == null) Log("[INFO]", WARN("Type 'CaseFlowOrchestratorMB' not found."));

            // --- Minigame checks ---
            if (minigameRoot)
            {
                // Button wiring: onClick -> CaseFlowAdvanceOnEventMB.Advance
                var button = minigameRoot.GetComponentsInChildren<Button>(true).FirstOrDefault();
                if (button == null)
                {
                    Log("[CHECK]", WARN($"{MinigameRootName}: No Button found under this root (used for test advance)."));
                }
                else
                {
                    bool hasAdvance = false;
                    int evCount = button.onClick != null ? button.onClick.GetPersistentEventCount() : 0;
                    for (int i = 0; i < evCount; i++)
                    {
                        var target = button.onClick.GetPersistentTarget(i);
                        var method = button.onClick.GetPersistentMethodName(i);
                        if (target != null && target.GetType().Name == "CaseFlowAdvanceOnEventMB" && method == "Advance")
                        {
                            hasAdvance = true; break;
                        }
                    }
                    Log("[CHECK]", hasAdvance
                        ? PASS($"{MinigameRootName}/Button.onClick → CaseFlowAdvanceOnEventMB.Advance is wired.")
                        : WARN($"{MinigameRootName}/Button.onClick does not call CaseFlowAdvanceOnEventMB.Advance (manual advance still OK for testing)."));
                }

                // Gate on minigame root
                var mgGate = gateType != null ? minigameRoot.GetComponent(gateType) : null;
                if (mgGate == null) Log("[CHECK]", WARN($"{MinigameRootName}: No CaseFlowGateMB on root."));
                else DescribeGate(mgGate, $"{MinigameRootName} gate", Log, PASS, WARN);
            }
            else
            {
                Log("[CHECK]", FAIL($"'{MinigameRootName}' GameObject is missing in the open scene."));
            }

            // --- Resolution UI checks ---
            if (resolutionRoot)
            {
                var canvas = resolutionRoot.GetComponentInParent<Canvas>() ?? resolutionRoot.GetComponent<Canvas>();
                if (canvas == null) Log("[CHECK]", FAIL($"{ResolutionRootName}: No Canvas found (UI won't render)."));
                else Log("[CHECK]", PASS($"{ResolutionRootName}: Canvas present (mode={canvas.renderMode}, order={canvas.sortingOrder}, overrideSorting={canvas.overrideSorting})."));

                var resGate = gateType != null ? resolutionRoot.GetComponent(gateType) : null;
                if (resGate == null) Log("[CHECK]", WARN($"{ResolutionRootName}: No CaseFlowGateMB found."));
                else DescribeGate(resGate, $"{ResolutionRootName} gate", Log, PASS, WARN);

                bool hasAnyGraphic = resolutionRoot.GetComponentsInChildren<MaskableGraphic>(true).Any();
                Log("[CHECK]", hasAnyGraphic
                    ? PASS($"{ResolutionRootName}: Contains UI graphics (panel/text/button).")
                    : WARN($"{ResolutionRootName}: No UI graphics detected under this root."));
            }
            else
            {
                Log("[CHECK]", FAIL($"'{ResolutionRootName}' GameObject is missing in the open scene."));
            }

            // --- Live (Play Mode) snapshot ---
            if (Application.isPlaying && orchType != null)
            {
                UnityEngine.Object orch =
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
                    UnityEngine.Object.FindAnyObjectByType(orchType) ?? UnityEngine.Object.FindFirstObjectByType(orchType);
#else
                    UnityEngine.Object.FindObjectOfType(orchType);
#endif
                if (orch == null) Log("[LIVE]", WARN("Orchestrator not found in Play mode."));
                else
                {
                    var stepVal = ReadIntByHint(orch, "step", "index", "current");
                    Log("[LIVE]", PASS($"Orchestrator current stepIndex ≈ {stepVal ?? -1} (reflection)."));

                    if (minigameRoot)
                        Log("[LIVE]", minigameRoot.activeInHierarchy
                            ? PASS($"{MinigameRootName}: activeInHierarchy=True")
                            : WARN($"{MinigameRootName}: activeInHierarchy=False"));

                    if (resolutionRoot)
                        Log("[LIVE]", resolutionRoot.activeInHierarchy
                            ? PASS($"{ResolutionRootName}: activeInHierarchy=True")
                            : WARN($"{ResolutionRootName}: activeInHierarchy=False"));
                }
            }
            else
            {
                Log("[INFO]", "Live gating check skipped (enter Play mode and run again for live state).");
            }

            // Save report
            var outDir = Path.Combine(Directory.GetCurrentDirectory(), "_Audit");
            Directory.CreateDirectory(outDir);
            var path  = Path.Combine(outDir, $"CaseFlow_Validate_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(path, sb.ToString());
            Log("[INFO]", $"Report saved → {path}");
        }

        static Type FindTypeByName(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetTypes().FirstOrDefault(x => x.Name == name); }
                catch { /* ignore */ }
                if (t != null) return t;
            }
            return null;
        }

        static int? ReadIntByHint(object obj, params string[] hints)
        {
            var t = obj.GetType();
            foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (p.PropertyType == typeof(int) && hints.Any(h => p.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return (int)p.GetValue(obj); } catch {}
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (f.FieldType == typeof(int) && hints.Any(h => f.Name.IndexOf(f.Name, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return (int)f.GetValue(obj); } catch {}
            return null;
        }

        static bool? ReadBoolByHint(object obj, params string[] hints)
        {
            var t = obj.GetType();
            foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (p.PropertyType == typeof(bool) && hints.Any(h => p.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return (bool)p.GetValue(obj); } catch {}
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (f.FieldType == typeof(bool) && hints.Any(h => f.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return (bool)f.GetValue(obj); } catch {}
            return null;
        }

        static string ReadEnumNameByHint(object obj, params string[] hints)
        {
            var t = obj.GetType();
            foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (p.PropertyType.IsEnum && hints.Any(h => p.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return p.GetValue(obj)?.ToString() ?? "(null)"; } catch {}
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (f.FieldType.IsEnum && hints.Any(h => f.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0))
                    try { return f.GetValue(obj)?.ToString() ?? "(null)"; } catch {}
            return null;
        }

        static void DescribeGate(object gate, string label,
            Action<string,string> Log, Func<string,string> PASS, Func<string,string> WARN)
        {
            var idx  = ReadIntByHint(gate, "Required", "At", "Index", "Step");
            var mode = ReadEnumNameByHint(gate, "Mode", "Gate", "When");
            var aom  = ReadBoolByHint(gate, "ActiveOnMatch", "ActivateOnMatch", "ShowOnMatch", "VisibleOnMatch");

            Log("[GATE]", PASS($"{label}: type={gate.GetType().Name}, mode={(mode ?? "(unknown)")}, index={(idx?.ToString() ?? "(unknown)")}, ActiveOnMatch={(aom?.ToString() ?? "(unknown)")}."));
            if (idx == null)  Log("[GATE]", WARN($"{label}: could not read a required/at index field (name differs in this version)."));
            if (mode == null) Log("[GATE]", WARN($"{label}: could not read a mode enum (name differs in this version)."));
        }
    }
}
#endif
