using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCameraControl
{
    private GameObject targetCamera;
    private GameObject targetCharacter;
    private float distanceToChara = 5;
    private Quaternion currentRot = new Quaternion();
    private bool forcusToChara = true;
    private Vector3 currentPosition;
    private Vector3 currentVelocity = new Vector3(0, 0, 0);
    public GameObject TargetCamera
    {
        set
        {
            targetCamera = value;
            currentPosition = targetCamera.transform.position;
            currentRot.eulerAngles = new Vector3(0, 180, 0);
        }
    }
    public GameObject TargetCharacter
    {
        set
        {
            targetCharacter = value;
        }
    }
    public float DistanceToChara
    {
        set
        {
            distanceToChara = value;
        }
    }
    public bool Forcus
    {
        set
        {
            forcusToChara = value;
        }
        get
        {
            return forcusToChara;
        }
    }
    public bool IsValid
    {
        get
        {
            return targetCamera != null && targetCharacter != null;
        }
    }
    public void ReSetByChara()
    {
        if (!IsValid)
        {
            return;
        }
        Vector3 charaPosition = targetCharacter.transform.position + new Vector3(0, 1, 0);
        currentPosition = charaPosition - targetCamera.transform.forward * distanceToChara;
        //currentRot.eulerAngles = new Vector3(180, 180, 180) + targetCharacter.transform.rotation.eulerAngles;

    }
    public void UpdateCamera(Rect rect)
    {
        if (forcusToChara)
        {
            ReSetByChara();
        }
        //float smoothTime = 0.1f;
        if (rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                if (forcusToChara)
                {
                    distanceToChara += Event.current.delta.y / 10;
                }
                else
                {
                    currentPosition -= targetCamera.transform.forward * Event.current.delta.y / 10;
                }
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                currentRot.eulerAngles += new Vector3(Event.current.delta.y, Event.current.delta.x, 0) / 10;
                targetCamera.transform.SetPositionAndRotation(currentPosition, currentRot);
            }
        }
        var campos = targetCamera.transform.position;
        targetCamera.transform.SetPositionAndRotation(currentPosition, currentRot);
        //targetCamera.transform.SetPositionAndRotation(Vector3.SmoothDamp(targetCamera.transform.position, currentPosition, ref currentVelocity, smoothTime), currentRot);
    }
}
