using UnityEngine;

public class CharacterMoveControl
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
    /// modifine this to control the speedRate for character
    /// </summary>
    public float SpeedRate { get; set; } = 1.0f;
    /// <summary>
    /// Time start for character acceleration or deceleration
    /// </summary>
    public float AccelerationTimeStart { get; private set; }
    /// <summary>
    /// Time left for character acceleration or deceleration
    /// </summary>
    public float AccelerationTimeLeft { get; private set; }

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
    float m_AccelerationTimeAmount = 1f;
    float m_GroundAccelerationTimeReduceFactor = 1.0f;
    float m_GroundDecelerationTimeReduceFactor = 1.0f;
    float m_AirAccelerationTimeReduceFactor = 1.5f;
    float m_AirDecelerationTimeReduceFactor = 1.5f;


    //public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    //public AnimationCurve deccelerationCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);


    public CharacterMoveControl(float accelerationTimeAmount, float groundAccelerationTimeReduceFactor, float groundDeccelerationTimeReduceFactor, float airAccelerationTimeReduceFactor, float airDeccelerationTimeReduceFactor)
    {
        this.m_AccelerationTimeAmount = accelerationTimeAmount == 0 ? Mathf.Infinity : accelerationTimeAmount;
        this.AccelerationTimeLeft = accelerationTimeAmount;
        this.m_GroundAccelerationTimeReduceFactor = groundAccelerationTimeReduceFactor;
        this.m_GroundDecelerationTimeReduceFactor = groundDeccelerationTimeReduceFactor;
        this.m_AirAccelerationTimeReduceFactor = airAccelerationTimeReduceFactor;
        this.m_AirDecelerationTimeReduceFactor = airDeccelerationTimeReduceFactor;
    }
    public CharacterMoveControl() { }

    /// <summary>
    /// Set the t value of the lerp
    /// </summary>
    /// <param name="normalizedTime">T value of the lerped Speed, set to 0 for no speed and 1 for the maximum speed</param>
    public void SetAccelerationLeftTimeNormalized(float normalizedTime) => AccelerationTimeStart = m_AccelerationTimeAmount == Mathf.Infinity ? 0 : m_AccelerationTimeAmount * (1 - normalizedTime);

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

        return SetCharacterBaseSpeed(characterSpeed, AccelerationTimeLeft);
    }

    protected virtual float SetCharacterBaseSpeed(float characterSpeed, float accelerationTimeLeft)
    {
        LerpedSpeed = Mathf.Lerp(characterSpeed, 0, accelerationTimeLeft / m_AccelerationTimeAmount);
        return LerpedSpeed * SpeedRate;
    }

    void CharacterAccelerating(bool isCharacterAccelerating, bool isGrounded)
    {
        if (isCharacterAccelerating)
        {
            IsDecelerating = false;
            if (AccelerationTimeLeft >= AccelerationTimeStart)
                AccelerationTimeLeft = AccelerationTimeStart;
            if (AccelerationTimeLeft > 0)
            {
                AccelerationTimeLeft -= Time.deltaTime * (isGrounded ? GroundAccelerationFactor * m_GroundAccelerationTimeReduceFactor : AirAccelerationFactor * m_AirAccelerationTimeReduceFactor);
                IsAccelerating = true;
            }
            if (AccelerationTimeLeft <= 0)
            {
                AccelerationTimeLeft = 0;
                IsAccelerating = false;
            }
        }
        else
        {
            IsAccelerating = false;
            if (AccelerationTimeLeft < m_AccelerationTimeAmount)
            {
                AccelerationTimeLeft += Time.deltaTime * (isGrounded ? GroundDecelerationFactor * m_GroundDecelerationTimeReduceFactor : AirDecelerationFactor * m_AirDecelerationTimeReduceFactor);
                IsDecelerating = true;
            }
            if (AccelerationTimeLeft >= m_AccelerationTimeAmount)
            {
                AccelerationTimeLeft = m_AccelerationTimeAmount;
                IsDecelerating = false;
            }
        }
    }
}
