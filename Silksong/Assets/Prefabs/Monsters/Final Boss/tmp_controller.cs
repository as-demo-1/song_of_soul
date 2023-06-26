using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tmp_controller : MonoBehaviour
{
    public GameObject words;
    public GameObject piano;
    public GameObject clarinet;
    public GameObject violin;
    public GameObject drum;
    void Start()
    {
        /*
        Invoke("Piano", 2f);
        Invoke("Clarinet", 8f);
        Invoke("ClarinetEnd", 12f);
        Invoke("Violin", 14f);
        Invoke("ViolinEnd", 20f);
        Invoke("Drum", 22f);
        */
        // words.GetComponent<Words>().Attack();
        // piano.GetComponent<Piano>().Generate();
        // clarinet.GetComponent<Clarinet>().Attack();
        // violin.GetComponent<Violin>().Attack();
        // drum.GetComponent<Drum>().Generate();
    }

    void Piano()
    {
        piano.GetComponent<Piano>().Attack();
    }

    void Clarinet()
    {
        clarinet.GetComponent<Clarinet>().Attack();
    }

    void ClarinetEnd()
    {
        clarinet.GetComponent<Clarinet>().End();
    }

    void Violin()
    {
        violin.GetComponent<Violin>().Attack();
    }

    void ViolinEnd()
    {
        violin.GetComponent<Violin>().End();
    }

    void Drum()
    {
        drum.GetComponent<Drum>().Generate();
    }
}
