using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
/// <summary>
/// ������Ϸ�����ڳ����ڵĴ��� ����
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

    public Vector3 playerRebornPoint;//������µ�������
    public bool Transitioning;

    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera mSecondVirtualCamera;
    public CinemachineVirtualCamera mThirdVirtualCamera;

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
        Teleport(PlayerInput.Instance.gameObject,Instance.playerRebornPoint);
    }
    public void playerEnterSceneEntance(SceneEntrance.EntranceTag entranceTag,Vector3 relativePos)
    {
        SceneEntrance entrance = SceneEntrance.GetDestination(entranceTag);
        if (entrance == null)//�ó���û����� ��������ҵ���Ϸ���� 
        {
            return;
        }
        Vector3 enterPos = entrance.transform.position;
        enterPos += relativePos; 

        //playerInput.transform.localScale = new Vector3();��ɫ���� ��δ����
        playerRebornPoint = enterPos;

        CameraController.Instance.AfterChangeScene();
        virtualCamera = CameraController.Instance.mMainVirtualCamera;
        if (virtualCamera && PlayerInput.Instance)
            virtualCamera.Follow = PlayerInput.Instance.transform;
        mSecondVirtualCamera = CameraController.Instance.mSecondVirtualCamera;
        if (mSecondVirtualCamera)
            mSecondVirtualCamera.Follow = PlayerInput.Instance.transform;
        mThirdVirtualCamera = CameraController.Instance.mThirdVirtualCamera;
        if (mThirdVirtualCamera)
            mThirdVirtualCamera.Follow = PlayerInput.Instance.transform;
        GameManager.Instance.audioManager.setMonstersDefaultHittedAudio();

        Teleport(PlayerInput.Instance.gameObject, enterPos);
    }
    public void playerEnterSceneFromTransitionPoint(SceneTransitionPoint transitionPoint)//����ҽ����³���ʱ���ø÷���
    {
        Vector3 enterPos = Vector3.zero;
        if(transitionPoint is  SceneTransitionPointKeepPos keepPos)
        {
            enterPos += keepPos.PlayerRelativePos;
        }
        playerEnterSceneEntance(transitionPoint.entranceTag, enterPos);
    }
    public static void Teleport(GameObject transitioningGameObject, Vector3 destinationPosition)
    {
        Instance.StartCoroutine(Instance.Transition(transitioningGameObject, false, false, destinationPosition, false));
    }

    protected IEnumerator Transition(GameObject transitioningGameObject, bool releaseControl, bool resetInputValues, Vector3 destinationPosition, bool fade)
    {
        Transitioning = true;


        if (releaseControl)
        {
            PlayerAnimatorParamsMapping.SetControl(false);
        }

        /*  if (fade)
              yield return StartCoroutine(ScreenFader.FadeSceneOut());*///�������ɼ����ݲ����� ����yield return null�����ֹû�з���ֵ
        transitioningGameObject.transform.position = destinationPosition;
        yield return null;

   

       /* if (fade)
            yield return StartCoroutine(ScreenFader.FadeSceneIn());*/

        if (releaseControl)
        {
            PlayerAnimatorParamsMapping.SetControl(true);
        }

        Transitioning = false;
    }

}
