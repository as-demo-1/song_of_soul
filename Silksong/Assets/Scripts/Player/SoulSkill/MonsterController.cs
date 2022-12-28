using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private Animator _animator;
    private Hittable _hittable;

    private void Awake()
    {
        _hittable = GetComponent<Hittable>();
    }

    private void Start()
    {
        
    }
}
