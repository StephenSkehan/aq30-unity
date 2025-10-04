// Assets/Scripts/Util/EnsureAudioListener.cs
using UnityEngine;

namespace AQ.App.Util
{
    /// <summary>
    /// Ensures an AudioListener exists at runtime so SFX can be heard.
    /// Prefers Camera.main; otherwise creates a hidden GO.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnsureAudioListener : MonoBehaviour
    {
        [Tooltip("If true, will create a hidden listener if none exists.")]
        public bool createIfMissing = true;

        void Awake()
        {
            if (FindObjectOfType<AudioListener>()) return;

            var cam = Camera.main;
            if (cam != null)
            {
                cam.gameObject.AddComponent<AudioListener>();
                return;
            }

            if (createIfMissing)
            {
                var go = new GameObject("__AudioListener__");
                go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
                go.AddComponent<AudioListener>();
                DontDestroyOnLoad(go);
            }
        }
    }
}
