using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Created by Unity from:
//https://assetstore.unity.com/packages/templates/tutorials/2d-game-kit-107098#description
public abstract class InputComponent : MonoBehaviour
{
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

        public InputButton(KeyCode key, XboxControllerButtons controllerButton, bool needGainAndReleaseControl) : base(needGainAndReleaseControl)
        {
            this.key = key;
            this.controllerButton = controllerButton;
        }

        public override void Get(InputType inputType)
        {
            if (!m_Enabled)
            {
                Down = false;
                Held = false;
                Up = false;
                return;
            }

            if (!m_GettingInput)
                return;

            if (inputType == InputType.Controller)
            {
                Down = Input.GetButtonDown(k_ButtonsToName[(int)controllerButton]);
                Held = Input.GetButton(k_ButtonsToName[(int)controllerButton]);
                Up = Input.GetButtonUp(k_ButtonsToName[(int)controllerButton]);
            }
            else if (inputType == InputType.MouseAndKeyboard)
            {
                Down = Input.GetKeyDown(key);
                Held = Input.GetKey(key);
                Up = Input.GetKeyUp(key);
            }
            IsValidUpdate(Down);
        }
        void IsValidUpdate(bool conditions)
        {
            if (!m_BufferEnabled)
                return;
            if (conditions)
                m_FrameCount = Constants.BufferFrameTime;
            if (m_FrameCount > 0)
            {
                IsValid = true;
                --m_FrameCount;
            }
            else
            {
                IsValid = false;
            }
        }
        public void SetValidToFalse()
        {
            IsValid = false;
            m_FrameCount = 0;
        }

        public void Enable()
        {
            m_Enabled = true;
        }

        public void Disable()
        {
            m_Enabled = false;
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
        public float Value { get; protected set; }
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

        public InputAxis(KeyCode positive, KeyCode negative, XboxControllerAxes controllerAxis, bool needToGainAndReleaseControl) : base(needToGainAndReleaseControl)
        {
            this.positive = positive;
            this.negative = negative;
            this.controllerAxis = controllerAxis;
        }

        public override void Get(InputType inputType)
        {
            if (!m_Enabled)
            {
                Value = 0f;
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
        }

        public void Enable()
        {
            m_Enabled = true;
        }

        public void Disable()
        {
            m_Enabled = false;
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
        public bool NeedToGainAndReleaseControl;
        public virtual void GainControl() { }

        public virtual void Get(InputType inputType) { }

        public virtual IEnumerator ReleaseControl(bool resetValues)
        {
            yield break;
        }

        public Button(bool needToGainAndReleaseControl) => this.NeedToGainAndReleaseControl = needToGainAndReleaseControl;
    }

    public InputType inputType = InputType.MouseAndKeyboard;

    void Update()
    {
        GetInputs();
    }

    protected abstract void GetInputs();

    public abstract void GainControls();

    public abstract void ReleaseControls(bool resetValues = true);

    [Obsolete]
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
    }

    public interface IButton
    {
        public void Get(InputType inputType);
        public void GainControl();
        public IEnumerator ReleaseControl(bool resetValues);
    }
}

