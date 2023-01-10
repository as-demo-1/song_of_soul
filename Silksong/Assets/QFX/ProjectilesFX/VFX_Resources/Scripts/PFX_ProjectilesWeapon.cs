#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif

using UnityEngine;


public class PFX_ProjectilesWeapon : MonoBehaviour
{
    public ParticleSystem[] ParticleSystems;

    public float FireRate = 0.15f;

    private bool _isButtonHold;
    private float _time;

    private void LateUpdate()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
            return;

        var lookDelta = hit.point - transform.position;
        var targetRot = Quaternion.LookRotation(lookDelta);
        transform.rotation = targetRot;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0))
            _isButtonHold = true;
        else if (Input.GetMouseButtonUp(0))
            _isButtonHold = false;
#endif

        _time += Time.deltaTime;

        if (!_isButtonHold)
            return;

        if (_time < FireRate)
            return;

        foreach (var ps in ParticleSystems)
            ps.Emit(1);

        _time = 0;
    }
}
