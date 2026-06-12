using UnityEngine;

namespace AQ.App.Audio
{
    [CreateAssetMenu(fileName = "UISfx", menuName = "AQ/UI SFX Config")]
    public sealed class UISfxSO : ScriptableObject
    {
        [Header("Lead")]
        public AudioClip leadFulfilled;

        [Header("Overflow")]
        public AudioClip overflowDrop;

        [Header("Volume")]
        [Range(0f, 1f)] public float volume = 0.8f;
    }
}
