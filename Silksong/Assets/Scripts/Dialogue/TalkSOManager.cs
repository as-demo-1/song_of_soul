using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkSOManager : MonoBehaviour
{
    public DialogContainerSO DialogueContainer;

    private static TalkSOManager _instance;
    public static TalkSOManager Instance => _instance;

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;
    }

}
