using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractiveUnitSO
{
    public string Content { get; }
    public Sprite Icon { get; }
    public Vector3 Position { get; set; }
    public bool IsFaceRight { get; }
}
