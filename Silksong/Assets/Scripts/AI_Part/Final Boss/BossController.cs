using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject piano;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PianoGenerate()
    {
        piano = GameObject.Find("Piano");
        piano.GetComponent<Piano>().Generate("101");
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
}
