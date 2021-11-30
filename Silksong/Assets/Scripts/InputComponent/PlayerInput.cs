using System.Collections.Generic;
using UnityEngine;
//Created by Unity from:
//https://assetstore.unity.com/packages/templates/tutorials/2d-game-kit-107098#description

public class PlayerInput : InputComponent
{
    public static PlayerInput Instance
    {
        get { return s_Instance; }
    }

    protected static PlayerInput s_Instance;


    public bool HaveControl { get { return m_HaveControl; } }

    private List<Button> buttons = new List<Button>();
    public InputButton sprint = new InputButton(KeyCode.LeftShift, XboxControllerButtons.LeftBumper, true);
    public InputButton Pick = new InputButton(KeyCode.F, XboxControllerButtons.Y, true);
    ////TODO:xbox button mapping
    public InputButton teleport = new InputButton(KeyCode.X, XboxControllerButtons.None, true);
    public InputButton jump = new InputButton(KeyCode.Space, XboxControllerButtons.A, true);
    public InputButton normalAttack = new InputButton(KeyCode.J, XboxControllerButtons.X, true);
    public InputAxis horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal, true);
    public InputAxis vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical, true);
    [HideInInspector]

    protected bool m_HaveControl = true;

    protected bool m_DebugMenuIsOpen = false;

    //初始化buttons
    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");

        //加入button
        buttons.AddRange(new List<Button>
        {
            horizontal,
            vertical,
            jump,
            normalAttack,
            sprint,
            teleport,
            Pick
        });
    }

    void OnEnable()
    {
        if (s_Instance == null)
            s_Instance = this;
        else if (s_Instance != this)
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");


    }

    void OnDisable()
    {

        s_Instance = null;
    }

    protected override void GetInputs()
    {
        foreach (var button in buttons)
        {
            button.Get(inputType);
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            m_DebugMenuIsOpen = !m_DebugMenuIsOpen;
        }
    }

    public override void GainControls()
    {
        m_HaveControl = true;

        foreach (var button in buttons)
        {
            if (button.NeedToGainAndReleaseControl)
                button.GainControl();
        }
    }

    public override void ReleaseControls(bool resetValues = true)
    {
        m_HaveControl = false;

        foreach (var button in buttons)
        {
            if (button.NeedToGainAndReleaseControl)
                StartCoroutine(button.ReleaseControl(resetValues));
        }
    }

}


