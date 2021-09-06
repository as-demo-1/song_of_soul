using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveAccel
{
    public float GroundAccelerationFactor { get; set; } = 1.0f;
    public float GroundDeccelerationFactor { get; set; } = 1.0f;
    float groundAccelerationTimeReduceFactor = 1.0f;
    float groundDeccelerationTimeReduceFactor = 1.0f;

    public float AirAccelerationFactor { get; set; } = 1.0f;
    public float AirDeccelerationFactor { get; set; } = 1.0f;
    float airAccelerationTimeReduceFactor = 1.5f;
    float airDeccelerationTimeReduceFactor = 1.5f;
    //public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    //public AnimationCurve deccelerationCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);

    public bool IsAccelerating { get; private set; }
    public bool IsDeccelerating { get; private set; }
    public float LerpSpeed { get; private set; }

    public float AccelerationTime { get; private set; } = 1.0f;
    float accelerationTimeAmount = 1f;

    public CharacterMoveAccel(float accelerationTimeAmount, float groundAccelerationTimeReduceFactor, float groundDeccelerationTimeReduceFactor, float airAccelerationTimeReduceFactor, float airDeccelerationTimeReduceFactor)
    {
        this.accelerationTimeAmount = accelerationTimeAmount;
        this.groundAccelerationTimeReduceFactor = groundAccelerationTimeReduceFactor;
        this.groundDeccelerationTimeReduceFactor = groundDeccelerationTimeReduceFactor;
        this.airAccelerationTimeReduceFactor = airAccelerationTimeReduceFactor;
        this.airDeccelerationTimeReduceFactor = airDeccelerationTimeReduceFactor;
    }
    public CharacterMoveAccel() { }

    public void SetAccelerationTime(float normalizedTime) => AccelerationTime = accelerationTimeAmount * normalizedTime;

    public float AccelSpeedUpdate(bool characterIsAccelerating, bool isGrounded, float characterHorizontalSpeed)
    {
        CharacterAccelerating(characterIsAccelerating, isGrounded);

        return SetCharacterBaseHorizontalSpeed(characterHorizontalSpeed, AccelerationTime);
    }

    protected virtual float SetCharacterBaseHorizontalSpeed(float characterHorizontalSpeed, float accelerationTime)
    {
        LerpSpeed = Mathf.Lerp(characterHorizontalSpeed, 0, accelerationTime / accelerationTimeAmount); ;
        return LerpSpeed;
    }

    void CharacterAccelerating(bool isCharacterAccelerating, bool isGrounded)
    {
        if (isCharacterAccelerating)
        {
            IsDeccelerating = false;
            if (AccelerationTime > 0)
            {
                AccelerationTime -= Time.deltaTime * Time.timeScale * (isGrounded ? GroundAccelerationFactor * groundAccelerationTimeReduceFactor : AirAccelerationFactor * airAccelerationTimeReduceFactor);
                IsAccelerating = true;
            }
            else
            {
                AccelerationTime = 0.0f;
                IsAccelerating = false;
            }
        }
        else
        {
            IsAccelerating = false;
            if (accelerationTimeAmount - AccelerationTime > 0)
            {
                AccelerationTime += Time.deltaTime * Time.timeScale * (isGrounded ? GroundDeccelerationFactor * groundDeccelerationTimeReduceFactor : AirDeccelerationFactor * airDeccelerationTimeReduceFactor);
                IsDeccelerating = true;
            }
            else
            {
                AccelerationTime = accelerationTimeAmount;
                IsDeccelerating = false;
            }
        }
    }
}
