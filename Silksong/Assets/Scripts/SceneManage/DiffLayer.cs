using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 相机移动时图片相对移动，制造景深效果。
/// </summary>使用方法：scene中对应景深层次的父物体挂载此脚本，其子物体统一按照该景深相对移动。
public class DiffLayer : MonoBehaviour
{
   [SerializeField,Tooltip("相对移动速率，近景为负数,远景为正数")]
    float multiplier = 0.0f;
   [SerializeField,Tooltip("只在水平方向相对移动")] 
    bool horizontalOnly = true;

    private Transform cameraTransform;

    [SerializeField,Tooltip("指定一个相机位置，此位置下物体没有相对移动,当相机偏离此位置时，物体开始相对移动")]
    Vector3 absCameraPos;

    private Vector3 startPos;

    void Start()
    {
        SetupStartPositions();
    }

    void SetupStartPositions()
    {
        cameraTransform = Camera.main.transform;
        startPos = transform.position;
    }

    void LateUpdate()
    {
        UpdateParallaxPosition();
    }

    void UpdateParallaxPosition()
    {
        var position = startPos;
        position.x += multiplier * (cameraTransform.position.x - absCameraPos.x);

        if (!horizontalOnly)
        {
            position.y += multiplier * (cameraTransform.position.y - absCameraPos.y);//目前相机-初始相机=相机的位移 相对运动 近快远慢
        }

        transform.position = position;//
    }

}
