using System.Collections.Generic;
using UnityEngine;

namespace AQ.Gameplay {
  [CreateAssetMenu(menuName = "AQ/Gameplay/RecipeBook", fileName = "RecipeBook")]
  public class RecipeBook : ScriptableObject {
    [System.Serializable]
    public struct Recipe {
      [Tooltip("ID of the resulting item (domain name or asset ID)")]
      public string resultId;

      [Tooltip("IDs of ingredients required for this recipe (order-insensitive)")]
      public List<string> ingredients;

      [Tooltip("Optional: minimum merge level / count for result")]
      public int minMergeLevel;
    }

    [Tooltip("All merge recipes known to the board")]
    public List<Recipe> recipes = new List<Recipe>();
  }
}