using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code by chatGPT
public class RangeWithStepAttribute : PropertyAttribute
{
    public float min;
    public float max;
    public float step;

    public RangeWithStepAttribute(float min, float max, float step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }
}
