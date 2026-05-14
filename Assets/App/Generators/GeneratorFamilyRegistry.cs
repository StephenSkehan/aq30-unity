using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace AQ.App.Generators
{
    /// <summary>
    /// Tracks per-generatorTypeId sub-gen lock state.
    /// Locked means any generator of that type has reached maxGeneratorTier —
    /// sub-gen drops are suppressed for all generators of that type until reset.
    /// </summary>
    public static class GeneratorFamilyRegistry
    {
        private static readonly HashSet<string> _locked = new();

        private static string FilePath
            => System.IO.Path.Combine(Application.persistentDataPath, "generator_registry.json");

        public static bool IsSubGenLocked(string typeId)
            => !string.IsNullOrEmpty(typeId) && _locked.Contains(typeId);

        public static void SetSubGenLocked(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return;
            if (_locked.Add(typeId)) Save();
        }

        public static void Clear()
        {
            _locked.Clear();
            Save();
        }

        public static void Load()
        {
            _locked.Clear();
            var p = FilePath;
            if (!File.Exists(p)) return;
            try
            {
                var dto = JsonUtility.FromJson<DTO>(File.ReadAllText(p, Encoding.UTF8));
                if (dto?.locked != null)
                    foreach (var t in dto.locked)
                        _locked.Add(t);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GeneratorRegistry] load failed: {ex.Message}");
            }
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(new DTO { locked = new List<string>(_locked) }), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GeneratorRegistry] save failed: {ex.Message}");
            }
        }

        [Serializable]
        private class DTO { public List<string> locked; }
    }
}
