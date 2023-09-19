 using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Exchange;
using Opsive.UltimateInventorySystem.SaveSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

            //GameObject sceneControllerGameObject = new GameObject("GameManager");
            //instance = sceneControllerGameObject.AddComponent<GameManager>();

            return instance;
        }
    }//单例

    protected static GameManager instance;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject mCamera;

    //public AudioManager audioManager;

    public GameObject gamingUI;

    public GameObject mapPack;

    //public GameObject eventSystem;

    public SaveSystem saveSystem;

    public GamingSaveSO gamingSave;

    public GameObject Loading_BlackScreen;
    
    public CurrencyOwner currencyOwner;
    public Inventory inventory;
    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        GameInitialize();

        Loading_BlackScreen = Instantiate(Loading_BlackScreen);
        DontDestroyOnLoad(Loading_BlackScreen);


        //以下代码代表玩家从菜单进入游戏场景的初始化，最终应通过开始游戏ui调用
        startGaming();
    }

    private void Start()
    {
        GameObjectTeleporter.Instance.playerEnterSceneEntance(SceneEntrance.EntranceTag.A, Vector3.zero);
    }

    public void startGaming()
    {
        // 加载存档
        saveSystem.Init();
        CreateCamera();
        
        ReloadInventory();
        ReloadDialogue();

        // 临时初始化UI
        UIManager.Instance.Show<UIPlayerStatus>();

        //before create the player, you need to load save data so the player can run init correctly  but at now we do not load save yet
        creatPlayer();

        

        //eventSystem = Instantiate(eventSystem);
        //DontDestroyOnLoad(eventSystem);
        uint bankid;
        AkSoundEngine.LoadBank("General", out bankid);
    }
    /// <summary>
    /// 进入游戏场景时生成玩家
    /// </summary>
    public void creatPlayer()
    {
        player = Instantiate(player.gameObject, transform.position, Quaternion.identity);
    }

    public void CreateCamera()
    {
        GameObject tempCam = GameObject.Find("TempCamera");
        if (tempCam != null)
        {
            GameObject.Destroy(tempCam);
        }
        GameObject cam = Instantiate(mCamera.gameObject);
        cam.name = "CameraPack";
        DontDestroyOnLoad(cam);
    }

    public void GameInitialize()
    {
        Application.targetFrameRate = 120;
        //audioManager = Instantiate(audioManager);
    }

    public void ReloadInventory()
    {
        //物品系统部分
        if (!SaveSystemManager.Saves.ContainsKey((int)saveSystem.SaveData.slotIndex))
        {
            SaveSystemManager.Save((int)saveSystem.SaveData.slotIndex, true);
        }
        SaveSystemManager.Load((int)saveSystem.SaveData.slotIndex);
        
        //currencyOwner = InventorySystemManager.GetInventoryIdentifier(saveSystem.SaveData.slotIndex).CurrencyOwner;
        //inventory = InventorySystemManager.GetInventoryIdentifier(saveSystem.SaveData.slotIndex).Inventory;
        //SaveSystemManager.Save((int)saveSystem.SaveData.slotIndex, true);
        //SaveSystemManager.Load((int)saveSystem.SaveData.slotIndex);
        //SaveSystemManager.Instance.PrintSaveFolderPath();
        //currencyOwner = InventorySystemManager.GetInventoryIdentifier(saveSystem.SaveData.slotIndex).CurrencyOwner;
        //inventory = InventorySystemManager.GetInventoryIdentifier(saveSystem.SaveData.slotIndex).Inventory;
    }

    public void ReloadDialogue()
    {
        PixelCrushers.SaveSystem.LoadFromSlot((int)saveSystem.SaveData.slotIndex);
        
    }
}