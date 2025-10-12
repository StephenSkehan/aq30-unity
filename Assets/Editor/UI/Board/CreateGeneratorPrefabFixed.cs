using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

/// <summary>
/// Creates a properly structured generator tile prefab matching slot structure
/// </summary>
public static class CreateGeneratorPrefabFixed
{
    [MenuItem("AQ/Board/Create Generator Prefab (Fixed Structure)")]
    private static void CreateGenerator()
    {
        // Load the generator sprite
        string spritePath = "Assets/Art/UI/Icons/Generators/corner_diner/corner_diner_t03_small_coffee_cart.png";
        Sprite generatorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

        if (generatorSprite == null)
        {
            Debug.LogError($"[CreateGenerator] Could not find generator sprite at: {spritePath}");
            EditorUtility.DisplayDialog("Error", $"Generator sprite not found at:\n{spritePath}", "OK");
            return;
        }

        // Create root GameObject with background Image
        GameObject generatorGO = new GameObject("GeneratorTile");

        // Add background Image to root
        Image bgImage = generatorGO.AddComponent<Image>();
        bgImage.color = new Color(0, 1, 1, 0.3f); // Cyan tint for generator

        // Set RectTransform size
        RectTransform rootRT = generatorGO.GetComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(100, 100);

        // Create "Icon" child GameObject
        GameObject iconChild = new GameObject("Icon");
        iconChild.transform.SetParent(generatorGO.transform, false);

        // Add Image component to icon child
        Image iconImage = iconChild.AddComponent<Image>();
        iconImage.sprite = generatorSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        // Set icon child to fill parent
        RectTransform iconRT = iconChild.GetComponent<RectTransform>();
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        // Add BoardTileView component using reflection
        Type boardTileViewType = FindTypeByName("BoardTileView");
        if (boardTileViewType != null)
        {
            Component tileView = generatorGO.AddComponent(boardTileViewType);

            // Assign references using reflection
            var iconImageField = boardTileViewType.GetField("IconImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bgImageField = boardTileViewType.GetField("BackgroundImage",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (iconImageField != null) iconImageField.SetValue(tileView, iconImage);
            if (bgImageField != null) bgImageField.SetValue(tileView, bgImage);

            Debug.Log("[CreateGenerator] Assigned Image references to BoardTileView");
        }

        // Create Prefabs folder if needed
        string prefabFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // Save as prefab
        string prefabPath = $"{prefabFolder}/GeneratorTile.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(generatorGO, prefabPath);

        Debug.Log($"[CreateGenerator] Created generator prefab at: {prefabPath}");
        Debug.Log($"  - Background Image: {bgImage.name}");
        Debug.Log($"  - Icon child: {iconChild.name}");
        Debug.Log($"  - Icon sprite: {generatorSprite.name}");

        // Assign to controller
        Type controllerType = FindTypeByName("MergeBoardController");
        var controller = FindFirstObjectByTypeInScene(controllerType) as Component;

        if (controller != null)
        {
            var prefabField = controllerType.GetField("GeneratorPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (prefabField != null)
            {
                prefabField.SetValue(controller, prefab);
                EditorUtility.SetDirty(controller);
                Debug.Log("[CreateGenerator] Assigned to MergeBoardController");
            }
        }

        // Clean up
        UnityEngine.Object.DestroyImmediate(generatorGO);

        EditorUtility.DisplayDialog("Success!",
            $"Generator prefab created with proper structure:\n" +
            $"- Root: GeneratorTile (with background Image)\n" +
            $"- Child: Icon (with generator sprite)\n" +
            $"- Sprite: {generatorSprite.name}\n\n" +
            $"Assigned to MergeBoardController", "OK");

        Selection.activeObject = prefab;
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == typeName) return type;
            }
        }
        return null;
    }

    // Renamed to avoid hiding UnityEngine.Object.FindFirstObjectByType
    private static UnityEngine.Object FindFirstObjectByTypeInScene(Type type)
    {
        var objects = Resources.FindObjectsOfTypeAll(type);
        foreach (var obj in objects)
        {
            if (obj is Component component && component.gameObject.scene.IsValid())
            {
                return obj;
            }
        }
        return null;
    }
}
