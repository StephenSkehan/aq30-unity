using UnityEngine;

/// <summary>
/// Ensures there is at least one active AudioListener in the scene.
/// If none exist at boot, creates a lightweight fallback listener
/// named "__AQ_FallbackAudioListener__" and marks it DontDestroyOnLoad.
/// If other listeners appear later, your Janitor can disable extras.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnsureAudioListener : MonoBehaviour
{
    [Tooltip("Name for the auto-created fallback listener.")]
    public string fallbackName = "__AQ_FallbackAudioListener__";

    void Awake()
    {
        // New API (Unity 6): count active listeners without using deprecated FindObjectOfType.
        var listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (listeners != null && listeners.Length > 0)
            return; // already have one somewhere

        // Create a tiny fallback so UI audio works even in UI-only scenes.
        var go = new GameObject(string.IsNullOrEmpty(fallbackName) ? "__AQ_FallbackAudioListener__" : fallbackName);
        go.transform.SetParent(null, worldPositionStays: false);
        go.AddComponent<AudioListener>();
        DontDestroyOnLoad(go);
#if UNITY_EDITOR
        Debug.Log("[Audio] Created fallback AudioListener: " + go.name);
#endif
    }
}
