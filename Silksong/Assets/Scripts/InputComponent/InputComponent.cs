using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Created by Unity from:
//https://assetstore.unity.com/packages/templates/tutorials/2d-game-kit-107098#description
public abstract class InputComponent : MonoBehaviour
{
    // todo:test
    public Queue<Action> actions = new Queue<Action>();

    public enum InputType
    {
        MouseAndKeyboard,
        Controller,
    }


    public enum XboxControllerButtons
    {
        None,
        A,
        B,
        X,
        Y,
        Leftstick,
        Rightstick,
        View,
        Menu,
        LeftBumper,
        RightBumper,
    }


    public enum XboxControllerAxes
    {
        None,
        LeftstickHorizontal,
        LeftstickVertical,
        DpadHorizontal,
        DpadVertical,
        RightstickHorizontal,
        RightstickVertical,
        LeftTrigger,
        RightTrigger,
    }


    [Serializable]
    public class InputButton : Button
    {
        public KeyCode key;
        public XboxControllerButtons controllerButton;
        public bool Down { get; protected set; }
        public bool Held { get; protected set; }
        public bool Up { get; protected set; }
        public bool IsValid { get; protected set; }
        public bool Enabled
        {
            get { return m_Enabled; }
        }
        public bool BufferEnabled
        {
            get { return m_BufferEnabled; }
        }

        [SerializeField]
        protected bool m_Enabled = true;
        protected bool m_BufferEnabled = true;
        protected bool m_GettingInput = true;
        protected int m_FrameCount;

        //This is used to change the state of a button (Down, Up) only if at least a FixedUpdate happened between the previous Frame
        //and this one. Since movement are made in FixedUpdate, without that an input could be missed it get press/release between fixedupdate
        bool m_AfterFixedUpdateDown;
        bool m_AfterFixedUpdateHeld;
        bool m_AfterFixedUpdateUp;

        protected static readonly Dictionary<int, string> k_ButtonsToName = new Dictionary<int, string>
            {
                {(int)XboxControllerButtons.A, "A"},
                {(int)XboxControllerButtons.B, "B"},
                {(int)XboxControllerButtons.X, "X"},
                {(int)XboxControllerButtons.Y, "Y"},
                {(int)XboxControllerButtons.Leftstick, "Leftstick"},
                {(int)XboxControllerButtons.Rightstick, "Rightstick"},
                {(int)XboxControllerButtons.View, "View"},
                {(int)XboxControllerButtons.Menu, "Menu"},
                {(int)XboxControllerButtons.LeftBumper, "Left Bumper"},
                {(int)XboxControllerButtons.RightBumper, "Right Bumper"},
            };

        public InputButton(KeyCode key, XboxControllerButtons controllerButton)
        {
            this.key = key;
            this.controllerButton = controllerButton;
        }

