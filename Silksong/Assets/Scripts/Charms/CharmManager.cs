using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharmManager : MonoBehaviour
{
    [SerializeField]
    private CharmListSO charmListSO = default;

    public PlayerCharacter playerCharacter;
    public PlayerController playerController;



    // Start is called before the first frame update
    void Awake()
    {
        playerController = PlayerController.Instance;
        playerCharacter = PlayerController.Instance.GetComponent<PlayerCharacter>();

        charmListSO.InitRef();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
