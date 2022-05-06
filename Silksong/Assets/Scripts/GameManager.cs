using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GameManager>();

            if (instance != null)
                return instance;

            GameObject sceneControllerGameObject = new GameObject("GameManager");
            instance = sceneControllerGameObject.AddComponent<GameManager>();

            return instance;
        }
    }//���� 

    protected static GameManager instance;

    [SerializeField]
    private GameObject player;

    public AudioManager audioManager;

    public GameObject gamingUI;

    public GameObject mapPack;

    public SaveSystem saveSystem;


    void Awake()
    {

        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        GameInitialize();

        //���´��������ҴӲ˵�������Ϸ�����ĳ�ʼ������ʱʹ��

        gamingUI = Instantiate(gamingUI);
        DontDestroyOnLoad(gamingUI);

        creatPlayer();
        GameObjectTeleporter.Instance.playerEnterScene(SceneEntrance.EntranceTag.A);

        mapPack = Instantiate(mapPack);
        DontDestroyOnLoad(mapPack);

    }

    /// <summary>
    /// ������Ϸ����ʱ�������
    /// </summary>
    public void creatPlayer()
    {
        player= Instantiate(player.gameObject);     
    }

    public void GameInitialize()
    {
        Application.targetFrameRate = 120;
        audioManager = Instantiate(audioManager);
    }
}
