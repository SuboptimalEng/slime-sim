using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// code by chatGPT
[CustomPropertyDrawer(typeof(RangeWithStepAttribute))]
public class RangeWithStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RangeWithStepAttribute rangeWithStep = attribute as RangeWithStepAttribute;
        float step = rangeWithStep.step;

        if (property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.BeginChangeCheck();

            float newValue = EditorGUI.Slider(
                position,
                label,
                property.floatValue,
                rangeWithStep.min,
                rangeWithStep.max
            );

            newValue = Mathf.Round(newValue / step) * step;
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = newValue;
            }
        }
        else
        {
            EditorGUI.LabelField(
                position,
                label.text,
                "Use RangeWithStep with float properties only."
            );
        }
    }
}
