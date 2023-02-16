using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader:MonoSingleton<ScreenFader>
{
    public Image blackScreen;
    public const float SceneFadeOutTime = 0f;
    public const float SceneFadeInTime = 1f;
    public const float TeleportFadeOutTime = 0.5f;
    public const float TeleportFadeInTime = 0.5f;


    public IEnumerator FadeSceneOut(float time)
    {
        //print("out");
        blackScreen.gameObject.SetActive(true);
        blackScreen.CrossFadeAlphaFixed(1,time,false);
        yield return new WaitForSeconds(time);
    }

    public IEnumerator FadeSceneIn(float time)
    {
       // print("in");
        blackScreen.CrossFadeAlpha(0, time, false);
        yield return new WaitForSeconds(time);
        blackScreen.gameObject.SetActive(false);
    }

    public override void Init()
    {
        DontDestroyOnLoad(this);
        blackScreen = GameManager.Instance.Loading_BlackScreen.GetComponentInChildren<Image>();
        blackScreen.gameObject.SetActive(false);
    }
}
