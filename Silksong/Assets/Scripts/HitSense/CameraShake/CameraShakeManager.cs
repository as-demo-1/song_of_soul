using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor;
public class CameraShakeManager : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;
    public static CameraShakeManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<CameraShakeManager>();

            if (instance != null)
                return instance;

            GameObject CameraShakeManager = new GameObject("CameraShakeManager");
            instance = CameraShakeManager.AddComponent<CameraShakeManager>();

            return instance;
        }
    }//µ¥Àý

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<CinemachineImpulseSource>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
#if UNITY_EDITOR
        impulseSource.m_ImpulseDefinition.m_RawSignal = (SignalSourceAsset)AssetDatabase.LoadAssetAtPath("Assets/Scripts/HitSense/CameraShake/shake.asset", typeof( SignalSourceAsset));
#endif  
    }
    protected static CameraShakeManager instance;
    void Start()
    {
        
    }

    public void cameraShake(Vector2 force)
    {
        impulseSource.GenerateImpulse(force);
    }
    public void cameraShake(float force)
    {
        impulseSource.GenerateImpulse(force);
    }
}
