using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject piano;
    private GameObject clarinet;
    private GameObject drum;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PianoGenerate()
    {
        clarinet = GameObject.Find("Piano");
        clarinet.GetComponent<Piano>().Generate("10010");
    }

    public void PianoAttack()
    {
        piano = GameObject.Find("Piano");
        piano.GetComponent<Piano>().Attack();
    }

    public void PianoAttackEnd()
    {
        piano = GameObject.Find("Piano");
        piano.GetComponent<Piano>().End();
    }

    public void ClarinetGenerate()
    {
        clarinet = GameObject.Find("Clarinet");
        clarinet.GetComponent<Clarinet>().Generate();
    }

    public void ClarinetAttack()
    {
        clarinet = GameObject.Find("Clarinet");
        clarinet.GetComponent<Clarinet>().Attack();
    }

    public void ClarinetAttackEnd()
    {
        clarinet = GameObject.Find("Clarinet");
        clarinet.GetComponent<Clarinet>().End();
    }

    public void DrumGenerate()
    {
        drum = GameObject.Find("Drum");
        drum.GetComponent<Drum>().Generate();
    }
}
