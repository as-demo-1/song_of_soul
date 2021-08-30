using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveAccel : MonoBehaviour
{
    public float groundAccelerationFactor = 1.0f;
    public float groundDeccelerationFactor  = 1.0f;
    public float groundAccelerationTimeReduceFactor = 1.0f;
    public float groundDeccelerationTimeReduceFactor = 1.0f;

    public float airAccelerationFactor = 1.0f;
    public float airDeccelerationFactor = 1.0f;
    public float airAccelerationTimeReduceFactor = 1.5f;
    public float airDeccelerationTimeReduceFactor = 1.5f;
    //public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    //public AnimationCurve deccelerationCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

    public bool IsAccelerating { get; private set; }
    public bool IsDeccelerating { get; private set; }
    public bool DoingAccelerating { get; set; }

    float m_AccelerationTime;
    //float m_DeccelerationTimeLeft;
    const float k_AccelerationTimeAmount = 100f;

    const float k_CharacterMaxMoveSpeed = 100f;

    void AccelUpdate(bool characterIsAccelerating, float normalizedTime, bool isGrounded, ref float characterHorizontalSpeed)
    {
        if (!DoingAccelerating)
            return;

        CharacterAccelerating(characterIsAccelerating, isGrounded);

        characterHorizontalSpeed = SetCharacterBaseHorizontalSpeed(characterHorizontalSpeed, m_AccelerationTime, k_CharacterMaxMoveSpeed);

    }

    protected virtual float SetCharacterBaseHorizontalSpeed(float characterHorizontalSpeed, float accelerationTime, float maxMoveSpeed)
    {
        if (IsAccelerating)
            characterHorizontalSpeed = Mathf.Lerp(0, maxMoveSpeed, accelerationTime / k_AccelerationTimeAmount);
        else if (IsDeccelerating)
            characterHorizontalSpeed = Mathf.Lerp(maxMoveSpeed, 0, accelerationTime / k_AccelerationTimeAmount);
        return characterHorizontalSpeed;
    }

    void CharacterAccelerating(bool isCharacterAccelerating, bool isGrounded)
    {
        if (isCharacterAccelerating)
        {
            if (m_AccelerationTime > 0)
            {
                m_AccelerationTime -= Time.deltaTime * Time.timeScale * (isGrounded ? groundAccelerationFactor * groundAccelerationTimeReduceFactor : airAccelerationFactor * airAccelerationTimeReduceFactor);
                IsAccelerating = true;
            }
            else
            {
                m_AccelerationTime = 0.0f;
                IsAccelerating = false;
            }
        }
        else
        {
            if (m_AccelerationTime > 0)
            {
                m_AccelerationTime -= Time.deltaTime * Time.timeScale * (isGrounded ? groundDeccelerationFactor * groundDeccelerationTimeReduceFactor : airDeccelerationFactor * groundDeccelerationTimeReduceFactor);
                IsDeccelerating = true;
            }
            else
            {
                m_AccelerationTime = 0.0f;
                IsDeccelerating = false;
            }
        }
    }

    private void Update()
    {
        float horizontalVelocity = 0f;
        AccelUpdate(true, .5f, true, ref horizontalVelocity);
    }
}
