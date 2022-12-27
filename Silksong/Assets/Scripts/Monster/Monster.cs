using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int atk = 1;
    public int maxHP = 100;
    private Damager damager;
    private Damable damable;

    private void Awake()
    {
        damager = new Damager();
    }
}
