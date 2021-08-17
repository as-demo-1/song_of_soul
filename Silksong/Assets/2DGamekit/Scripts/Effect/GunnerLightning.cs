using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerLightning : MonoBehaviour {

        public Transform end;

        public float updateInterval = 0.5f;

        public int pointCount = 10;
        public float randomOffset = 0.5f;

        Transform[] m_Branch;
        float m_UpdateTime = 0;
        Vector3[] m_Points;
        LineRenderer m_LineRenderer;

        // Use this for initialization
        void Start () {
            m_LineRenderer = GetComponent<LineRenderer> ();
            m_Points = new Vector3[pointCount];
            m_LineRenderer.positionCount = pointCount;
            m_LineRenderer.useWorldSpace = false;
        }

        void Update () {


            if (Time.time >= m_UpdateTime)
            {
                m_LineRenderer.positionCount = pointCount;

                m_Points [0] = Vector3.zero;
                Vector3 Segment = (end.position - transform.position) / (pointCount - 1);

                for(int i = 1; i < pointCount-1; i++){
                    m_Points [i] = Segment * i;
                    m_Points [i].y += Random.Range (-randomOffset, randomOffset);
                    m_Points [i].x += Random.Range (-randomOffset, randomOffset);
                }

                m_Points [pointCount - 1] = end.position - transform.position;
                m_LineRenderer.SetPositions (m_Points);

                m_UpdateTime += updateInterval;
            }


        }
    }
}