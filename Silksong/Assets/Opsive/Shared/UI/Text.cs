namespace Opsive.Shared.UI
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A struct that allows you to choose whether to use TMP_Text or UnityEngine.UI.Text.
    /// </summary>
    [Serializable]
    public struct Text
    {
        //[Header("Select either Unity UI Text or TextMesh Pro Text.")]
        [Tooltip("Unity Engine UI Text.")]
        [SerializeField] private UnityEngine.UI.Text m_UnityText;

        public UnityEngine.UI.Text UnityText
        {
            get { return m_UnityText; }
            set => m_UnityText = value;
        }

#if TEXTMESH_PRO_PRESENT

        [Tooltip("Text Mesh Pro Text.")]
        [SerializeField] private TMPro.TMP_Text m_TextMeshProText;

        public TMPro.TMP_Text TextMeshProText
        {
            get { return m_TextMeshProText; }
            set => m_TextMeshProText = value;
        }
#endif

        /// <summary>
        /// The struct constructor.
        /// </summary>
        /// <param name="unityText">The unity text.</param>
        public Text(UnityEngine.UI.Text unityText)
        {
            m_UnityText = unityText;
    #if TEXTMESH_PRO_PRESENT
            m_TextMeshProText = null;
    #endif
        }

    #if TEXTMESH_PRO_PRESENT
        /// <summary>
        /// The struct constructor.
        /// </summary>
        /// <param name="textMeshProText">The text mesh pro tex.</param>
        public Text(TMPro.TMP_Text textMeshProText)
        {
            m_UnityText = null;
            m_TextMeshProText = textMeshProText;
        }
    #endif

        public string text {
            get {
    #if TEXTMESH_PRO_PRESENT
                return m_TextMeshProText?.text ?? m_UnityText?.text;
    #else
                return m_UnityText?.text ?? null;
    #endif
            }
            set => SetText(value);
        }

        public GameObject gameObject {
            get {
    #if TEXTMESH_PRO_PRESENT
                return m_TextMeshProText?.gameObject ?? m_UnityText?.gameObject;
    #else
                return m_UnityText?.gameObject;
    #endif
            }
        }

        public Color color {
            get {
    #if TEXTMESH_PRO_PRESENT
                return m_TextMeshProText?.color ?? m_UnityText?.color ?? Color.black;
    #else
                return m_UnityText?.color ?? Color.black;
    #endif
            }
            set => SetColor(value);
        }

        /// <summary>
        /// Set the text of the components.
        /// </summary>
        /// <param name="newText">The new text.</param>
        public void SetText(string newText)
        {
    #if TEXTMESH_PRO_PRESENT
            if (m_TextMeshProText != null) { m_TextMeshProText.text = newText; return; }
    #endif
            if (m_UnityText != null) { m_UnityText.text = newText; return; }
        }

        /// <summary>
        /// Set the text of the components.
        /// </summary>
        /// <param name="newColor">The new text.</param>
        public void SetColor(Color newColor)
        {
    #if TEXTMESH_PRO_PRESENT
            if (m_TextMeshProText != null) { m_TextMeshProText.color = newColor; return; }
    #endif
            if (m_UnityText != null) { m_UnityText.color = newColor; return; }
        }
    }
}