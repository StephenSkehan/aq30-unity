#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

namespace AQ.EditorTools.Art
{
    public static class AuditTopBar
    {
        [MenuItem("AQ/Art/Audit TopBar (tree + guesses)")]
        public static void Run()
        {
            var top = Find("Canvas_Board/HUD_Board/TopBar");
            if (!top)
            {
                Debug.LogWarning("[AQ Art] TopBar not found at Canvas_Board/HUD_Board/TopBar");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("[AQ Art] --- TopBar Tree ---");
            Dump(top, 0, sb);
            sb.AppendLine("[AQ Art] --- Guesses ---");
            Guess(top, "Energy", sb);
            Guess(top, "Soft|Coin|Gold", sb);
            Guess(top, "Premium|Gem|Diamond|Ruby", sb);
            Guess(top, "Episode|Chip|Pill", sb, includeSelf:true);
            Guess(top, "Avatar|Portrait|Profile", sb, includeSelf:true);
            Guess(top, "Home", sb, includeSelf:true);

            Debug.Log(sb.ToString());
        }

        static Transform Find(string path)
        {
            var root = GameObject.Find("Canvas_Board");
            if (!root) return null;
            var t = root.transform;
            var parts = path.Split('/');
            foreach (var p in parts)
                if (p != "Canvas_Board")
                    t = FindChild(t, p) ?? t;
            return t;
        }

        static Transform FindChild(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c.name.ToLower().Contains(name.ToLower())) return c;
            }
            return null;
        }

        static void Dump(Transform t, int depth, StringBuilder sb)
        {
            var rt = t as RectTransform;
            var img = t.GetComponent<Image>();
            var btn = t.GetComponent<UnityEngine.UI.Button>();
            var tmp = t.GetComponent<TMP_Text>();
            sb.Append(' ', depth * 2);
            sb.Append("• ").Append(PathOf(t));
            if (rt) sb.Append($"  [{rt.rect.width:0}×{rt.rect.height:0}]");
            if (img) sb.Append($"  Image(sprite={(img.sprite ? img.sprite.name : "null")})");
            if (btn) sb.Append("  Button");
            if (tmp) sb.Append($"  TMP(\"{tmp.text}\")");
            sb.AppendLine();
            for (int i = 0; i < t.childCount; i++) Dump(t.GetChild(i), depth + 1, sb);
        }

        static string PathOf(Transform t)
        {
            System.Collections.Generic.List<string> parts = new();
            var cur = t;
            while (cur != null && cur.gameObject.scene.IsValid())
            {
                parts.Add(cur.name);
                cur = cur.parent;
                if (cur && cur.name == "Canvas_Board") break;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        static void Guess(Transform scope, string hints, StringBuilder sb, bool includeSelf=false)
        {
            var target = FindByHints(scope, hints);
            if (!target) { sb.AppendLine($"  {hints} -> (not found)"); return; }

            var img = includeSelf ? target.GetComponent<Image>() : null;
            if (!img) img = FindImageBelow(target);
            sb.AppendLine($"  {hints} -> {(img ? PathOf(img.transform) : PathOf(target))} {(img ? "(Image)" : "(no Image found)")} ");
        }

        static Transform FindByHints(Transform scope, string hints)
        {
            string[] keys = hints.Split('|');
            var stack = new System.Collections.Generic.Stack<Transform>();
            stack.Push(scope);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                foreach (var k in keys)
                    if (t.name.ToLower().Contains(k.ToLower())) return t;
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return null;
        }

        static Image FindImageBelow(Transform t)
        {
            return t.GetComponentInChildren<Image>(true);
        }
    }
}
#endif
