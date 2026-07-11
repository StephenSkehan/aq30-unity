using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AQ.EditorTools
{
    public static class BuildFontAssets
    {
        [MenuItem("AQ/Setup/Build TMP Font Assets (Staatliches + Nunito Sans)")]
        public static void Build()
        {
            Create("Assets/Fonts/Staatliches-Regular.ttf", "Assets/Resources/App/UI/Fonts/Staatliches SDF.asset");
            Create("Assets/Fonts/NunitoSans-Variable.ttf", "Assets/Resources/App/UI/Fonts/NunitoSans SDF.asset");
            SetProjectDefaultFont();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fonts] TMP font assets built (dynamic atlases).");
        }

        /// <summary>Every TMP text created without an explicit font gets Nunito Sans.</summary>
        static void SetProjectDefaultFont()
        {
            var nunito = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/App/UI/Fonts/NunitoSans SDF.asset");
            if (nunito == null || TMP_Settings.instance == null) { Debug.LogError("[Fonts] cannot set TMP default font"); return; }

            var so = new SerializedObject(TMP_Settings.instance);
            so.FindProperty("m_defaultFontAsset").objectReferenceValue = nunito;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(TMP_Settings.instance);
            Debug.Log("[Fonts] TMP default font -> NunitoSans SDF");
        }

        [MenuItem("AQ/Setup/Apply Theme Fonts To Open Scene")]
        public static void SweepScene()
        {
            var nunito = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/App/UI/Fonts/NunitoSans SDF.asset");
            if (nunito == null) { Debug.LogError("[Fonts] NunitoSans SDF missing — run Build first"); return; }

            int swapped = 0;
            foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (tmp.font != null && !tmp.font.name.StartsWith("LiberationSans")) continue;
                Undo.RecordObject(tmp, "Theme font sweep");
                tmp.font = nunito;
                EditorUtility.SetDirty(tmp);
                swapped++;
            }
            if (swapped > 0)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"[Fonts] scene sweep: {swapped} TMP texts -> NunitoSans SDF");
        }

        static void Create(string ttfPath, string assetPath)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) { Debug.LogError("[Fonts] TTF not found/imported: " + ttfPath); return; }

            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) { Debug.Log("[Fonts] exists, skipping: " + assetPath); return; }

            var fa = TMP_FontAsset.CreateFontAsset(font, 90, 9,
                GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic);
            if (fa == null) { Debug.LogError("[Fonts] CreateFontAsset failed for " + ttfPath); return; }

            var dir = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(fa, assetPath);
            if (fa.atlasTextures != null)
                foreach (var t in fa.atlasTextures)
                    if (t != null) { t.name = fa.name + " Atlas"; AssetDatabase.AddObjectToAsset(t, fa); }
            if (fa.material != null) { fa.material.name = fa.name + " Material"; AssetDatabase.AddObjectToAsset(fa.material, fa); }
            EditorUtility.SetDirty(fa);
        }
    }
}
