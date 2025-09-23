// Assets/Editor/AQ/Art/ArtUtil.cs
// Small helpers used by the other tools.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AQ.EditorTools.Art
{
    internal static class ArtUtil
    {
        internal static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c ? c : go.AddComponent<T>();
        }

        internal static void StretchRect(RectTransform rt, Vector2 pivot, Vector2 size)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = pivot;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, size.y); // width controlled by parent
        }

        internal static void FullStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        /// Robust sprite lookup: exact path -> by filename anywhere -> first sprite in texture
        internal static Sprite LoadSprite(string preferPathOrFileName)
        {
            // 1) Direct path?
            var byPath = AssetDatabase.LoadAssetAtPath<Sprite>(preferPathOrFileName);
            if (byPath) return byPath;

            // 2) Search by filename (without extension) anywhere under Assets
            var name = System.IO.Path.GetFileNameWithoutExtension(preferPathOrFileName);
            var guids = AssetDatabase.FindAssets($"{name} t:sprite");
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (sp && sp.name == name) return sp;
            }
            // fallback to first found if names differ (e.g., multi-sprite sheets)
            if (guids.Length > 0)
            {
                var p = AssetDatabase.GUIDToAssetPath(guids[0]);
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (sp) return sp;
            }

            Debug.LogWarning($"[AQ Art] Sprite missing: {preferPathOrFileName}");
            return null;
        }

        internal static Image MakeImage(Transform parent, string name, float w, float h, Sprite sprite, Image.Type type, bool preserveAspect=false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(w, h);

            var img = go.GetComponent<Image>();
            img.type = type;
            img.sprite = sprite;
            img.preserveAspect = preserveAspect;
            return img;
        }
    }
}
#endif
