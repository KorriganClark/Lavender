using Lavender;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TimerUpdate : MonoBehaviour
{
    private static float timer;
    static TimerUpdate()
    {
        EditorApplication.update += EditorUpdate;
        //Physics.autoSimulation = false;
    }

    void Update()
    {
        LavenderTimer.Instance.UpdateEvents();
    }

    private static void EditorUpdate()
    {
        LavenderTimer.Instance.UpdateEvents();
        
        timer += Time.deltaTime;
        Physics.autoSimulation = false;
        while (timer >= Time.fixedDeltaTime)
        {
            timer -= Time.fixedDeltaTime;
            Physics.Simulate(Time.fixedDeltaTime);
        }
    }
}
