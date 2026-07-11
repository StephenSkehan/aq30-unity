using System;
using UnityEngine;

namespace AQ.App.Audio
{
    public static class AudioSettingsService
    {
        private const string kMusic    = "aq_vol_music";
        private const string kDialogue = "aq_vol_dialogue";
        private const string kSFX      = "aq_vol_sfx";

        public static event Action<float> MusicVolumeChanged;
        public static event Action<float> DialogueVolumeChanged;
        public static event Action<float> SFXVolumeChanged;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(kMusic, 1f);
            set { PlayerPrefs.SetFloat(kMusic, Mathf.Clamp01(value)); PlayerPrefs.Save(); MusicVolumeChanged?.Invoke(Mathf.Clamp01(value)); }
        }

        public static float DialogueVolume
        {
            get => PlayerPrefs.GetFloat(kDialogue, 1f);
            set { PlayerPrefs.SetFloat(kDialogue, Mathf.Clamp01(value)); PlayerPrefs.Save(); DialogueVolumeChanged?.Invoke(Mathf.Clamp01(value)); }
        }

        public static float SFXVolume
        {
            get => PlayerPrefs.GetFloat(kSFX, 1f);
            set { PlayerPrefs.SetFloat(kSFX, Mathf.Clamp01(value)); PlayerPrefs.Save(); SFXVolumeChanged?.Invoke(Mathf.Clamp01(value)); }
        }
    }
}
