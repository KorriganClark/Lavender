using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
    public Transform Target
    {
        set
        {
            m_target = value;
        }
    }
    [SerializeField] Transform m_target;
    //相机与人物距离
    [SerializeField] float m_distance = 5;
    //初始化的偏移角度，以人物的(0,0,-1)为基准
    [SerializeField] float m_offsetAngleX = 0;
    [SerializeField] float m_offsetAngleY = 45;

    //相机与人物的坐标的偏移量
    Vector3 m_offsetVector;
    //纪录偏移角度用于复原
    float m_recordAngleX;
    float m_recordAngleY;
    //相机是否在旋转，旋转中需要一直重新计算 m_offsetVector
    bool m_isRotateing = false;

    //弧度，用于Mathf.Sin，Mathf.Cos的计算
    const float ANGLE_CONVERTER = Mathf.PI / 180;

    //相机上下的最大最小角度
    const float MAX_ANGLE_Y = 80;
    const float MIN_ANGLE_Y = 10;

    Transform m_trans;
    public Transform mineTransform
    {
        get
        {
            if (m_trans == null)
            {
                m_trans = this.transform;
            }
            return m_trans;
        }
    }

    GameObject m_go;
    public GameObject mineGameObject
    {
        get
        {
            if (m_go == null)
            {
                m_go = this.gameObject;
            }
            return m_go;
        }
    }

    void Start()
    {
        //m_target = GetComponentInParent<Transform>();
        CalculateOffset();
    }

    void LateUpdate()
    {
        //相机坐标 = 人物坐标 + 偏移坐标
        mineTransform.position = m_target.position + m_offsetVector;
        mineTransform.LookAt(m_target);
    }

    void Update()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        Rotate(-x, -y);
        //if (m_isRotateing)
        {
            CalculateOffset();
        }
    }

    //计算偏移，可以想象成在一个球面转，m_distance为半径，m_offsetAngleY决定了相机的高度y
    //高度确定后，就是在一个圆面上转，根据m_offsetAngleX计算出x与z
    void CalculateOffset()
    {
        m_offsetVector.y = m_distance * Mathf.Sin(m_offsetAngleY * ANGLE_CONVERTER);
        float newRadius = m_distance * Mathf.Cos(m_offsetAngleY * ANGLE_CONVERTER);
        m_offsetVector.x = newRadius * Mathf.Sin(m_offsetAngleX * ANGLE_CONVERTER);
        m_offsetVector.z = -newRadius * Mathf.Cos(m_offsetAngleX * ANGLE_CONVERTER);
    }

    //开始旋转，纪录当前偏移角度，用于复原
    public void StartRotate()
    {
        m_isRotateing = true;

        m_recordAngleX = m_offsetAngleX;
        m_recordAngleY = m_offsetAngleY;
    }

    //旋转，修改偏移角度的值，屏幕左右滑动即修改m_offsetAngleX，上下滑动修改m_offsetAngleY
    public void Rotate(float x, float y)
    {
        if (x != 0)
        {
            m_offsetAngleX += x;
        }
        if (y != 0)
        {
            m_offsetAngleY += y;
            m_offsetAngleY = m_offsetAngleY > MAX_ANGLE_Y ? MAX_ANGLE_Y : m_offsetAngleY;
            m_offsetAngleY = m_offsetAngleY < MIN_ANGLE_Y ? MIN_ANGLE_Y : m_offsetAngleY;
        }
    }

    //旋转结束，如需要复原镜头则，偏移角度还原并计算偏移坐标
    public void EndRotate(bool isNeedReset = false)
    {
        m_isRotateing = false;

        if (isNeedReset)
        {
            m_offsetAngleY = m_recordAngleY;
            m_offsetAngleX = m_recordAngleX;
            CalculateOffset();
        }
    }

}
