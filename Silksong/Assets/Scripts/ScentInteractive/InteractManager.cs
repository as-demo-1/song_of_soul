using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InteractManager
{
    public static void Interact()
    {
        DialogInteract.Instance.Interact();
        NormaInteract.Instance.Interact();
    }
}
