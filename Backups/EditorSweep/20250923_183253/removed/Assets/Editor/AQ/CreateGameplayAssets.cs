using UnityEditor;
using UnityEngine;

namespace AQ.Tools {
  public static class CreateGameplayAssets {
    const string RecipePath = "Assets/Resources/App/Gameplay/RecipeBook.asset";
    const string SpawnPath  = "Assets/Resources/App/Gameplay/SpawnPolicy.asset";

    [MenuItem("AQ/Tools/Create Gameplay Assets (RecipeBook & SpawnPolicy)", priority = 2100)]
    public static void RunMenu() => Run();

    // Safe to call in batch: -executeMethod AQ.Tools.CreateGameplayAssets.Run
    public static void Run() {
      EnsureFolder("Assets/Resources/App/Gameplay");
      var recipeBook = LoadOrCreate<AQ.Gameplay.RecipeBook>(RecipePath);
      var spawnPol   = LoadOrCreate<AQ.Gameplay.SpawnPolicy>(SpawnPath);
      Debug.Log($"[CreateGameplayAssets] OK - RecipeBook at {AssetDatabase.GetAssetPath(recipeBook)}; SpawnPolicy at {AssetDatabase.GetAssetPath(spawnPol)}");

      // Addressables wiring intentionally omitted to satisfy stabilizer.
      AssetDatabase.SaveAssets();
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject {
      var asset = AssetDatabase.LoadAssetAtPath<T>(path);
      if (asset != null) return asset;
      asset = ScriptableObject.CreateInstance<T>();
      var parent = System.IO.Path.GetDirectoryName(path).Replace("\\","/");
      EnsureFolder(parent);
      AssetDatabase.CreateAsset(asset, path);
      return asset;
    }

    static void EnsureFolder(string folderPath) {
      var parts = folderPath.Split('/');
      var curr = "";
      foreach (var p in parts) {
        curr = string.IsNullOrEmpty(curr) ? p : $"{curr}/{p}";
        if (!AssetDatabase.IsValidFolder(curr)) {
          var parent = System.IO.Path.GetDirectoryName(curr).Replace("\\","/");
          var leaf   = System.IO.Path.GetFileName(curr);
          AssetDatabase.CreateFolder(parent, leaf);
        }
      }
    }
  }
}
