using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Control camera function such as following player, swtich boundary, look up and down 
/// </summary>
public class CameraPack : MonoBehaviour
{
    

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    [SerializeField]
    private CinemachineConfiner confiner;
    [SerializeField]
    private PolygonCollider2D boundary;
    public PolygonCollider2D Boundary => boundary;
    // Start is called before the first frame update


    private void Awake()
    {

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetFollow(Transform _transform)
    {
        virtualCamera.Follow = _transform;
    }
    public void SetBoundary(PolygonCollider2D _boundary)
    {
        confiner.m_BoundingShape2D = _boundary;
    }
}
