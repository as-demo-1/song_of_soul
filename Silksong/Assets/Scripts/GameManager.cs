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
    }//单例

    protected static GameManager instance;

    [SerializeField]
    private GameObject player;

    public AudioManager audioManager;

    public GameObject gamingUI;

    public SceneEntrance.EntranceTag entranceTag;//Temporary use
    public GameObject mapPack;


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

        gamingUI = Instantiate(gamingUI);
        DontDestroyOnLoad(gamingUI);

        creatPlayer();
        GameObjectTeleporter.Instance.playerEnterScene(entranceTag);

        mapPack = Instantiate(mapPack);
        DontDestroyOnLoad(mapPack);

    }

    /// <summary>
    /// 进入游戏场景时生成玩家
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
