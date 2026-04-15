using UnityEngine;
using UnityEditor;

public class TestPortraitAnimator : EditorWindow
{
    [MenuItem("Tools/Test Portrait Animator")]
    static void ShowWindow()
    {
        GetWindow<TestPortraitAnimator>("Portrait Test");
    }

    void OnGUI()
    {
        GUILayout.Label("Test Portrait Emotions", EditorStyles.boldLabel);

        var animator = Selection.activeGameObject?.GetComponent<Animator>();
        if (animator == null)
        {
            GUILayout.Label("Select Portrait GameObject in scene");
            return;
        }

        if (GUILayout.Button("Neutral")) animator.SetTrigger("Neutral");
        if (GUILayout.Button("Happy")) animator.SetTrigger("Happy");
        if (GUILayout.Button("Sad")) animator.SetTrigger("Sad");
        if (GUILayout.Button("Angry")) animator.SetTrigger("Angry");
        if (GUILayout.Button("Surprised")) animator.SetTrigger("Surprised");
        if (GUILayout.Button("Worried")) animator.SetTrigger("Worried");
        if (GUILayout.Button("Confused")) animator.SetTrigger("Confused");
    }
}