        public override void Get(bool fixedUpdateHappened, InputType inputType)
        {
            if (!m_Enabled)
            {
                return;
            }

            if (!m_GettingInput)
                return;

            if (inputType == InputType.Controller)
            {
                if (fixedUpdateHappened)
                {
                    Down = Input.GetButtonDown(k_ButtonsToName[(int)controllerButton]);
                    Held = Input.GetButton(k_ButtonsToName[(int)controllerButton]);
                    Up = Input.GetButtonUp(k_ButtonsToName[(int)controllerButton]);

                    m_AfterFixedUpdateDown = Down;
                    m_AfterFixedUpdateHeld = Held;
                    m_AfterFixedUpdateUp = Up;
                }
                else
                {
                    Down = Input.GetButtonDown(k_ButtonsToName[(int)controllerButton]) || m_AfterFixedUpdateDown;
                    Held = Input.GetButton(k_ButtonsToName[(int)controllerButton]) || m_AfterFixedUpdateHeld;
                    Up = Input.GetButtonUp(k_ButtonsToName[(int)controllerButton]) || m_AfterFixedUpdateUp;

                    m_AfterFixedUpdateDown |= Down;
                    m_AfterFixedUpdateHeld |= Held;
                    m_AfterFixedUpdateUp |= Up;
                }
            }
            else if (inputType == InputType.MouseAndKeyboard)
            {
                if (fixedUpdateHappened)
                {
                    Down = Input.GetKeyDown(key);
                    Held = Input.GetKey(key);
                    Up = Input.GetKeyUp(key);

                    m_AfterFixedUpdateDown = Down;
                    m_AfterFixedUpdateHeld = Held;
                    m_AfterFixedUpdateUp = Up;
                }
                else//update键入后，在下一个fixedupdate发生前认为一直有键入 目的是为了在update顺序随机情况下不丢失输入
                {
                    Down = Input.GetKeyDown(key) || m_AfterFixedUpdateDown;
                    Held = Input.GetKey(key) || m_AfterFixedUpdateHeld;
                    Up = Input.GetKeyUp(key) || m_AfterFixedUpdateUp;

                    m_AfterFixedUpdateDown |= Down;
                    m_AfterFixedUpdateHeld |= Held;
                    m_AfterFixedUpdateUp |= Up;
                }
            }
            IsValidUpdate(Down);

        }
        public void IsValidUpdate(bool conditions)
        {
            if (!m_BufferEnabled)
                return;
            if (conditions)
            {
                m_FrameCount = Constants.BufferFrameTime;
            }

            // 一次按下只触发一次IsValid
            if (m_FrameCount >= Constants.BufferFrameTime)
            {
                IsValid = true;
                --m_FrameCount;
            }
            else
            {
                IsValid = false;
            }
        }
        public void SetValidToFalse()//无用，down仍然有效，valid将不断激活
        {
            m_FrameCount = 0;
            IsValid = false;
        }

        public override void Enable()//
        {
            m_Enabled = true;       
        }

        public override void Disable()//冻结控制使用此函数
        {
            if (NotNeedGainAndReleaseControl) return;

            m_Enabled = false;
            Down = false;
            Held = false;
            Up = false;
            m_AfterFixedUpdateDown = false;
            m_AfterFixedUpdateHeld = false;
            m_AfterFixedUpdateUp = false;

            m_FrameCount = 0;
            IsValid = false;
        }

        public override void GainControl()
        {
            m_GettingInput = true;
        }

        public override IEnumerator ReleaseControl(bool resetValues)
        {
            m_GettingInput = false;

            if (!resetValues)
                yield break;

            if (Down)
                Up = true;
            Down = false;
            Held = false;

            m_AfterFixedUpdateDown = false;
            m_AfterFixedUpdateHeld = false;
            m_AfterFixedUpdateUp = false;

            yield return null;

            Up = false;
        }
    }

    [Serializable]
    public class InputAxis : Button
    {
        public KeyCode positive;
        public KeyCode negative;
        public XboxControllerAxes controllerAxis;
        public float Value { get; protected set; }//can only be -1 0 or 1

        public float ValueBuffer;
        private bool m_BufferEnabled=true;
        private int m_FrameCount;
        public bool ReceivingInput { get; protected set; }
        public bool Enabled
        {
            get { return m_Enabled; }
        }

        protected bool m_Enabled = true;
        protected bool m_GettingInput = true;

        protected readonly static Dictionary<int, string> k_AxisToName = new Dictionary<int, string> {
                {(int)XboxControllerAxes.LeftstickHorizontal, "Leftstick Horizontal"},
                {(int)XboxControllerAxes.LeftstickVertical, "Leftstick Vertical"},
                {(int)XboxControllerAxes.DpadHorizontal, "Dpad Horizontal"},
                {(int)XboxControllerAxes.DpadVertical, "Dpad Vertical"},
                {(int)XboxControllerAxes.RightstickHorizontal, "Rightstick Horizontal"},
                {(int)XboxControllerAxes.RightstickVertical, "Rightstick Vertical"},
                {(int)XboxControllerAxes.LeftTrigger, "Left Trigger"},
                {(int)XboxControllerAxes.RightTrigger, "Right Trigger"},
            };

