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

    //[SerializeField]
    //private List<InputButton> playerInputButtons;
    //[SerializeField]
    //private List<InputAxis> playerInputAxes;

    private List<Button> buttons = new List<Button>();

    //public InputButton pause = new InputButton(KeyCode.Escape, XboxControllerButtons.Menu);
    //public InputButton interact = new InputButton(KeyCode.E, XboxControllerButtons.Y);
    //public InputButton meleeAttack = new InputButton(KeyCode.K, XboxControllerButtons.X);
    //public InputButton rangedAttack = new InputButton(KeyCode.O, XboxControllerButtons.B);
    public InputButton sprint = new InputButton(KeyCode.LeftShift, XboxControllerButtons.LeftBumper);
    ////TODO:xbox button mapping
    public InputButton teleport = new InputButton(KeyCode.X, XboxControllerButtons.None);
    public InputButton jump = new InputButton(KeyCode.Space, XboxControllerButtons.A);
    public InputAxis horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal);
    public InputAxis vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical);
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

        //buttons.AddRange(playerInputButtons);
        //buttons.AddRange(playerInputAxes);

        //加入button
        buttons.AddRange(new List<Button>
        {
            horizontal,
            vertical,
            jump,
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

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        foreach (var button in buttons)
        {
            button.Get(fixedUpdateHappened, inputType);
        }

        //Pause.Get(fixedUpdateHappened, inputType);
        //Interact.Get(fixedUpdateHappened, inputType);
        //MeleeAttack.Get(fixedUpdateHappened, inputType);
        //RangedAttack.Get(fixedUpdateHappened, inputType);
        //Jump.Get(fixedUpdateHappened, inputType);
        //Horizontal.Get(inputType);
        //Vertical.Get(inputType);

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
            if (button.NeedGainAndReleaseControl)
                button.GainControl();
        }
        //GainControl(Pause);
        //GainControl(Interact);
        //GainControl(MeleeAttack);
        //GainControl(RangedAttack);
        //GainControl(Jump);
        //GainControl(Horizontal);
        //GainControl(Vertical);
    }

    public override void ReleaseControls(bool resetValues = true)
    {
        m_HaveControl = false;

        foreach (var button in buttons)
        {
            if (button.NeedGainAndReleaseControl)
                StartCoroutine(button.ReleaseControl(resetValues));
        }
        //ReleaseControl(Pause, resetValues);
        //ReleaseControl(Interact, resetValues);
        //ReleaseControl(MeleeAttack, resetValues);
        //ReleaseControl(RangedAttack, resetValues);
        //ReleaseControl(Jump, resetValues);
        //ReleaseControl(Horizontal, resetValues);
        //ReleaseControl(Vertical, resetValues);
    }

    //public void DisableMeleeAttacking()
    //{
    //    MeleeAttack.Disable();
    //}

    //public void EnableMeleeAttacking()
    //{
    //    MeleeAttack.Enable();
    //}

    //public void DisableRangedAttacking()
    //{
    //    RangedAttack.Disable();
    //}

    //public void EnableRangedAttacking()
    //{
    //    RangedAttack.Enable();
    //}

    public enum PlayerInputButton
    {

    }
}


