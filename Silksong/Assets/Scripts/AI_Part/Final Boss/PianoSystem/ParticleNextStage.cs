using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleNextStage : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject nextParticleSystem;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleSystemStopped()
    {
        this.gameObject.SetActive(false);
        nextParticleSystem.SetActive(true);
    }
}
