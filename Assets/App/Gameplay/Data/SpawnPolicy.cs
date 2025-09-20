using System.Collections.Generic;
using UnityEngine;

namespace AQ.Gameplay {
  [CreateAssetMenu(menuName = "AQ/Gameplay/SpawnPolicy", fileName = "SpawnPolicy")]
  public class SpawnPolicy : ScriptableObject {
    [Tooltip("IDs of items eligible to spawn initially / at refill ticks")]
    public List<string> startingBag = new List<string>();

    [Tooltip("Seconds between spawn ticks or cooldown gating (tunable)")]
    public float spawnCooldownSeconds = 2f;

    [Tooltip("Max simultaneous spawns per tick (tunable)")]
    public int maxPerTick = 1;
  }
}