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
    public CinemachineVirtualCamera vcam;
    [SerializeField]
    private CinemachineVirtualCamera currentVcam;
    [SerializeField]
    private PolygonCollider2D boundary;
    // Start is called before the first frame update


    private void Awake()
    {
        
    }
    void Start()
    {
        currentVcam = vcam;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetFollow(Transform _transform)
    {
        vcam.Follow = _transform;
    }
    public void ChangeVcam(CinemachineVirtualCamera _vcam)
    {
        currentVcam.gameObject.SetActive(false);
        _vcam.gameObject.SetActive(true);
        _vcam.Follow = vcam.Follow;
        currentVcam = _vcam;
    }
    public void SetVcam(bool _option)
    {
        vcam.gameObject.SetActive(_option);
    }

}
