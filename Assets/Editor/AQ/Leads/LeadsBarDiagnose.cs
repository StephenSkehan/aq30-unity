#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.Editor.Leads
{
    public static class LeadsBarDiagnose
    {
        private const string RootGOName   = "LeadsBar";
        private const string ScrollName   = "ScrollLeads";
        private const string ViewportName = "Viewport";
        private const string ContentName  = "Content_Leads";

        [MenuItem("AQ/Leads/Diagnose ScrollLeads")]
        public static void Diagnose()
        {
            var root = GameObject.Find(RootGOName);
            if (!root) { Debug.LogError($"❌ {RootGOName} not found."); return; }

            var scroll = root.transform.Find(ScrollName);
            if (!scroll) { Debug.LogError($"❌ {RootGOName}/{ScrollName} not found."); return; }

            var sr   = scroll.GetComponent<ScrollRect>();
            var img  = scroll.GetComponent<Image>();
            var mask = (Component)scroll.GetComponent<Mask>() ?? scroll.GetComponent<RectMask2D>();

            var vp   = scroll.Find(ViewportName) as RectTransform;
            var cont = vp ? vp.Find(ContentName) as RectTransform : null;

            var cardCount = cont ? cont.childCount : 0;

            string comps = "";
            foreach (var c in scroll.GetComponents<Component>())
            {
                if (!c) continue;
                comps += $"    - {c.GetType().Name}\n";
            }

            Debug.Log(
$@"🔎 LeadsBar/ScrollLeads diagnosis
Path: {GetPath(scroll)}
Components:
{comps}
ScrollRect: {(sr ? "FOUND" : "MISSING")}
  viewport: {(sr && sr.viewport ? sr.viewport.name : "(null)")}
  content : {(sr && sr.content  ? sr.content.name  : "(null)")}
Image: {(img ? "FOUND" : "MISSING")}
Mask/RectMask2D: {(mask ? $"FOUND ({mask.GetType().Name})" : "MISSING")}
Viewport child: {(vp ? "FOUND" : "MISSING")}
Content child : {(cont ? $"FOUND (cards={cardCount})" : "MISSING")}
Scene saved? {(IsSceneSaved() ? "Yes" : "No")}");

            // Bonus hint if the audit still says "(no ScrollRect)"
            if (sr && vp && cont && img)
                Debug.Log("✅ ScrollLeads has a ScrollRect + refs. If the Deep Audit still says '(no ScrollRect)', it’s reading a stale state or only checking the parent node. Run: AQ → Leads → Force Persist & Save.", scroll);
        }

        [MenuItem("AQ/Leads/Re-assert ScrollRect (rewire clean)")]
        public static void Reassert()
        {
            var root = GameObject.Find(RootGOName);
            if (!root) { Debug.LogError($"❌ {RootGOName} not found."); return; }

            var scroll = root.transform.Find(ScrollName) as RectTransform;
            if (!scroll)
            {
                scroll = new GameObject(ScrollName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(scroll.gameObject, "Create ScrollLeads");
                scroll.SetParent(root.transform, false);
                scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one;
                scroll.offsetMin = Vector2.zero; scroll.offsetMax = Vector2.zero;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();

            // Remove any old ScrollRect to avoid half-wired states
            var oldSr = scroll.GetComponent<ScrollRect>();
            if (oldSr) Undo.DestroyObjectImmediate(oldSr);

            var sr = Undo.AddComponent<ScrollRect>(scroll.gameObject);
            var img = scroll.GetComponent<Image>() ?? Undo.AddComponent<Image>(scroll.gameObject);
            img.color = new Color(0,0,0,0);

            // Ensure some mask exists
            if (!scroll.GetComponent<Mask>() && !scroll.GetComponent<RectMask2D>())
                Undo.AddComponent<RectMask2D>(scroll.gameObject);

            var viewport = scroll.Find(ViewportName) as RectTransform;
            if (!viewport)
            {
                viewport = new GameObject(ViewportName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(viewport.gameObject, "Create Viewport");
                viewport.SetParent(scroll, false);
                viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
                viewport.offsetMin = Vector2.zero; viewport.offsetMax = Vector2.zero;
                if (!viewport.GetComponent<Mask>() && !viewport.GetComponent<RectMask2D>())
                    Undo.AddComponent<RectMask2D>(viewport.gameObject);
            }

            var content = viewport.Find(ContentName) as RectTransform;
            if (!content)
            {
                content = new GameObject(ContentName, typeof(RectTransform)).GetComponent<RectTransform>();
                Undo.RegisterCreatedObjectUndo(content.gameObject, "Create Content_Leads");
                content.SetParent(viewport, false);
            }

            // Wire up
            sr.viewport = viewport;
            sr.content  = content;
            sr.horizontal = true;
            sr.vertical   = false;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.inertia = true;

            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("🛠️ Re-asserted ScrollRect and rewired viewport/content. Now run: AQ → Leads → Force Persist & Save.");
        }

        [MenuItem("AQ/Leads/Force Persist & Save")]
        public static void ForcePersistAndSave()
        {
            EditorSceneManager.MarkAllScenesDirty();
            var ok = EditorSceneManager.SaveOpenScenes();
            Debug.Log(ok ? "💾 Scenes saved. Wiring persisted." : "⚠️ SaveOpenScenes returned false (check console).");
        }

        private static string GetPath(Transform t)
        {
            if (!t) return "(null)";
            string path = t.name;
            while (t.parent) { t = t.parent; path = t.name + "/" + path; }
            var scene = t.gameObject.scene.IsValid() ? t.gameObject.scene.name : "(no-scene)";
            return $"{scene}/{path}";
        }

        private static bool IsSceneSaved()
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var s = EditorSceneManager.GetSceneAt(i);
                if (s.isDirty) return false;
            }
            return true;
        }
    }
}
#endif
