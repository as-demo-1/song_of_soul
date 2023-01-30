using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 管理场景切换 单例
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<SceneController>();

            if (instance != null)
                return instance;

            GameObject sceneControllerGameObject = new GameObject("SceneManager");
            instance = sceneControllerGameObject.AddComponent<SceneController>();

            return instance;
        }
    }//单例

    protected static SceneController instance;

    public bool m_Transitioning;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }


    /// <summary>
    /// 从游戏场景出口到另一个场景
    /// </summary>
    /// <param name="transitionPoint"></param>
    public static void TransitionToScene(SceneTransitionPoint transitionPoint)
    {
        CameraController.Instance.BeforeChangeScene();
        Instance.StartCoroutine(Instance.Transition(transitionPoint.newSceneName, transitionPoint, transitionPoint.resetInputValuesOnTransition));
    }

   /* public static void TransitionToScene(string SceneName,bool resetInputValuesOnTransition)//从菜单到游戏场景用 暂不用
    {
        Instance.StartCoroutine(Instance.Transition(SceneName, SceneEntrance.EntranceTag.A, resetInputValuesOnTransition));
    }*/


    protected IEnumerator Transition(string newSceneName, SceneTransitionPoint destination, bool resetInputValues)
    {
        m_Transitioning = true;


        PlayerAnimatorParamsMapping.SetControl(false);

        //  yield return StartCoroutine(ScreenFader.FadeSceneOut(ScreenFader.FadeType.Loading));
        yield return SceneManager.LoadSceneAsync(newSceneName);//异步加载场景
        GameObjectTeleporter.Instance.playerEnterSceneFromTransitionPoint(destination);//玩家到场景入口 

        // yield return StartCoroutine(ScreenFader.FadeSceneIn());

        PlayerAnimatorParamsMapping.SetControl(true);

        m_Transitioning = false;
    }

}
