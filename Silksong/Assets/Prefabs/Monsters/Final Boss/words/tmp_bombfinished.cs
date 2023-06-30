using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tmp_bombfinished : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject word;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnParticleSystemStopped()
    {
        word.SetActive(false);
    }
}
