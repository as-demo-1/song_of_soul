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
[AddComponentMenu("2DxFX/Standard/Shiny Reflect")]
[System.Serializable]
public class _2dxFX_Shiny_Reflect : MonoBehaviour
{
    [HideInInspector] public Material ForceMaterial;
    [HideInInspector] public bool ActiveChange = true;
    [HideInInspector] public Texture2D __MainTex2;
    private string shader = "2DxFX/Standard/Shiny_Reflect";
    [HideInInspector] [Range(0, 1)] public float _Alpha = 1f;
    [HideInInspector] [Range(-0.5f, 1.5f)] public float Light = 1.0f;
    [HideInInspector] [Range(0.05f, 1f)] public float LightSize = 0.5f;
    [HideInInspector] public bool UseShinyCurve = true;
    [HideInInspector] public AnimationCurve ShinyLightCurve;

    [HideInInspector] [Range(0, 32)] public float AnimationSpeedReduction = 3f;
    [HideInInspector] [Range(0f, 2f)] public float Intensity = 1.0f;
    [HideInInspector] [Range(0f, 1f)] public float OnlyLight = 0.0f;
    [HideInInspector] [Range(-1f, 1f)] public float LightBump = 0.05f;
    private float ShinyLightCurveTime;



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
        __MainTex2 = Resources.Load("_2dxFX_Gradient") as Texture2D;
        ShaderChange = 0;
        if (CanvasSpriteRenderer != null)
        {
            CanvasSpriteRenderer.sharedMaterial.SetTexture("_MainTex2", __MainTex2);
        }
        else if (CanvasImage != null)
        {
            CanvasImage.material.SetTexture("_MainTex2", __MainTex2);
        }

        // VS AnimationCurve To C# for ShinyLightCurve
        // Put this code on 'Start' or 'Awake' fonction
        if (ShinyLightCurve == null) ShinyLightCurve = new AnimationCurve();

        if (ShinyLightCurve.length == 0)
        {
            ShinyLightCurve.AddKey(7.780734E-06f, -0.4416301f);
            ShinyLightCurve.keys[0].inTangent = 0f;
            ShinyLightCurve.keys[0].outTangent = 0f;

            ShinyLightCurve.AddKey(0.4310643f, 1.113406f);
            ShinyLightCurve.keys[1].inTangent = 0.2280953f;
            ShinyLightCurve.keys[1].outTangent = 0.2280953f;

            ShinyLightCurve.AddKey(0.5258899f, 1.229086f);
            ShinyLightCurve.keys[2].inTangent = -0.1474274f;
            ShinyLightCurve.keys[2].outTangent = -0.1474274f;

            ShinyLightCurve.AddKey(0.6136486f, 1.113075f);
            ShinyLightCurve.keys[3].inTangent = 0.005268873f;
            ShinyLightCurve.keys[3].outTangent = 0.005268873f;

            ShinyLightCurve.AddKey(0.9367767f, -0.4775873f);
            ShinyLightCurve.keys[4].inTangent = -3.890693f;
            ShinyLightCurve.keys[4].outTangent = -3.890693f;

            ShinyLightCurve.AddKey(1.144408f, -0.4976555f);
            ShinyLightCurve.keys[5].inTangent = 0f;
            ShinyLightCurve.keys[5].outTangent = 0f;

            ShinyLightCurve.postWrapMode = WrapMode.Loop;
            ShinyLightCurve.preWrapMode = WrapMode.Loop;

        }


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
                if (img.material == null) CanvasImage.material = ForceMaterial;
            }
            __MainTex2 = Resources.Load("_2dxFX_Gradient") as Texture2D;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial.SetTexture("_MainTex2", __MainTex2);
            }
            else if (CanvasImage != null)
            {
                Image img = CanvasImage;
                if (img.material == null) CanvasImage.material.SetTexture("_MainTex2", __MainTex2);
            }
        }
#endif
        if (ActiveChange)
        {
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Alpha", 1 - _Alpha);
                if (UseShinyCurve)
                {
                    if (ShinyLightCurve != null) CanvasSpriteRenderer.sharedMaterial.SetFloat("_Distortion", ShinyLightCurve.Evaluate(ShinyLightCurveTime));
                    ShinyLightCurveTime += (Time.deltaTime / 8) * AnimationSpeedReduction;
                }
                else
                {
                    CanvasSpriteRenderer.sharedMaterial.SetFloat("_Distortion", Light);
                }

                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Value2", LightSize);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Value3", Intensity);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Value4", OnlyLight);
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Value5", LightBump);
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material.SetFloat("_Alpha", 1 - _Alpha);
                if (UseShinyCurve)
                {
                    CanvasImage.material.SetFloat("_Distortion", ShinyLightCurve.Evaluate(ShinyLightCurveTime));
                    ShinyLightCurveTime += (Time.deltaTime / 8) * AnimationSpeedReduction;
                }
                else
                {
                    CanvasImage.material.SetFloat("_Distortion", Light);
                }

                CanvasImage.material.SetFloat("_Value2", LightSize);
                CanvasImage.material.SetFloat("_Value3", Intensity);
                CanvasImage.material.SetFloat("_Value4", OnlyLight);
                CanvasImage.material.SetFloat("_Value5", LightBump);
            }

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
            __MainTex2 = Resources.Load("_2dxFX_Gradient") as Texture2D;
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
            __MainTex2 = Resources.Load("_2dxFX_Gradient") as Texture2D;
        }

        if (__MainTex2)
        {
            __MainTex2.wrapMode = TextureWrapMode.Repeat;
            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial.SetTexture("_MainTex2", __MainTex2);
            }
            else if (CanvasImage != null)
            {
                CanvasImage.material.SetTexture("_MainTex2", __MainTex2);
            }
        }



    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(_2dxFX_Shiny_Reflect)), CanEditMultipleObjects]
public class _2dxFX_Shiny_Reflect_Editor : Editor
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

        _2dxFX_Shiny_Reflect _2dxScript = (_2dxFX_Shiny_Reflect)target;

        Texture2D icon = Resources.Load("2dxfxinspector-anim") as Texture2D;
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

            Texture2D icone = Resources.Load("2dxfx-icon-color") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("UseShinyCurve"), new GUIContent("Use Shiny Curve", "Change The Material Property"));

            if (_2dxScript.UseShinyCurve)
            {
                EditorGUILayout.PropertyField(m_object.FindProperty("ShinyLightCurve"), new GUIContent("Shiny Light Curve", icone, "Use Curve"));
                icone = Resources.Load("2dxfx-icon-time") as Texture2D;
                EditorGUILayout.PropertyField(m_object.FindProperty("AnimationSpeedReduction"), new GUIContent("Animation Speed Reduction", icone, "Change the speed of the animation based on the curve timeline"));
            }
            else
            {
                EditorGUILayout.PropertyField(m_object.FindProperty("Light"), new GUIContent("Shiny Light", icone, "Position of the Shine Light!"));
            }

            icone = Resources.Load("2dxfx-icon-color") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("LightSize"), new GUIContent("Shiny Light Size", icone, "Size of the Shine Light!"));
            EditorGUILayout.PropertyField(m_object.FindProperty("Intensity"), new GUIContent("Light Intensity", icone, "Intensity of the light"));
            EditorGUILayout.PropertyField(m_object.FindProperty("OnlyLight"), new GUIContent("Only Show Light", icone, "the value between the sprite and no sprite to show only the light"));
            EditorGUILayout.PropertyField(m_object.FindProperty("LightBump"), new GUIContent("Light Bump Intensity", icone, "the intensity of the light bump"));

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
