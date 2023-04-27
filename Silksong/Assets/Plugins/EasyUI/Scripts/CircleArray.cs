using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum ArrayType
{
    // 等间距圆周阵列
    [Tooltip("等间距")]
    EQUAL,
    
    // 按角度阵列
    [Tooltip("角度间隔")]
    GAP,
}

/// <summary>
/// UI圆周阵列容器
/// </summary>
public class CircleArray : MonoBehaviour
{
    [SerializeField]
    private List<Transform> elements = new List<Transform>();


    public ArrayType mode = ArrayType.EQUAL;

    [Header("半径")]
    public float radius;
    
    [Header("角度模式下设置")]
    public float gapAngle;
    public float startAngle;
    
    [Header("方向")]
    public bool circleDirection;


    public bool withAnim;

    private void Awake()
    {
        UpdateTrasform(withAnim);
    }

    public void AddElement(Transform _element)
    {
        elements.Add(_element);
        _element.SetParent(transform);
        _element.localPosition = Vector3.zero;
        UpdateTrasform(withAnim);
    }

    float theta;
    public void UpdateTrasform(bool _anim = true)
    {
        if (mode == ArrayType.EQUAL)
        {
            gapAngle = 360.0f / elements.Count;
            startAngle = 0.0f;
        }
        
        for (int i = 0; i < elements.Count; i++)
        {
            theta = startAngle + i * gapAngle * (circleDirection ? 1.0f : -1.0f);
            if (!_anim)
            {
                elements[i].localPosition = new Vector3(
                    -radius * Mathf.Sin(theta * Mathf.PI / 180.0f),
                    radius * Mathf.Cos(theta * Mathf.PI / 180.0f),
                    0.0f);
            }
            else
            {
                elements[i].DOLocalMove(new Vector3(
                    -radius * Mathf.Sin(theta * Mathf.PI / 180.0f),
                    radius * Mathf.Cos(theta * Mathf.PI / 180.0f),
                    0.0f), 0.5f);
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position,radius);
    }
}
