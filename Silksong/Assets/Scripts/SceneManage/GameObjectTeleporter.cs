using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/// <summary>
/// 负责游戏物体在场景内的传送 单例
/// </summary>
public class GameObjectTeleporter : MonoBehaviour
{
    public static GameObjectTeleporter Instance
    {
        get
        {
            
           // Debug.Log("get");
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GameObjectTeleporter>();

            if (instance != null)
                return instance;

            GameObject gameObjectTeleporter = new GameObject("GameObjectTeleporter");
            instance = gameObjectTeleporter.AddComponent<GameObjectTeleporter>();

            return instance;
        }
    }

    protected static GameObjectTeleporter instance;

    public Vector3 playerRebornPoint;//玩家最新的重生点
    public bool Transitioning;

    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera mSecondVirtualCamera;
    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

    }

    public static void playerReborn()
    {
        //Instance.playerInput.transform.localScale = new Vector3(1, 0, 0);
        Teleport(PlayerInput.Instance.gameObject,Instance.playerRebornPoint,true,true);
    }
    public void playerEnterSceneEntance(SceneEntrance.EntranceTag entranceTag,Vector3 relativePos,bool fade=false,bool releaseControl = false)
    {
        SceneEntrance entrance = SceneEntrance.GetDestination(entranceTag);
        if (entrance == null)//该场景没有入口 不是有玩家的游戏场景 
        {
            return;
        }
        Vector3 enterPos = entrance.transform.position;
        enterPos += relativePos; 

        //playerInput.transform.localScale = new Vector3();角色朝向 暂未考虑
        playerRebornPoint = enterPos;

        CameraController.Instance.AfterChangeScene();
        virtualCamera = CameraController.Instance.mMainVirtualCamera;
        if (virtualCamera && PlayerInput.Instance)
            virtualCamera.Follow = PlayerInput.Instance.transform;
        mSecondVirtualCamera = CameraController.Instance.mSecondVirtualCamera;
        if (mSecondVirtualCamera)
            mSecondVirtualCamera.Follow = PlayerInput.Instance.transform;

       // GameManager.Instance.audioManager.setMonstersDefaultHittedAudio();

       // SceneController.Instance.PreLoadScenes();

        Teleport(PlayerInput.Instance.gameObject, enterPos,fade,releaseControl);
    }
    public void playerEnterSceneFromTransitionPoint(SceneTransitionPoint transitionPoint)//在玩家进入新场景时调用该方法
    {
        Vector3 enterPos = Vector3.zero;
        if(transitionPoint is  SceneTransitionPointKeepPos keepPos)
        {
            enterPos += keepPos.PlayerRelativePos;
        }
        playerEnterSceneEntance(transitionPoint.entranceTag, enterPos);
    }
    public static void Teleport(GameObject transitioningGameObject, Vector3 destinationPosition,bool fade,bool releaseControl)
    {
        Instance.StartCoroutine(Instance.Transition(transitioningGameObject, destinationPosition,fade,releaseControl));
    }

    protected IEnumerator Transition(GameObject transitioningGameObject, Vector3 destinationPosition, bool fade, bool releaseControl, bool resetInputValues = false)
    {

        if (Transitioning) yield break;

        Transitioning = true;


        if (releaseControl)
        {
            PlayerAnimatorParamsMapping.SetControl(false);
        }

        if (fade)
            yield return StartCoroutine(ScreenFader.Instance.FadeSceneOut(ScreenFader.TeleportFadeOutTime));

        transitioningGameObject.transform.position = destinationPosition;

        if (fade)
            yield return StartCoroutine(ScreenFader.Instance.FadeSceneIn(ScreenFader.TeleportFadeInTime));

        if (releaseControl)
        {
            PlayerAnimatorParamsMapping.SetControl(true);
        }

        Transitioning = false;
    }

}
