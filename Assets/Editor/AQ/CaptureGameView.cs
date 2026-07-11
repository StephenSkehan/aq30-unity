using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AQ.EditorTools
{
    public static class CaptureGameView
    {
        [MenuItem("AQ/Dev/Capture Game View Screenshot")]
        public static void Capture()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Capture] enter play mode first — Game view capture needs a rendering loop.");
                return;
            }
            ForcePortraitGameView();
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"gameview_{System.DateTime.Now:HHmmss}.png");
            ScreenCapture.CaptureScreenshot(path, 1);
            Debug.Log("[Capture] queued -> " + path);
        }

        [MenuItem("AQ/Dev/Exit Play Mode")]
        public static void ExitPlay()
        {
            EditorApplication.isPlaying = false;
        }

        [MenuItem("AQ/Dev/Enter Play Mode (no reset)")]
        public static void EnterPlay()
        {
            EditorApplication.isPlaying = true;
        }

        /// <summary>
        /// Select a fixed 1080x1920 size on the Game view (reflection — the
        /// GameView APIs are internal) so captures are consistent regardless
        /// of how the window happens to be docked.
        /// </summary>
        static void ForcePortraitGameView()
        {
            try
            {
                var asm = typeof(EditorWindow).Assembly;
                var gameViewType = asm.GetType("UnityEditor.GameView");
                var gameView = EditorWindow.GetWindow(gameViewType, false, null, true);

                var sizesType    = asm.GetType("UnityEditor.GameViewSizes");
                var singleType   = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var sizes        = singleType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                var group        = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] {
                                       sizesType.GetProperty("currentGroupType").GetValue(sizes) });

                var groupType  = group.GetType();
                int builtin    = (int)groupType.GetMethod("GetBuiltinCount").Invoke(group, null);
                int custom     = (int)groupType.GetMethod("GetCustomCount").Invoke(group, null);
                int foundIndex = -1;
                for (int i = 0; i < builtin + custom; i++)
                {
                    var size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                    int w = (int)size.GetType().GetProperty("width").GetValue(size);
                    int h = (int)size.GetType().GetProperty("height").GetValue(size);
                    if (w == 1080 && h == 1920) { foundIndex = i; break; }
                }

                if (foundIndex < 0)
                {
                    var sizeType     = asm.GetType("UnityEditor.GameViewSize");
                    var sizeTypeEnum = asm.GetType("UnityEditor.GameViewSizeType");
                    var ctor  = sizeType.GetConstructor(new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
                    var size  = ctor.Invoke(new object[] { 1, 1080, 1920, "AQ Portrait" }); // 1 = FixedResolution
                    groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                    foundIndex = builtin + custom;
                }

                gameViewType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SetValue(gameView, foundIndex);
                // Portrait-shaped window so the capture isn't letterboxed to a slice.
                gameView.position = new Rect(80f, 40f, 560f, 1030f);
                gameView.Repaint();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Capture] could not force game view size: " + e.Message);
            }
        }
    }
}
