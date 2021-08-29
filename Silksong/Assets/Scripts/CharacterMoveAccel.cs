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

    public bool IsGrounded { get; private set; }
    float m_AccelerationTimeLeft;
    float m_DeccelerationTimeLeft;
    const float k_AccelerationTimeAmount = 100f;

    const float k_CharacterMaxMoveSpeed = 100f;
    float m_CharacterMoveSpeed;

    bool m_CharacterIsMovingAndInput;
    Coroutine CharacterAccelCoroutine;
    Coroutine CharacterDeccelCoroutine;

    void AccelUpdate()
    {
        if (m_CharacterIsMovingAndInput)
        {
            m_AccelerationTimeLeft = k_AccelerationTimeAmount - m_DeccelerationTimeLeft;
            StopCoroutine(CharacterDeccelCoroutine);
            CharacterAccelCoroutine = StartCoroutine(CharacterAccelerating(IsGrounded));
        }
        else
        {
            m_DeccelerationTimeLeft = k_AccelerationTimeAmount - m_AccelerationTimeLeft;
            StopCoroutine(CharacterAccelCoroutine);
            CharacterDeccelCoroutine = StartCoroutine(CharacterDeccelerating(IsGrounded));
        }
    }

    //void SetCharacterMoveSpeed()
    //{
    //    if (IsAccelerating)
    //        m_CharacterMoveSpeed = Mathf.Lerp(m_CharacterMoveSpeed, k_CharacterMaxMoveSpeed, 1 - m_AccelerationTimeLeft / k_AccelerationTimeAmount);
    //    if (IsDeccelerating)
    //        m_CharacterMoveSpeed = Mathf.Lerp(m_CharacterMoveSpeed, 0, 1 - m_DeccelerationTimeLeft / k_AccelerationTimeAmount);
    //}

    IEnumerator CharacterAccelerating(bool isGrounded)
    {
        if (m_AccelerationTimeLeft > 0)
        {
            m_AccelerationTimeLeft -= Time.deltaTime * Time.timeScale * (isGrounded ? groundAccelerationFactor * groundAccelerationTimeReduceFactor : airAccelerationFactor * airAccelerationTimeReduceFactor);
            IsAccelerating = true;
        }
        else
        {
            m_AccelerationTimeLeft = 0.0f;
            IsAccelerating = false;
        }
        yield return null;
    }

    IEnumerator CharacterDeccelerating(bool isGrounded)
    {
        if (m_DeccelerationTimeLeft > 0)
        {
            m_DeccelerationTimeLeft -= Time.deltaTime * Time.timeScale * (isGrounded ? groundDeccelerationFactor * groundDeccelerationTimeReduceFactor : airDeccelerationFactor * groundDeccelerationTimeReduceFactor);
            IsDeccelerating = true;
        }
        else
        {
            m_DeccelerationTimeLeft = 0.0f;
            IsDeccelerating = false;
        }
        yield return null;
    }
}