        public InputAxis(KeyCode positive, KeyCode negative, XboxControllerAxes controllerAxis)
        {
            this.positive = positive;
            this.negative = negative;
            this.controllerAxis = controllerAxis;
        }

        public override void Get(bool fixedUpdateHappened, InputType inputType)
        {
            if (!m_Enabled)
            {
               // Value = 0f;
                return;
            }

            if (!m_GettingInput)
                return;

            bool positiveHeld = false;
            bool negativeHeld = false;

            if (inputType == InputType.Controller)
            {
                float value = Input.GetAxisRaw(k_AxisToName[(int)controllerAxis]);
                positiveHeld = value > Single.Epsilon;
                negativeHeld = value < -Single.Epsilon;
            }
            else if (inputType == InputType.MouseAndKeyboard)
            {
                positiveHeld = Input.GetKey(positive);
                negativeHeld = Input.GetKey(negative);
            }

            if (positiveHeld == negativeHeld)
                Value = 0f;
            else if (positiveHeld)
                Value = 1f;
            else
                Value = -1f;

            ReceivingInput = positiveHeld || negativeHeld;

            IsValidUpdate(ReceivingInput);
        }
        public void IsValidUpdate(bool ReceivingInput)
        {
            if (!m_BufferEnabled)
                return;
            if (ReceivingInput)
            {
                m_FrameCount = Constants.BufferFrameTime;
                ValueBuffer = Value;
                return;
            }


            if (m_FrameCount > 0)
            {
                --m_FrameCount;
            }
            else
            {
                ValueBuffer = 0;
            }
        }
        public override void Enable()
        {
            m_Enabled = true;
        }

        public override void Disable()
        {
            if (NotNeedGainAndReleaseControl) return;

            m_Enabled = false;
            Value = 0f;
        }

        public override void GainControl()
        {
            m_GettingInput = true;
        }

        public override IEnumerator ReleaseControl(bool resetValues)
        {
            m_GettingInput = false;
            if (resetValues)
            {
                Value = 0f;
                ReceivingInput = false;
            }
            yield break;
        }
    }

    public class Button : IButton
    {
        //public PlayerInputButton buttonName;
        public bool NotNeedGainAndReleaseControl;
        public virtual void GainControl() { }

        public virtual void Get(bool fixedUpdateHappened, InputType inputType) { }

        public virtual IEnumerator ReleaseControl(bool resetValues)
        {
            yield break;
        }

        public virtual void Enable()
        {
        }

        public virtual void Disable()
        {
        }


    }

    public InputType inputType = InputType.MouseAndKeyboard;

    bool m_FixedUpdateHappened;

    void Update()
    {
        // todo:test
        if (actions.Count != 0)
        {
            actions.Dequeue()();
        }

        GetInputs(m_FixedUpdateHappened || Mathf.Approximately(Time.timeScale, 0));

        m_FixedUpdateHappened = false;
    }

    void FixedUpdate()
    {
        m_FixedUpdateHappened = true;
    }

    protected abstract void GetInputs(bool fixedUpdateHappened);

    public abstract void GainControls();

    public abstract void ReleaseControls(bool resetValues = true);

   /* [Obsolete]
    protected void GainControl(InputButton inputButton)
    {
        inputButton.GainControl();
    }
    [Obsolete]
    protected void GainControl(InputAxis inputAxis)
    {
        inputAxis.GainControl();
    }
    [Obsolete]
    public void ReleaseControl(InputButton inputButton, bool resetValues)
    {
        StartCoroutine(inputButton.ReleaseControl(resetValues));
    }
    [Obsolete]
    public void ReleaseControl(InputAxis inputAxis, bool resetValues)
    {
        inputAxis.ReleaseControl(resetValues);
    }*/

    public interface IButton
    {
        public void Get(bool fixedUpdateHappened, InputType inputType);
        public void GainControl();
        public IEnumerator ReleaseControl(bool resetValues);
    }
}

