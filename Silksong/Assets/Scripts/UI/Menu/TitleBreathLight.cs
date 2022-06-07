using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleBreathLight : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color color;

    private void OnValidate()
    {
        GetComponent<Image>().color = color;
    }

    private void Start()
    {
        GetComponent<Image>().color = color;
    }
}
