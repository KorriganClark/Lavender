using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{

    public Transform Target
    {
        set
        {
            m_target = value;
        }
    }
    [SerializeField] Transform m_target;
    [SerializeField] CameraMove m_thridCamera;

    //滑动的偏移转换为角度偏移的系数
    const float DRAG_TO_ANGLE = 0.5f;
    Vector2 m_previousPressPosition;
    float m_angleX, m_angleY;

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_previousPressPosition = eventData.position;
        m_thridCamera.StartRotate();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //滑动屏幕，人物和摄像机一起旋转（人物只转Y轴，即左右旋转）
        m_angleX = (eventData.position.x - m_previousPressPosition.x) * DRAG_TO_ANGLE;
        m_angleY = (eventData.position.y - m_previousPressPosition.y) * DRAG_TO_ANGLE;
        m_thridCamera.Rotate(-m_angleX, -m_angleY);
        m_target.Rotate(new Vector3(0, m_angleX, 0));
        m_previousPressPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        m_thridCamera.EndRotate();
    }
}
