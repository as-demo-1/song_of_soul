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
    private bool m_isFrozen;

    private List<Button> buttons = new List<Button>();

    public InputButton sprint = new InputButton(KeyCode.LeftShift, XboxControllerButtons.LeftBumper);
    public InputButton Pick = new InputButton(KeyCode.F, XboxControllerButtons.Y);
    ////TODO:xbox button mapping
    public InputButton plunge = new InputButton(KeyCode.H, XboxControllerButtons.None);
    public InputButton teleport = new InputButton(KeyCode.X, XboxControllerButtons.None);
    public InputButton jump = new InputButton(KeyCode.K, XboxControllerButtons.A);
    public InputButton interact = new InputButton(KeyCode.W, XboxControllerButtons.None);
    public InputButton breakMoon = new InputButton(KeyCode.Q, XboxControllerButtons.None);
    public InputButton heal = new InputButton(KeyCode.C, XboxControllerButtons.None);
    public InputButton toCat = new InputButton(KeyCode.N, XboxControllerButtons.None);
    public InputButton castSkill = new InputButton(KeyCode.L, XboxControllerButtons.None);
    public InputAxis horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal);
    public InputAxis vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical);
    public InputButton normalAttack = new InputButton(KeyCode.J, XboxControllerButtons.X);
    public InputButton soulSkill = new InputButton(KeyCode.R, XboxControllerButtons.None);
    public InputButton sing = new InputButton(KeyCode.Z, XboxControllerButtons.None);
    public InputButton heartSword = new InputButton(KeyCode.U, XboxControllerButtons.None);
    ////TODO:xbox button mapping
    public InputButton showMap = new InputButton(KeyCode.M, XboxControllerButtons.None);
    ////TODO:xbox button mapping
    public InputButton quickMap = new InputButton(KeyCode.Tab, XboxControllerButtons.None);
    [HideInInspector]

    protected bool m_HaveControl = true;

    protected bool m_DebugMenuIsOpen = false;

    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");

        buttons.AddRange(new List<Button>
        {
            horizontal,
            vertical,
            jump,
            interact,
            normalAttack,
            soulSkill,
            sprint,
            teleport,
            Pick,
            breakMoon,
            heal,
            toCat,
            castSkill,
            showMap,
            quickMap,
            plunge,
            sing,
            heartSword,
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

    public void ToggleFrozen(bool isFrozen)
    {
        m_isFrozen = isFrozen;
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        if (m_isFrozen)
        {
            return;
        }
        foreach (var button in buttons)
        {
            button.Get(fixedUpdateHappened, inputType);
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            m_DebugMenuIsOpen = !m_DebugMenuIsOpen;
        }
    }



    public override void ReleaseControls(bool resetValues = true)
    {
        Debug.Log("releaseCtrl");
        m_HaveControl = false;

        foreach (var button in buttons)
        {
            button.Disable();
        }
    }

    public override void GainControls()
    {
        Debug.Log("gainCtrl");
        m_HaveControl = true;

        foreach (var button in buttons)
        {
            button.Enable();
        }
    }

}


