//////////////////////////////////////////////
/// 2DxFX - 2D SPRITE FX - by VETASOFT 2018 //
//////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("2DxFX/Standard/Wave")]
[System.Serializable]
public class _2dxFX_Wave : MonoBehaviour
{
    [HideInInspector] public Material ForceMaterial;
    [HideInInspector] public bool ActiveChange = true;
    private string shader = "2DxFX/Standard/Wave";
    [HideInInspector] [Range(0, 1)] public float _Alpha = 1f;
    [HideInInspector] [Range(0f, 128f)] public float _OffsetX = 10f;
    [HideInInspector] [Range(0f, 128f)] public float _OffsetY = 10f;
    [HideInInspector] [Range(0f, 1f)] public float _DistanceX = 0.03f;
    [HideInInspector] [Range(0f, 1f)] public float _DistanceY = 0.03f;
    [HideInInspector] [Range(0f, 6.28f)] public float _WaveTimeX = 0.16f;
    [HideInInspector] [Range(0f, 6.28f)] public float _WaveTimeY = 0.12f;
    [HideInInspector] public bool AutoPlayWaveX = false;
    [HideInInspector] [Range(0f, 5f)] public float AutoPlaySpeedX = 5f;
    [HideInInspector] public bool AutoPlayWaveY = false;
    [HideInInspector] [Range(0f, 50f)] public float AutoPlaySpeedY = 5f;
    [HideInInspector] public bool AutoRandom = false;
    [HideInInspector] [Range(0f, 50f)] public float AutoRandomRange = 10f;

    [HideInInspector] public int ShaderChange = 0;
    Material tempMaterial;
    Material defaultMaterial;
    Image CanvasImage;
    SpriteRenderer CanvasSpriteRenderer;[HideInInspector] public bool ActiveUpdate = true;

    void Awake()
    {
        if (this.gameObject.GetComponent<Image>() != null) CanvasImage = this.gameObject.GetComponent<Image>();
        if (this.gameObject.GetComponent<SpriteRenderer>() != null) CanvasSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        ShaderChange = 0;
        XUpdate();
    }

    public void CallUpdate()
    {
        XUpdate();
    }


    void Update()
    {
        if (ActiveUpdate) XUpdate();
    }

    void XUpdate()
    {

        if (CanvasImage == null)
        {
            if (this.gameObject.GetComponent<Image>() != null) CanvasImage = this.gameObject.GetComponent<Image>();
        }
        if (CanvasSpriteRenderer == null)
        {
            if (this.gameObject.GetComponent<SpriteRenderer>() != null) CanvasSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }
        if ((ShaderChange == 0) && (ForceMaterial != null))
        {
            ShaderChange = 1;
            if (tempMaterial != null) DestroyImmediate(tempMaterial);
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = ForceMaterial;
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material = ForceMaterial;
            }
            ForceMaterial.hideFlags = HideFlags.None;
            ForceMaterial.shader = Shader.Find(shader);


        }
        if ((ForceMaterial == null) && (ShaderChange == 1))
        {
            if (tempMaterial != null) DestroyImmediate(tempMaterial);
            tempMaterial = new Material(Shader.Find(shader));
            tempMaterial.hideFlags = HideFlags.None;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = tempMaterial;
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material = tempMaterial;
            }
            ShaderChange = 0;
        }

#if UNITY_EDITOR
        string dfname = "";
        if (CanvasSpriteRenderer != null) dfname = CanvasSpriteRenderer.sharedMaterial.shader.name;
        if (CanvasImage != null)
        {
            Image img = CanvasImage;
            if (img.material == null) dfname = "Sprites/Default";
        }
        if (dfname == "Sprites/Default")
        {
            ForceMaterial.shader = Shader.Find(shader);
            ForceMaterial.hideFlags = HideFlags.None;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = ForceMaterial;
            }
            else if (CanvasImage != null)
            {
                Image img = CanvasImage;
                if (img.material == null)
                {
                    CanvasImage.material = ForceMaterial;
                }
            }
        }
