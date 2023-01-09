#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif

using System;
using UnityEngine;

public class DemoEffects : MonoBehaviour
{
    public GameObject[] Effects;

    private int _num;

    private void Start()
    {
        UpdateEffects();
    }

    public void ShowNextEffect()
    {
        _num++;
        UpdateEffects();
    }

    public void ShowPrevEffect()
    {
        _num--;
        UpdateEffects();
    }

    private void Update()
    {
        #if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.X))
            {
                ShowNextEffect();
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                ShowPrevEffect();
            }
        #elif USE_INPUT_SYSTEM
            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                ShowNextEffect();
            }
            else if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                ShowPrevEffect();
            }
        #endif
    }

    private void UpdateEffects()
    {
        if (_num >= Effects.Length)
            _num = 0;
        else if (_num < 0)
            _num = Effects.Length - 1;

        foreach (var effect in Effects)
            effect.SetActive(false);
        
        Effects[_num].SetActive(true);
    }
}
