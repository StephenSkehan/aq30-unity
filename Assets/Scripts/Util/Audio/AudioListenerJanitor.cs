// Assets/Scripts/Util/Audio/AudioListenerJanitor.cs
using System.Collections;
using UnityEngine;

namespace AQ.App.Util.Audio
{
    /// <summary>
    /// Enforce exactly ONE enabled AudioListener at runtime.
    /// - Runs AFTER scene load and waits one frame so cameras exist.
    /// - Prefers Camera.main; adds a listener there if missing.
    /// - Disables all other listeners (incl. hidden/DDOL).
    /// - If no cameras/listeners exist, creates a DDOL fallback listener.
    /// </summary>
    public static class AudioListenerJanitor
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var host = new GameObject("__AQ_AudioJanitor__");
            host.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(host);
            host.AddComponent<Runner>();
        }

        private sealed class Runner : MonoBehaviour
        {
            IEnumerator Start()
            {
                // Wait one frame so the scene and cameras are alive.
                yield return null;
                Enforce();
                Destroy(gameObject);
            }

            static void Enforce()
            {
                // 1) Keeper preference: Camera.main
                AudioListener keeper = null;
                var main = Camera.main;
                if (main != null)
                {
                    keeper = main.GetComponent<AudioListener>();
                    if (keeper == null) keeper = main.gameObject.AddComponent<AudioListener>();
                    keeper.enabled = true;
                }

                // 2) Disable every other enabled listener
                var all = Resources.FindObjectsOfTypeAll<AudioListener>();
                int disabled = 0;
                foreach (var l in all)
                {
                    if (!l) continue;
                    if (l == keeper) continue;
                    if (l.enabled)
                    {
                        l.enabled = false;   // safer than Destroy on engine-owned objects
                        disabled++;
                    }
                }

                // 3) If there’s still no keeper, create a fallback in DDOL
                if (keeper == null)
                {
                    var go = new GameObject("__AQ_FallbackAudioListener__");
                    go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    Object.DontDestroyOnLoad(go);
                    keeper = go.AddComponent<AudioListener>();
                    keeper.enabled = true;
                }

                Debug.Log($"[AudioListenerJanitor] total={all.Length}, kept={keeper.gameObject.name}, disabled={disabled}");
            }
        }
    }
}