#endif
        if (ActiveChange)
        {
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Alpha", 1 - _Alpha);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_OffsetX", _OffsetX);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_OffsetY", _OffsetY);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_DistanceX", _DistanceX);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_DistanceY", _DistanceY);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_WaveTimeX", _WaveTimeX);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_WaveTimeY", _WaveTimeY);
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material.SetFloat("_Alpha", 1 - _Alpha);
                CanvasImage.material.SetFloat("_OffsetX", _OffsetX);
                CanvasImage.material.SetFloat("_OffsetY", _OffsetY);
                CanvasImage.material.SetFloat("_DistanceX", _DistanceX);
                CanvasImage.material.SetFloat("_DistanceY", _DistanceY);
                CanvasImage.material.SetFloat("_WaveTimeX", _WaveTimeX);
                CanvasImage.material.SetFloat("_WaveTimeY", _WaveTimeY);
            }

            float timerange;
            if (AutoRandom)
            {
                timerange = (Random.Range(1, AutoRandomRange) / 5) * Time.deltaTime;
            }
            else
            {
                timerange = Time.deltaTime;
            }

            if (AutoPlayWaveX) _WaveTimeX += AutoPlaySpeedX * timerange;
            if (AutoPlayWaveY) _WaveTimeY += AutoPlaySpeedY * timerange;
            if (_WaveTimeX > 6.28f) _WaveTimeX = 0f;
            if (_WaveTimeY > 6.28f) _WaveTimeY = 0f;
        }

    }

    void OnDestroy()
    {
       
        if ((Application.isPlaying == false) && (Application.isEditor == true))
        {

            if (tempMaterial != null) DestroyImmediate(tempMaterial);

            if (gameObject.activeSelf && defaultMaterial != null)
            {
                if (CanvasSpriteRenderer != null)
                {
                    CanvasSpriteRenderer.sharedMaterial = defaultMaterial;
                    CanvasSpriteRenderer.sharedMaterial.hideFlags = HideFlags.None;
                }
                else if (CanvasImage != null)
                {
                    CanvasImage.material = defaultMaterial;
                    CanvasImage.material.hideFlags = HideFlags.None;
                }
            }
        }
    }
    void OnDisable()
    {
       
        if (gameObject.activeSelf && defaultMaterial != null)
        {
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = defaultMaterial;
                CanvasSpriteRenderer.sharedMaterial.hideFlags = HideFlags.None;
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material = defaultMaterial;
                CanvasImage.material.hideFlags = HideFlags.None;
            }
        }
    }

    void OnEnable()
    {
       
        if (defaultMaterial == null)
        {
            defaultMaterial = new Material(Shader.Find("Sprites/Default"));


        }
        if (ForceMaterial == null)
        {
            ActiveChange = true;
            tempMaterial = new Material(Shader.Find(shader));
            tempMaterial.hideFlags = HideFlags.None;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = tempMaterial;
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material = tempMaterial;
            }
        }
        else
        {
            ForceMaterial.shader = Shader.Find(shader);
            ForceMaterial.hideFlags = HideFlags.None;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial = ForceMaterial;
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material = ForceMaterial;
            }
        }

    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(_2dxFX_Wave)), CanEditMultipleObjects]
public class _2dxFX_Wave_Editor : Editor
{
    private SerializedObject m_object;

    public void OnEnable()
    {

        m_object = new SerializedObject(targets);
    }

