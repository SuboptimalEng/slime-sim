using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// code by chatGPT
[CustomEditor(typeof(SlimeSimulationV1))]
public class SlimeSimulationV1Editor : Editor
{
    SlimeSimulationV1 slimeSimulationV1;
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

            slimeSimulationV1.ResetAgents();
        }
        else
        {
            // If no changes were detected, still apply any modifications to the serialized object
            serializedObject.ApplyModifiedProperties();
        }

        if (GUILayout.Button("Randomize Agent Settings"))
        {
            slimeSimulationV1.RandomizeAgentSettings();
        }

        if (GUILayout.Button("Only Change Gradient"))
        {
            slimeSimulationV1.ChangeGradient();
        }

        if (GUILayout.Button("Randomize Gradient"))
        {
            slimeSimulationV1.RandomizeGradient();
        }

        // note: runs the method whenever any field changes
        // using (var check = new EditorGUI.ChangeCheckScope())
        // {
        //     base.OnInspectorGUI();
        //     if (check.changed)
        //     {
        //         slimeSimulation.ResetAgents();
        //     }
        // }
    }

    void OnEnable()
    {
        slimeSimulationV1 = target as SlimeSimulationV1;
        numOfAgents = serializedObject.FindProperty("numOfAgents");
        prevNumOfAgents = numOfAgents.floatValue;
    }
}
