using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticleConfig 
{
    [SerializeField]
    [OnValueChanged("Modified")]
    public Vector3 relativePosition;
    [OnValueChanged("Modified")]
    public Quaternion relativeRotation;
    [HideInInspector]
    [NonSerialized]
    public LavenderClip owner;
    public void Modified()
    {
        if (owner != null)
        {
            owner.ModifyParticle();
        }
    }
}
