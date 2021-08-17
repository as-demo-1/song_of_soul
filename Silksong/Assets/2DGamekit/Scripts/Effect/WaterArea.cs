using System.Collections;
using System.Collections.Generic;
using Gamekit2D;
using UnityEngine;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamekit2D
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class WaterArea : MonoBehaviour
    {
        protected struct WaterColumn
        {
            public float currentHeight;
            public float baseHeight;
            public float velocity;
            public float xPosition;
            public int vertexIndex;
        }

        const float NEIGHBOUR_TRANSFER = 0.001f;

        public int pointPerUnits = 5;
        public Vector2 offset;
        public Vector2 size = new Vector2(6f, 2f);

        public int sortingLayerID;
        public int sortingLayerOrder = 3;

        public float dampening = 0.93f;
        public float tension = 0.025f;
        public float neighbourTransfer = 0.03f;

        public RandomAudioPlayer splashPlayerPrefab;

        public BoxCollider2D boxCollider2D
        {
            get { return m_BoxCollider; }
        }

        protected MeshRenderer m_Renderer;
        protected MeshFilter m_Filter;
        protected Mesh m_Mesh;

        protected BoxCollider2D m_BoxCollider;
        protected Damager m_Damager;
        protected ParticleSystem m_Bubbles;
        protected ParticleSystem m_Steam;
        protected BuoyancyEffector2D m_BuoyancyEffector;

        protected WaterColumn[] m_Columns;
        protected float m_Width;
        protected Vector2 m_LowerCorner;

        protected RandomAudioPlayer[] m_SplashSourcePool;
        protected int m_CurrentSplashPlayer = 0;
        readonly int m_SplashPlayerPoolSize = 5;
        protected Vector3[] meshVertices;

        private void OnEnable()
        {
            GetReferences();

            m_BoxCollider.isTrigger = true;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                //we don't want to do it in editor when not playing as it would leak object
#endif
                if (splashPlayerPrefab != null)
                {
                    m_SplashSourcePool = new RandomAudioPlayer[m_SplashPlayerPoolSize];
                    for (int i = 0; i < m_SplashPlayerPoolSize; ++i)
                    {
                        m_SplashSourcePool[i] = Instantiate(splashPlayerPrefab);
                        m_SplashSourcePool[i].transform.SetParent(transform);
                        m_SplashSourcePool[i].gameObject.SetActive(false);
                    }
                }
#if UNITY_EDITOR
            }
#endif
            AdjustComponentSizes();
            RecomputeMesh();
            SetSortingLayer();

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                //we don't want to do it in editor when not playing as it would leak object
#endif
                m_Bubbles.Play();
                m_Steam.Play();
#if UNITY_EDITOR
            }
#endif
                

            meshVertices = m_Mesh.vertices;
        }

        public void GetReferences()
        {
            m_Renderer = GetComponent<MeshRenderer>();
            m_Filter = GetComponent<MeshFilter>();

            m_BoxCollider = GetComponent<BoxCollider2D>();

            m_Damager = GetComponent<Damager>();
            m_Bubbles = transform.Find("Bubbles").GetComponent<ParticleSystem>();
            m_Steam = transform.Find("Steam").GetComponent<ParticleSystem>();
            m_BuoyancyEffector = GetComponent<BuoyancyEffector2D>();
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < m_Columns.Length; ++i)
            {
                //float ratio = ((float)i) / m_columns.Length;

                float leftDelta = 0;
                if (i > 0)
                    leftDelta = neighbourTransfer * (m_Columns[i - 1].currentHeight - m_Columns[i].currentHeight);

                float rightDelta = 0;
                if (i < m_Columns.Length - 1)
                    rightDelta = neighbourTransfer * (m_Columns[i + 1].currentHeight - m_Columns[i].currentHeight);

                float force = leftDelta;
                force += rightDelta;
                force += tension * (m_Columns[i].baseHeight - m_Columns[i].currentHeight);

                m_Columns[i].velocity = dampening * m_Columns[i].velocity + force;

                m_Columns[i].currentHeight += m_Columns[i].velocity;
            }

            for (int i = 0; i < m_Columns.Length; ++i)
            {
                meshVertices[m_Columns[i].vertexIndex].y = m_Columns[i].currentHeight;
            }

            m_Mesh.vertices = meshVertices;

            m_Mesh.UploadMeshData(false);
        }

        public void AdjustComponentSizes()
        {
            ParticleSystem.ShapeModule steamShape = m_Steam.shape;
            steamShape.radius = size.x * 0.5f;

            Vector3 steamLocalPosition = m_Steam.transform.localPosition;
            steamLocalPosition = offset + Vector2.up * size.y * 0.5f;
            m_Steam.transform.localPosition = steamLocalPosition;

            ParticleSystem.ShapeModule bubblesShape = m_Bubbles.shape;
            bubblesShape.radius = size.x * 0.5f;

            Vector3 bubblesLocalPosition = m_Bubbles.transform.localPosition;
            bubblesLocalPosition = offset + Vector2.down * size.y * 0.5f;
            m_Bubbles.transform.localPosition = bubblesLocalPosition;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                //we don't want to do it in editor when not playing as it would leak object
#endif
                m_Steam.Simulate(0.1f);
                m_Bubbles.Simulate(0.1f);
#if UNITY_EDITOR
            }
