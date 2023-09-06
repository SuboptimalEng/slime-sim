using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SlimeSimulation))]
public class SlimeSimulationEditor : Editor
{
    SlimeSimulation slimeSimulation;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if (check.changed)
            {
                slimeSimulation.ResetAgents();
            }
        }
    }

    void OnEnable()
    {
        slimeSimulation = target as SlimeSimulation;
    }
}
