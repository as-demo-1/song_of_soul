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
[AddComponentMenu("2DxFX/Standard/4 Gradients")]
[System.Serializable]
public class _2dxFX_4Gradients : MonoBehaviour
{
    [HideInInspector] public Material ForceMaterial;
    [HideInInspector] public bool ActiveChange = true;
    private string shader = "2DxFX/Standard/4Gradients";
    [HideInInspector] public Color _Color1 = new Color(1f, 0f, 0f, 1f);
    [HideInInspector] public Color _Color2 = new Color(1f, 1f, 0f, 1f);
    [HideInInspector] public Color _Color3 = new Color(0f, 1f, 1f, 1f);
    [HideInInspector] public Color _Color4 = new Color(0f, 1f, 0f, 1f);
    [Range(0, 1)] [HideInInspector] public float _Alpha = 1f;

    [HideInInspector] public int ShaderChange = 0;
    Material tempMaterial;
    Material defaultMaterial;
    Image CanvasImage;
    SpriteRenderer CanvasSpriteRenderer; [HideInInspector] public bool ActiveUpdate = true;


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
        if (CanvasSpriteRenderer != null)
        {
            if (CanvasSpriteRenderer.sharedMaterial.shader.name == "Sprites/Default")
            {
                ForceMaterial.shader = Shader.Find(shader);
                ForceMaterial.hideFlags = HideFlags.None;
                CanvasSpriteRenderer.sharedMaterial = ForceMaterial;
            }
        }
        else if (CanvasImage != null)
        {
            Image img = CanvasImage;
            if (img.material == null)
            {
                ForceMaterial.shader = Shader.Find(shader);
                ForceMaterial.hideFlags = HideFlags.None;
                CanvasImage.material = ForceMaterial;
            }
        }
#endif
        if (ActiveChange)
        {

            if (CanvasSpriteRenderer != null)
            {
                CanvasSpriteRenderer.sharedMaterial.SetFloat("_Alpha", 1 - _Alpha);
                CanvasSpriteRenderer.sharedMaterial.SetColor("_Color1", _Color1);
                CanvasSpriteRenderer.sharedMaterial.SetColor("_Color2", _Color2);
                CanvasSpriteRenderer.sharedMaterial.SetColor("_Color3", _Color3);
                CanvasSpriteRenderer.sharedMaterial.SetColor("_Color4", _Color4);

            }
            else if (CanvasImage != null)
            {
                CanvasImage.material.SetFloat("_Alpha", 1 - _Alpha);
                CanvasImage.material.SetColor("_Color1", _Color1);
                CanvasImage.material.SetColor("_Color2", _Color2);
                CanvasImage.material.SetColor("_Color3", _Color3);
                CanvasImage.material.SetColor("_Color4", _Color4);
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
[CustomEditor(typeof(_2dxFX_4Gradients)), CanEditMultipleObjects]
public class _2dxFX_4Gradients_Editor : Editor
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
        _2dxFX_4Gradients _2dxScript = (_2dxFX_4Gradients)target;

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
        EditorGUILayout.PropertyField(m_object.FindProperty("ActiveUpdate"), new GUIContent("Active Update", "Active Update, for animation / Animator only")); 
        EditorGUILayout.PropertyField(m_object.FindProperty("ForceMaterial"), new GUIContent("Shared Material", "Use a unique material, reduce drastically the use of draw call"));

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
                _2dxScript._Color1 = new Color(1, 0, 0, 1);
                _2dxScript._Color2 = new Color(0, 0, 1, 1);
                _2dxScript._Color3 = new Color(0, 1, 0, 1);
                _2dxScript._Color4 = new Color(0, 1, 1, 1);
                _2dxScript.ActiveChange = true;
                _2dxScript.CallUpdate();
            }
            EditorGUILayout.PropertyField(m_object.FindProperty("ActiveChange"), new GUIContent("Change Material Property", "Change The Material Property"));
        }

        if (_2dxScript.ActiveChange)
        {
            EditorGUILayout.BeginVertical("Box");
            Texture2D icone = Resources.Load("2dxfx-icon-corner-1") as Texture2D;

            EditorGUILayout.PropertyField(m_object.FindProperty("_Color1"), new GUIContent("Upper Left Color", icone, "Select the color from upper left"));
            icone = Resources.Load("2dxfx-icon-corner-2") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_Color2"), new GUIContent("Upper Right Color", icone, "Select the color from upper right"));
            icone = Resources.Load("2dxfx-icon-corner-3") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_Color3"), new GUIContent("Bottom Left Color", icone, "Select the color from Bottom left"));
            icone = Resources.Load("2dxfx-icon-corner-4") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_Color4"), new GUIContent("Bottom Right Color", icone, "Select the color from Bottom right"));
            EditorGUILayout.BeginVertical("Box");
            icone = Resources.Load("2dxfx-icon-fade") as Texture2D;
            EditorGUILayout.PropertyField(m_object.FindProperty("_Alpha"), new GUIContent("Fading", icone, "Fade from nothing to showing"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            // PRESET FX
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(new GUIContent("PRESET FX", "PRESET FX"));
            EditorGUILayout.BeginHorizontal("Box");
            Texture2D preview = Resources.Load("2dxfx-p-4g1") as Texture2D;

            if (GUILayout.Button(preview))
            {
                _2dxScript._Color1 = new Color(1, 0, 0, 1);
                _2dxScript._Color2 = new Color(0, 0, 1, 1);
                _2dxScript._Color3 = new Color(0, 1, 0, 1);
                _2dxScript._Color4 = new Color(0, 1, 1, 1);
                _2dxScript._Alpha = 1;
                m_object.ApplyModifiedProperties();
                _2dxScript.CallUpdate();
            }

            preview = Resources.Load("2dxfx-p-4g2") as Texture2D;
            if (GUILayout.Button(preview))
            {
                _2dxScript._Color1 = new Color(0, 1, 0.7f, 0);
                _2dxScript._Color2 = new Color(0, 1, 0.7f, 0);
                _2dxScript._Color3 = new Color(0, 1, 0.7f, 1);
                _2dxScript._Color4 = new Color(0, 1, 0.7f, 1);
                _2dxScript._Alpha = 1;
                m_object.ApplyModifiedProperties();
                _2dxScript.CallUpdate();
            }

            preview = Resources.Load("2dxfx-p-4g3") as Texture2D;
            if (GUILayout.Button(preview))
            {
                _2dxScript._Color1 = new Color(1, 1, 0, 1);
                _2dxScript._Color2 = new Color(1, 0.8f, 0, 0);
                _2dxScript._Color3 = new Color(1, 0.6f, 0, 0);
                _2dxScript._Color4 = new Color(1, 0.6f, 0, 0);
                _2dxScript._Alpha = 1;
                m_object.ApplyModifiedProperties();
                _2dxScript.CallUpdate();
            }

            preview = Resources.Load("2dxfx-p-4g4") as Texture2D;
            if (GUILayout.Button(preview))
            {
                _2dxScript._Color1 = new Color(1, 0, 0, 1);
                _2dxScript._Color2 = new Color(1, 0, 0, 1);
                _2dxScript._Color3 = new Color(1, 0, 0, 0);
                _2dxScript._Color4 = new Color(1, 0, 0, 0);
                _2dxScript._Alpha = 1;
                m_object.ApplyModifiedProperties();
                _2dxScript.CallUpdate();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }

        m_object.ApplyModifiedProperties();

    }
}
#endif