#endif

            m_Damager.size = size;
            m_Damager.offset = offset;

            m_BoxCollider.size = size;
            m_BoxCollider.offset = offset;

            m_BuoyancyEffector.surfaceLevel = size.y * 0.5f - 0.84f;
        }

        public void RecomputeMesh()
        {
            //we recreate the mesh as we the previous one could come from prefab (and so every object would the same when they each need there...)
            //ref countign should take care of leaking, (and if it's a prefabed mesh, the prefab keep its mesh)
            m_Mesh = new Mesh();
            m_Mesh.name = "WaterMesh";
            m_Filter.sharedMesh = m_Mesh;

            m_LowerCorner = -(size * 0.5f - offset);

            m_Width = size.x;

            int count = Mathf.CeilToInt(size.x * (pointPerUnits - 1)) + 1;

            m_Columns = new WaterColumn[count + 1];

            float step = size.x / count;

            Vector3[] pts = new Vector3[(count + 1) * 2];
            Vector3[] normal = new Vector3[(count + 1) * 2];
            Vector2[] uvs = new Vector2[(count + 1) * 2];
            Vector2[] uvs2 = new Vector2[(count + 1) * 2];
            int[] indices = new int[6 * count];

            for (int i = 0; i <= count; ++i)
            {
                pts[i * 2 + 0].Set(m_LowerCorner.x + step * i, m_LowerCorner.y, 0);
                pts[i * 2 + 1].Set(m_LowerCorner.x + step * i, m_LowerCorner.y + size.y, 0);

                normal[i * 2 + 0].Set(0, 0, 1);
                normal[i * 2 + 1].Set(0, 0, 1);

                uvs[i * 2 + 0].Set(((float) i) / count, 0);
                uvs[i * 2 + 1].Set(((float) i) / count, 1);

                //Set the 2nd uv set to local position, allow for coherent tiling of normal map
                uvs2[i * 2 + 0].Set(pts[i * 2 + 0].x, pts[i * 2 + 0].y);
                uvs2[i * 2 + 1].Set(pts[i * 2 + 1].x, pts[i * 2 + 1].y);

                if (i > 0)
                {
                    int arrayIdx = (i - 1) * 6;
                    int startingIdx = (i - 1) * 2;

                    indices[arrayIdx + 0] = startingIdx;
                    indices[arrayIdx + 1] = startingIdx + 1;
                    indices[arrayIdx + 2] = startingIdx + 3;

                    indices[arrayIdx + 3] = startingIdx;
                    indices[arrayIdx + 4] = startingIdx + 3;
                    indices[arrayIdx + 5] = startingIdx + 2;
                }

                m_Columns[i] = new WaterColumn();
                m_Columns[i].xPosition = pts[i * 2].x;
                m_Columns[i].baseHeight = pts[i * 2 + 1].y;
                m_Columns[i].velocity = 0;
                m_Columns[i].vertexIndex = i * 2 + 1;
                m_Columns[i].currentHeight = m_Columns[i].baseHeight;
            }

            m_Mesh.Clear();

            m_Mesh.vertices = pts;
            m_Mesh.normals = normal;
            m_Mesh.uv = uvs;
            m_Mesh.uv2 = uvs2;
            m_Mesh.triangles = indices;

            meshVertices = m_Mesh.vertices;

            m_Mesh.UploadMeshData(false);
        }

        public void SetSortingLayer()
        {
            m_Renderer.sortingLayerID = sortingLayerID;
            m_Renderer.sortingOrder = sortingLayerOrder;
        }

        private void PlaySplash(Vector3 position)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (splashPlayerPrefab == null)
                return; //the splash prefab wasn't set, we don't have any instance of it to play sound

            int use = m_CurrentSplashPlayer;
            m_CurrentSplashPlayer = (m_CurrentSplashPlayer + 1) % m_SplashPlayerPoolSize;

            m_SplashSourcePool[use].transform.position = position;

            //disable/enable to force the effect to
            m_SplashSourcePool[use].gameObject.SetActive(false);
            m_SplashSourcePool[use].gameObject.SetActive(true);
            m_SplashSourcePool[use].PlayRandomSound();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb == null || rb.bodyType == RigidbodyType2D.Static)
                return; //we don't care about static rigidbody, they can't "fall" in water

            Bounds bounds = collision.bounds;

            List<int> touchedColumnIndices = new List<int>();
            float divisionWith = m_Width / m_Columns.Length;

            Vector3 localMin = transform.InverseTransformPoint(bounds.min);
            Vector3 localMax = transform.InverseTransformPoint(bounds.max);

            // find all our springs within the bounds
            var xMin = localMin.x;
            var xMax = localMax.x;


            PlaySplash(new Vector3(bounds.min.x + bounds.extents.x, bounds.min.y, bounds.min.z));

            for (var i = 0; i < m_Columns.Length; i++)
            {
                if (m_Columns[i].xPosition > xMin && m_Columns[i].xPosition < xMax)
                    touchedColumnIndices.Add(i);
            }

            // if we have no hits we should loop back through and find the 2 closest verts and use them
            if (touchedColumnIndices.Count == 0)
            {
                for (var i = 0; i < m_Columns.Length; i++)
                {
                    // widen our search to included divisitionWidth padding on each side so we definitely get a couple hits
                    if (m_Columns[i].xPosition + divisionWith > xMin && m_Columns[i].xPosition - divisionWith < xMax)
                        touchedColumnIndices.Add(i);
                }
            }

            float testForce = 0.2f;
            for (int i = 0; i < touchedColumnIndices.Count; ++i)
            {
                int idx = touchedColumnIndices[i];
                m_Columns[idx].velocity -= testForce;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WaterArea))]
    public class WaterAreaEditor : Editor
    {
        protected WaterArea m_WaterArea;
        protected Vector2 m_PrevSize;
        protected Vector2 m_PrevOffset;

        protected SerializedProperty m_OffsetProp;
        protected SerializedProperty m_SizeProp;
        protected SerializedProperty m_SortingLayerProp;
        protected SerializedProperty m_SortingLayerOrderProp;
        protected SerializedProperty m_SplashPlayerPrefabProp;

        protected string[] m_SortingLayers;
        protected int m_CurrentLayer;

        private void OnEnable()
        {
            m_WaterArea = target as WaterArea;
            m_WaterArea.GetReferences(); //needed for when you edit prefab directly, onEnable won't be called on it

            m_PrevSize = m_WaterArea.boxCollider2D.size;
            m_PrevOffset = m_WaterArea.boxCollider2D.offset;

            m_CurrentLayer = -1;
            int defaultLayer = 0;
            SortingLayer[] layers = SortingLayer.layers;
            m_SortingLayers = new string[layers.Length];
            for (int i = 0; i < layers.Length; ++i)
            {
                m_SortingLayers[i] = layers[i].name;
                if (layers[i].id == m_WaterArea.sortingLayerID)
                    m_CurrentLayer = i;

                if (layers[i].name == "Default")
                    defaultLayer = i;
            }

            if (m_CurrentLayer == -1)
            {
                m_WaterArea.sortingLayerID = SortingLayer.NameToID("Default");
                m_CurrentLayer = defaultLayer;
            }

            m_OffsetProp = serializedObject.FindProperty("offset");
            m_SizeProp = serializedObject.FindProperty("size");
            m_SortingLayerProp = serializedObject.FindProperty("sortingLayer");
            m_SortingLayerOrderProp = serializedObject.FindProperty("sortingLayerOrder");
            m_SplashPlayerPrefabProp = serializedObject.FindProperty("splashPlayerPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            m_WaterArea.pointPerUnits = EditorGUILayout.DelayedIntField("Point per unit", m_WaterArea.pointPerUnits);

            EditorGUI.BeginChangeCheck();

            Vector2 newOffsetValue = EditorGUILayout.Vector2Field("Offset", m_OffsetProp.vector2Value);
            if (newOffsetValue.x > 0f && newOffsetValue.y > 0f)
                m_OffsetProp.vector2Value = newOffsetValue;
            Vector2 newSizeValue = EditorGUILayout.Vector2Field("Size", m_SizeProp.vector2Value);
            if (newSizeValue.x > 0f && newSizeValue.y > 0f)
                m_SizeProp.vector2Value = newSizeValue;

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_WaterArea.AdjustComponentSizes();
                m_WaterArea.RecomputeMesh();
                EditorUtility.SetDirty(target);
            }

            m_WaterArea.dampening = EditorGUILayout.FloatField("Dampening", m_WaterArea.dampening);
            m_WaterArea.tension = EditorGUILayout.FloatField("Tension", m_WaterArea.tension);
            m_WaterArea.neighbourTransfer =
                EditorGUILayout.FloatField("Neighbour Transfer", m_WaterArea.neighbourTransfer);

            if (EditorGUI.EndChangeCheck() || (m_WaterArea.boxCollider2D != null &&
                                               (m_PrevSize != m_WaterArea.boxCollider2D.size ||
                                                m_PrevOffset != m_WaterArea.boxCollider2D.offset)))
            {
                m_WaterArea.RecomputeMesh();
                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            int sortLayer = EditorGUILayout.Popup("Sorting Layer", m_CurrentLayer, m_SortingLayers);
            if (sortLayer != m_CurrentLayer)
            {
                m_WaterArea.sortingLayerID = SortingLayer.NameToID(m_SortingLayers[sortLayer]);
                m_CurrentLayer = sortLayer;
            }

            EditorGUILayout.PropertyField(m_SortingLayerOrderProp);

            if (EditorGUI.EndChangeCheck())
            {
                m_WaterArea.SetSortingLayer();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.PropertyField(m_SplashPlayerPrefabProp);

            serializedObject.ApplyModifiedProperties();

            m_PrevSize = m_WaterArea.boxCollider2D.size;
            m_PrevOffset = m_WaterArea.boxCollider2D.offset;
        }
    }
#endif
}