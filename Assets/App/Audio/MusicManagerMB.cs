using UnityEngine;
using AQ.App.Audio;

namespace AQ.App
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManagerMB : MonoBehaviour
    {
        private AudioSource _source;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.volume = AudioSettingsService.MusicVolume;
        }

        void OnEnable()  => AudioSettingsService.MusicVolumeChanged += OnMusicVolumeChanged;
        void OnDisable() => AudioSettingsService.MusicVolumeChanged -= OnMusicVolumeChanged;

        private void OnMusicVolumeChanged(float v) => _source.volume = v;

        // Called by DialogueRunner to duck/restore, respecting the user's volume setting.
        public void SetDuckedVolume(float normalisedDuck) =>
            _source.volume = AudioSettingsService.MusicVolume * normalisedDuck;

        public void RestoreVolume() =>
            _source.volume = AudioSettingsService.MusicVolume;
    }
}
