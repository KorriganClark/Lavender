using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NGUICurveTest : MonoBehaviour
{
    public AnimationCurve animationCurve;
    public float time;
    private void Update()
    {
        //Debug.Log(animationCurve.Evaluate)
        Debug.Log(animationCurve.Evaluate(time));
    }
}
