using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Resloader
{
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
}
