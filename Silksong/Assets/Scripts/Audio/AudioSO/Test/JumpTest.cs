using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTest : MonoBehaviour
{
    public PlayerInput PInput;

    private bool jumpKeyDown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (PInput.Jump.Down && !jumpKeyDown)
        {
            jumpKeyDown = true;
            GetComponent<AudioCue>().PlayAudioCue();
        }
        if (PInput.Jump.Up)
        {
            jumpKeyDown = false;
        }
    }
}
