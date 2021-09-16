using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveAccel
{
    /// <summary>
    /// Set this to dynamically modify the factor of a character ground acceleration, default value is 1.0f
    /// </summary>
    public float GroundAccelerationFactor { get; set; } = 1.0f;
    /// <summary>
    /// Set this to dynamically modify the factor of a character ground deceleration, default value is 1.0f
    /// </summary>
    public float GroundDecelerationFactor { get; set; } = 1.0f;
    /// <summary>
    /// Set this to dynamically modify the factor of a character air acceleration, default value is 1.0f
    /// </summary>
    public float AirAccelerationFactor { get; set; } = 1.0f;
    /// <summary>
    /// Set this to dynamically modify the factor of a character air deceleration, default value is 1.0f
    /// </summary>
    public float AirDecelerationFactor { get; set; } = 1.0f;
    /// <summary>
    /// Time remaining for character acceleration or deceleration
    /// </summary>
    public float AccelerationTime { get; private set; }

    /// <summary>
    /// Whether it is Accelerating
    /// </summary>
    public bool IsAccelerating { get; private set; }
    /// <summary>
    /// Whether it is Decelerating
    /// </summary>
    public bool IsDecelerating { get; private set; }
    /// <summary>
    /// Lerped Speed calculated by AccelSpeedUpdate method
    /// </summary>
    public float LerpedSpeed { get; private set; }

    // Acceleration or deceleration initial factor, and those would be determined in edit mode or setup by constructor
    float accelerationTimeAmount = 1f;
    float groundAccelerationTimeReduceFactor = 1.0f;
    float groundDecelerationTimeReduceFactor = 1.0f;
    float airAccelerationTimeReduceFactor = 1.5f;
    float airDecelerationTimeReduceFactor = 1.5f;


    //public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    //public AnimationCurve deccelerationCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);


    public CharacterMoveAccel(float accelerationTimeAmount, float groundAccelerationTimeReduceFactor, float groundDeccelerationTimeReduceFactor, float airAccelerationTimeReduceFactor, float airDeccelerationTimeReduceFactor)
    {
        this.accelerationTimeAmount = accelerationTimeAmount;
        this.groundAccelerationTimeReduceFactor = groundAccelerationTimeReduceFactor;
        this.groundDecelerationTimeReduceFactor = groundDeccelerationTimeReduceFactor;
        this.airAccelerationTimeReduceFactor = airAccelerationTimeReduceFactor;
        this.airDecelerationTimeReduceFactor = airDeccelerationTimeReduceFactor;
    }
    public CharacterMoveAccel() { }

    /// <summary>
    /// Set the t value of the lerp
    /// </summary>
    /// <param name="normalizedTime">T value of the lerped Speed, set to 0 for no speed and 1 for the maximum speed</param>
    public void SetAccelerationNormalizedTime(float normalizedTime) => AccelerationTime = accelerationTimeAmount * (1 - normalizedTime);

    /// <summary>
    /// Call this in UpdateMethod to set speed to a linear one
    /// </summary>
    /// <param name="characterIsAccelerating">Set this to true if the character will accelerate,or false if it will deccelerate</param>
    /// <param name="isGrounded">Set to true if character is grounded</param>
    /// <param name="characterSpeed">Character moving speed in one dimension</param>
    /// <returns></returns>
    public float AccelSpeedUpdate(bool characterIsAccelerating, bool isGrounded, float characterSpeed)
    {
        CharacterAccelerating(characterIsAccelerating, isGrounded);

        return SetCharacterBaseSpeed(characterSpeed, AccelerationTime);
    }

    protected virtual float SetCharacterBaseSpeed(float characterHorizontalSpeed, float accelerationTime)
    {
        LerpedSpeed = Mathf.Lerp(characterHorizontalSpeed, 0, accelerationTime / accelerationTimeAmount); ;
        return LerpedSpeed;
    }

    void CharacterAccelerating(bool isCharacterAccelerating, bool isGrounded)
    {
        if (isCharacterAccelerating)
        {
            IsDecelerating = false;
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
                AccelerationTime += Time.deltaTime * Time.timeScale * (isGrounded ? GroundDecelerationFactor * groundDecelerationTimeReduceFactor : AirDecelerationFactor * airDecelerationTimeReduceFactor);
                IsDecelerating = true;
            }
            else
            {
                AccelerationTime = accelerationTimeAmount;
                IsDecelerating = false;
            }
        }
    }
}
