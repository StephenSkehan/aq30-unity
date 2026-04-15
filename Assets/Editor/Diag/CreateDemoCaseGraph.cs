using UnityEngine;
using UnityEditor;
using AQ.App;

public class CreateDemoCaseGraph : EditorWindow
{
    [MenuItem("Tools/Create Demo CaseGraph")]
    static void CreateDemo()
    {
        Debug.Log("=== CREATING DEMO CASEGRAPH ===");

        // Create the CaseGraph ScriptableObject
        CaseGraph graph = ScriptableObject.CreateInstance<CaseGraph>();
        
        graph.startId = "INTRO";
        
        // Create demo nodes
        graph.nodes = new CaseGraph.Node[]
        {
            // Node 0: INTRO
            new CaseGraph.Node
            {
                id = "INTRO",
                speaker = "Ally Quinn",
                line = "Welcome to the dialogue system demo! I'm Ally, your detective partner.",
                emotion = CaseGraph.EmotionType.Happy,
                nextId = "FEATURES"
            },
            
            // Node 1: FEATURES
            new CaseGraph.Node
            {
                id = "FEATURES",
                speaker = "Ally Quinn",
                line = "Let me show you what this system can do. Click to continue...",
                emotion = CaseGraph.EmotionType.Neutral,
                nextId = "TYPEWRITER"
            },
            
            // Node 2: TYPEWRITER
            new CaseGraph.Node
            {
                id = "TYPEWRITER",
                speaker = "Ally Quinn",
                line = "Notice the typewriter effect? The text appears character by character. Pretty cool, right?",
                emotion = CaseGraph.EmotionType.Happy,
                nextId = "CHOICES_INTRO"
            },
            
            // Node 3: CHOICES_INTRO
            new CaseGraph.Node
            {
                id = "CHOICES_INTRO",
                speaker = "Ally Quinn",
                line = "Now let's try something interactive. What would you like to see?",
                emotion = CaseGraph.EmotionType.Neutral,
                choices = new CaseGraph.Choice[]
                {
                    new CaseGraph.Choice
                    {
                        text = "Tell me about portraits",
                        nextId = "PORTRAITS"
                    },
                    new CaseGraph.Choice
                    {
                        text = "Show me branching dialogue",
                        nextId = "BRANCHING"
                    },
                    new CaseGraph.Choice
                    {
                        text = "I'm ready to finish",
                        nextId = "FINISH"
                    }
                }
            },
            
            // Node 4: PORTRAITS
            new CaseGraph.Node
            {
                id = "PORTRAITS",
                speaker = "Ally Quinn",
                line = "The system supports character portraits with different emotions. When you add portrait sprites, they'll show up here on the left!",
                emotion = CaseGraph.EmotionType.Happy,
                nextId = "BACK_TO_MENU"
            },
            
            // Node 5: BRANCHING
            new CaseGraph.Node
            {
                id = "BRANCHING",
                speaker = "Ally Quinn",
                line = "You can create complex branching narratives with choices, conditions, and flags. The system tracks what the player has seen.",
                emotion = CaseGraph.EmotionType.Neutral,
                setsFlag = "learned_about_branching",
                nextId = "BACK_TO_MENU"
            },
            
            // Node 6: BACK_TO_MENU
            new CaseGraph.Node
            {
                id = "BACK_TO_MENU",
                speaker = "Ally Quinn",
                line = "Want to see something else?",
                emotion = CaseGraph.EmotionType.Neutral,
                nextId = "CHOICES_INTRO"
            },
            
            // Node 7: FINISH
            new CaseGraph.Node
            {
                id = "FINISH",
                speaker = "Ally Quinn",
                line = "Thanks for checking out the dialogue system! Now you're ready to create your own detective story.",
                emotion = CaseGraph.EmotionType.Happy,
                nextId = "" // End of dialogue
            }
        };

        // Save the asset
        string path = "Assets/Content/Demo/DemoDialogue.asset";
        string directory = System.IO.Path.GetDirectoryName(path);
        
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        AssetDatabase.CreateAsset(graph, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Created CaseGraph at: {path}");

        // Wire it to DialogueRunner
        GameObject runner = GameObject.Find("DialogueRunner");
        if (runner != null)
        {
            DialogueRunner runnerComponent = runner.GetComponent<DialogueRunner>();
            if (runnerComponent != null)
            {
                runnerComponent.Graph = graph;
                EditorUtility.SetDirty(runner);
                Debug.Log("✅ Wired CaseGraph to DialogueRunner");
            }
        }

        // Ping the asset
        EditorGUIUtility.PingObject(graph);

        Debug.Log("=== COMPLETE ===");

        EditorUtility.DisplayDialog("Demo Content Created!", 
            "DemoDialogue.asset created!\n\n" +
            "Content includes:\n" +
            "✅ Welcome dialogue\n" +
            "✅ Interactive choices\n" +
            "✅ Branching paths\n" +
            "✅ Multiple characters\n\n" +
            "The CaseGraph has been wired to DialogueRunner.\n\n" +
            "Ready to test! Click the green START DIALOGUE button in Game view!", 
            "OK");
    }
}