using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SonicBloom.Koreo;


public class MusicTrigger : MonoBehaviour
{
    [EventID]
    public string eventId;
    private ShootSystem shootSystem;
    // Start is called before the first frame update
    void Start()
    {
        Koreographer.Instance.RegisterForEvents(eventId, onRhythmTrigger);
        shootSystem = this.GetComponent<ShootSystem>();
    }

    void onRhythmTrigger(KoreographyEvent evt)
    {
        shootSystem.Shoot("normal");
    }
}
