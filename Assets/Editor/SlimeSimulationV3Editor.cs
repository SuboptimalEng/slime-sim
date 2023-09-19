using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// code by chatGPT
[CustomEditor(typeof(SlimeSimulationV3))]
public class SlimeSimulationV3Editor : Editor
{
    SlimeSimulationV3 slimeSimulationV3;
    SerializedProperty numOfAgents;
    float prevNumOfAgents;

    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Store the current value of the serialized property
        float currNumOfAgents = numOfAgents.floatValue;

        // Call the base class's OnInspectorGUI() to display the default inspector
        base.OnInspectorGUI();

        // Check if the value of the specific field has changed
        if (currNumOfAgents != prevNumOfAgents)
        {
            // The specific field has changed
            Debug.Log(
                "Field 'numOfAgents' changed from " + prevNumOfAgents + " to " + currNumOfAgents
            );

            // Update the previous value to reflect the new value
            prevNumOfAgents = currNumOfAgents;

            // Apply any modifications to the serialized object
            serializedObject.ApplyModifiedProperties();

            slimeSimulationV3.InitializeAgents();
        }
        else
        {
            // If no changes were detected, still apply any modifications to the serialized object
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Randomize Species Settings"))
        {
            slimeSimulationV3.RandomizeSpeciesSettings();
        }

        if (GUILayout.Button("Apply Settings"))
        {
            slimeSimulationV3.InitializeSpeciesSettings();
        }

        // Add a button to the Inspector
        if (GUILayout.Button("Randomize Gradient"))
        {
            // Perform your custom action when the button is clicked
            slimeSimulationV3.RandomizeGradient();
        }

        // note: runs the method whenever any field changes
        // using (var check = new EditorGUI.ChangeCheckScope())
        // {
        //     base.OnInspectorGUI();
        //     if (check.changed)
        //     {
        //         slimeSimulationV3.InitializeAgents();
        //     }
        // }
    }

    void OnEnable()
    {
        slimeSimulationV3 = target as SlimeSimulationV3;
        numOfAgents = serializedObject.FindProperty("numOfAgents");
        prevNumOfAgents = numOfAgents.floatValue;
    }
}
