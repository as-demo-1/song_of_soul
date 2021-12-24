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


    void Awake()
    {

        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        GameInitialize();

        creatPlayer();
        GameObjectTeleporter.Instance.playerEnterScene(SceneEntrance.EntranceTag.A);
    }

    /// <summary>
    /// 进入游戏场景时生成玩家
    /// </summary>
    public void creatPlayer()
    {
        Instantiate(player.gameObject);
    }

    public void GameInitialize()
    {
        audioManager = Instantiate(audioManager);
    }
}
