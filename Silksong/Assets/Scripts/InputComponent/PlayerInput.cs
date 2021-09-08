using UnityEngine;

public class PlayerInput : InputComponent
{
    public static PlayerInput Instance
    {
        get { return s_Instance; }
    }

    protected static PlayerInput s_Instance;


    public bool HaveControl { get { return m_HaveControl; } }

    public InputButton Pause = new InputButton(KeyCode.Escape, XboxControllerButtons.Menu);
    public InputButton Interact = new InputButton(KeyCode.E, XboxControllerButtons.Y);
    public InputButton MeleeAttack = new InputButton(KeyCode.K, XboxControllerButtons.X);
    public InputButton RangedAttack = new InputButton(KeyCode.O, XboxControllerButtons.B);
    public InputButton Sprint = new InputButton(KeyCode.LeftShift, XboxControllerButtons.LeftBumper);
    //TODO:xbox button mapping
    public InputButton Teleport = new InputButton(KeyCode.X,XboxControllerButtons.None);
    public InputButton Jump = new InputButton(KeyCode.Space, XboxControllerButtons.A);
    public InputAxis Horizontal = new InputAxis(KeyCode.D, KeyCode.A, XboxControllerAxes.LeftstickHorizontal);
    public InputAxis Vertical = new InputAxis(KeyCode.W, KeyCode.S, XboxControllerAxes.LeftstickVertical);
    [HideInInspector]

    protected bool m_HaveControl = true;

    protected bool m_DebugMenuIsOpen = false;

    void Awake()
    {
        if (s_Instance == null)
            s_Instance = this;
        else
            throw new UnityException("There cannot be more than one PlayerInput script.  The instances are " + s_Instance.name + " and " + name + ".");
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
        Pause.Get(fixedUpdateHappened, inputType);
        Interact.Get(fixedUpdateHappened, inputType);
        MeleeAttack.Get(fixedUpdateHappened, inputType);
        RangedAttack.Get(fixedUpdateHappened, inputType);
        Jump.Get(fixedUpdateHappened, inputType);
        Horizontal.Get(inputType);
        Vertical.Get(inputType);

        if (Input.GetKeyDown(KeyCode.F12))
        {
            m_DebugMenuIsOpen = !m_DebugMenuIsOpen;
        }
    }

    public override void GainControl()
    {
        m_HaveControl = true;

        GainControl(Pause);
        GainControl(Interact);
        GainControl(MeleeAttack);
        GainControl(RangedAttack);
        GainControl(Jump);
        GainControl(Horizontal);
        GainControl(Vertical);
    }

    public override void ReleaseControl(bool resetValues = true)
    {
        m_HaveControl = false;

        ReleaseControl(Pause, resetValues);
        ReleaseControl(Interact, resetValues);
        ReleaseControl(MeleeAttack, resetValues);
        ReleaseControl(RangedAttack, resetValues);
        ReleaseControl(Jump, resetValues);
        ReleaseControl(Horizontal, resetValues);
        ReleaseControl(Vertical, resetValues);
    }

    public void DisableMeleeAttacking()
    {
        MeleeAttack.Disable();
    }

    public void EnableMeleeAttacking()
    {
        MeleeAttack.Enable();
    }

    public void DisableRangedAttacking()
    {
        RangedAttack.Disable();
    }

    public void EnableRangedAttacking()
    {
        RangedAttack.Enable();
    }
}

