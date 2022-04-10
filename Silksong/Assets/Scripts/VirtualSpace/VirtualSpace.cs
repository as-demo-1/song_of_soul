using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualSpace : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void TroggleRender(bool isShow)
    {
        _anim.SetBool("isShow", isShow);
    }
}
