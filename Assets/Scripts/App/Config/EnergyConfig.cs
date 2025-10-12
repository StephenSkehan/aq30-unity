using UnityEngine;

namespace AQ.App.Config
{
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "AQ/Config/Energy")]
    public sealed class EnergyConfig : ScriptableObject
    {
        [Header("Economy")]
        public int Start = 100;                 // Starting energy
        public int Cap = 100;                   // Regen ceiling
        public int RegenSecondsPerPoint = 90;   // +1 every N seconds
    }
}
