using System.IO;
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
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"gameview_{System.DateTime.Now:HHmmss}.png");
            ScreenCapture.CaptureScreenshot(path, 1);
            Debug.Log("[Capture] queued -> " + path);
        }
    }
}
