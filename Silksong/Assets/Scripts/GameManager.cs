using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


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
    }//单例

    protected static GameManager instance;

    [SerializeField]
    private GameObject player;

    public AudioManager audioManager;

    public GameObject gamingUI;

    public GameObject mapPack;

    public SaveSystem saveSystem;

    public GameObject gameMenu;

    [SerializeField]
    private CharmManager charmManager;

    [SerializeField]
    private CameraPack cameraPack;


    void Awake()
    {

        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        GameInitialize();

        //以下代码代表玩家从菜单进入游戏场景的初始化，临时使用

        // 用生成的物体替换对预制体的引用，未验证合理性
        gamingUI = Instantiate(gamingUI);
        gameMenu = Instantiate(gameMenu);
        DontDestroyOnLoad(gamingUI);
        DontDestroyOnLoad(gameMenu);

        creatPlayer();
        GameObjectTeleporter.Instance.playerEnterSceneEntance(SceneEntrance.EntranceTag.A,Vector3.zero);

        mapPack = Instantiate(mapPack);
        DontDestroyOnLoad(mapPack);
        uint bankid;
        AkSoundEngine.LoadBank("General",out bankid);

    }

    /// <summary>
    /// 进入游戏场景时生成玩家
    /// Generate player while enter game scene
    /// </summary>
    public void creatPlayer()
    {
        player= Instantiate(player.gameObject);

        // initialize charm manager, update player properties 护符效果引用初始化，需要获取一些玩家的属性
        charmManager = Instantiate(charmManager);

        // initialize camera following
        cameraPack = Camera.main.GetComponentInParent<CameraPack>();
        cameraPack.SetFollow(PlayerController.Instance.lookPos);
    }

    public void GameInitialize()
    {
        Application.targetFrameRate = 120;
        audioManager = Instantiate(audioManager);
    }
}