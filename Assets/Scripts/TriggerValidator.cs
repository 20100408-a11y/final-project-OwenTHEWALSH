using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class TriggerValidator : EditorWindow
{
    private bool autoFixAddRigidbody = true;
    [MenuItem("Window/Trigger Validator")]
    public static void ShowWindow() => GetWindow<TriggerValidator>("Trigger Validator");

    private void OnGUI()
    {
        GUILayout.Label("Trigger Validator", EditorStyles.boldLabel);
        GUILayout.Space(8);
        autoFixAddRigidbody = EditorGUILayout.Toggle("Auto-fix: add Rigidbody to Player", autoFixAddRigidbody);

        if (GUILayout.Button("Validate Scene", GUILayout.Height(30)))
            ValidateScene();
    }

    private void ValidateScene()
    {
        var problems = 0;

        // Find Player
        var player = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        if (player == null)
        {
            Debug.LogError("[TriggerValidator] No GameObject with tag 'Player' found.");
            problems++;
        }
        else
        {
            var pCol = player.GetComponent<Collider>();
            var pRb = player.GetComponent<Rigidbody>();
            if (pCol == null)
            {
                Debug.LogError($"[TriggerValidator] Player '{player.name}' has no Collider. Add a Collider (Box/Capsule/Sphere).");
                problems++;
            }
            if (pRb == null)
            {
                Debug.LogWarning($"[TriggerValidator] Player '{player.name}' has no Rigidbody.");
                if (autoFixAddRigidbody)
                {
                    Undo.AddComponent<Rigidbody>(player);
                    player.GetComponent<Rigidbody>().isKinematic = true;
                    Debug.Log($"[TriggerValidator] Added isKinematic Rigidbody to Player '{player.name}'.");
                }
            }
        }
    }
}
