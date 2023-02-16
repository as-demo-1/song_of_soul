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
    private string transitioningScene;

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
    private void Update()
    {
       // print(PlayerAnimatorParamsMapping.HaveControl());
    }

    protected IEnumerator Transition(string newSceneName, SceneTransitionPoint destination, bool resetInputValues)
    {
        if (m_Transitioning && newSceneName==transitioningScene)
        {
            print("transitioning to the same scene");
            yield break;
        }
        m_Transitioning = true;
        transitioningScene = newSceneName;
        //  print("to true");
        PlayerAnimatorParamsMapping.SetControl(false);
        stopPlayer();

        yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut(ScreenFader.SceneFadeOutTime));

        yield return SceneManager.LoadSceneAsync(newSceneName);//异步加载场景


        GameObjectTeleporter.Instance.playerEnterSceneFromTransitionPoint(destination);//玩家到场景入口 
        setPlayerAction();
        yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn(ScreenFader.SceneFadeInTime));

        PlayerAnimatorParamsMapping.SetControl(true);
        m_Transitioning = false;
        //print("to false");
    }

    private void stopPlayer()
    {
  
       /*if (PlayerController.Instance.playerAnimatorStatesControl.CurrentPlayerState==EPlayerState.Jump)
        {
            PlayerController.Instance.setRigidVelocity(Vector2.zero);
        }*/

    }

    private void setPlayerAction()
    {
        
        PlayerController playerController = PlayerController.Instance;

        if (playerController.playerAnimatorStatesControl.CurrentPlayerState == EPlayerState.Plunge) return;

        playerController.setRigidVelocity(Vector2.zero);
        if (playerController.playerToCat.IsCat)
        {
            playerController.PlayerAnimator.Play("Cat_Idle");
        }
        else
        {
            if(playerController.IsUnderWater)
            {
                playerController.PlayerAnimator.Play("WaterIdle");
            }
            else 
            {
                playerController.PlayerAnimator.Play("Idle");
            }
        }
        
    }

}