    public override void OnInspectorGUI()
    {
        m_object.Update();
        DrawDefaultInspector();

        _2dxFX_Wave _2dxScript = (_2dxFX_Wave)target;

        Texture2D icon = Resources.Load("2dxfxinspector") as Texture2D;
        if (icon)
        {
            Rect r;
            float ih = icon.height;
            float iw = icon.width;
            float result = ih / iw;
            float w = Screen.width;
            result = result * w;
            r = GUILayoutUtility.GetRect(ih, result);
            EditorGUI.DrawTextureTransparent(r, icon);
        }

        EditorGUILayout.PropertyField(m_object.FindProperty("ActiveUpdate"), new GUIContent("Active Update", "Active Update, for animation / Animator only")); EditorGUILayout.PropertyField(m_object.FindProperty("ForceMaterial"), new GUIContent("Shared Material", "Use a unique material, reduce drastically the use of draw call"));

        if (_2dxScript.ForceMaterial == null)
        {
            _2dxScript.ActiveChange = true;
        }
        else
        {
            if (GUILayout.Button("Remove Shared Material"))
            {
                _2dxScript.ForceMaterial = null;
                _2dxScript.ShaderChange = 1;
                _2dxScript.ActiveChange = true;
                _2dxScript.CallUpdate();
            }

            EditorGUILayout.PropertyField(m_object.FindProperty("ActiveChange"), new GUIContent("Change Material Property", "Change The Material Property"));
        }

        if (_2dxScript.ActiveChange)
        {

            EditorGUILayout.BeginVertical("Box");

            Texture2D icone = Resources.Load("2dxfx-icon-clip_left") as Texture2D;

            EditorGUILayout.PropertyField(m_object.FindProperty("_OffsetX"), new GUIContent("Offset X", icone, "Change the offset of X"));
            icone = Resources.Load("2dxfx-icon-clip_right") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_OffsetY"), new GUIContent("Offset Y", icone, "Change the offset of Y"));
            icone = Resources.Load("2dxfx-icon-size_x") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_DistanceX"), new GUIContent("Distance X", icone, "Change the distance of X"));
            icone = Resources.Load("2dxfx-icon-size_y") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_DistanceY"), new GUIContent("Distance Y", icone, "Change the distance of Y"));
            icone = Resources.Load("2dxfx-icon-time") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_WaveTimeX"), new GUIContent("Wave Time X", icone, "Change the time speed of the wave X"));
            icone = Resources.Load("2dxfx-icon-time") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_WaveTimeY"), new GUIContent("Wave Time Y", icone, "Change the time speed of the wave Y"));
            icone = Resources.Load("2dxfx-icon-time") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("AutoPlayWaveX"), new GUIContent("Active AutoPlay Wave X", icone, "Active the time speed"));
            if (_2dxScript.AutoPlayWaveX)
            {
                icone = Resources.Load("2dxfx-icon-time") as Texture2D;
                EditorGUILayout.PropertyField(m_object.FindProperty("AutoPlaySpeedX"), new GUIContent("AutoPlay Speed X", icone, "Speed of the auto play X"));
            }
            icone = Resources.Load("2dxfx-icon-time") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("AutoPlayWaveY"), new GUIContent("Active AutoPlay Wave Y", icone, "Active the time speed"));
            if (_2dxScript.AutoPlayWaveY)
            {
                icone = Resources.Load("2dxfx-icon-time") as Texture2D;
                EditorGUILayout.PropertyField(m_object.FindProperty("AutoPlaySpeedY"), new GUIContent("AutoPlay Speed Y", icone, "Speed of the auto play Y"));
            }
            icone = Resources.Load("2dxfx-icon-pixel") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("AutoRandom"), new GUIContent("Auto Random", icone, "Active the random value"));
            if (_2dxScript.AutoRandom)
            {
                icone = Resources.Load("2dxfx-icon-value") as Texture2D;
                EditorGUILayout.PropertyField(m_object.FindProperty("AutoRandomRange"), new GUIContent("Auto Random Range", icone, "Change the random value"));
            }

            EditorGUILayout.BeginVertical("Box");



            icone = Resources.Load("2dxfx-icon-fade") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_Alpha"), new GUIContent("Fading", icone, "Fade from nothing to showing"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();


        }

        m_object.ApplyModifiedProperties();

    }
}
#endif
