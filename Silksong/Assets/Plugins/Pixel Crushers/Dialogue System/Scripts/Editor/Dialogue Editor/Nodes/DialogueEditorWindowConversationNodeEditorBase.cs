using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window provides the base for
    /// a Mecanim-style editor window.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        private const int LeftMouseButton = 0;
        private const int RightMouseButton = 1;
        private const int MiddleMouseButton = 2;

        private const float MinorGridLineWidth = 12f;
        private const float MajorGridLineWidth = 120f;

        private readonly Color MinorGridLineColor = new Color(0f, 0f, 0f, 0.2f);
        private readonly Color MajorGridLineColor = new Color(0f, 0f, 0f, 0.4f);

        private const float CanvasSize = 100000f;

        [SerializeField]
        private Vector2 canvasScrollView = new Vector2(CanvasSize, CanvasSize);
        [SerializeField]
        private Vector2 canvasScrollPosition = Vector2.zero;

        private bool isDraggingCanvas = false;

        private Texture2D canvasBackgroundColorTexture = null;
        private Material linkArrowMaterial = null;

        private void GotoCanvasHomePosition()
        {
            canvasScrollPosition = Vector2.zero;
        }

        private void PageUpCanvas()
        {
            canvasScrollPosition.y = Mathf.Max(0, canvasScrollPosition.y - (position.height / _zoom) / 2);
        }

        private void PageDownCanvas()
        {
            canvasScrollPosition.y = Mathf.Max(0, canvasScrollPosition.y + (position.height / _zoom) / 2);
        }

        private void DrawCanvas()
        {
            // Make scrollbars invisible:
            GUIStyle verticalScrollbar = GUI.skin.verticalScrollbar;
            GUIStyle horizontalScrollbar = GUI.skin.horizontalScrollbar;
            GUI.skin.verticalScrollbar = GUIStyle.none;
            GUI.skin.horizontalScrollbar = GUIStyle.none;

            try
            {
                canvasScrollPosition = GUI.BeginScrollView(new Rect(0, 0, (1 / _zoom) * position.width, (1 / _zoom) * position.height), canvasScrollPosition, new Rect(0, 0, canvasScrollView.x, canvasScrollView.y), false, false);
                DrawCanvasBackground();
                DrawCanvasContents();
            }
            finally
            {
                GUI.EndScrollView();
                if (currentConversation != null)
                {
                    currentConversation.canvasScrollPosition = canvasScrollPosition;
                    currentConversation.canvasZoom = _zoom;
                }
            }

            //--- For debugging zoom:
            //EditorGUI.LabelField(new Rect(10, 60, 500, 30), "pos=" + canvasScrollPosition);
            //EditorGUI.LabelField(new Rect(10, 90, 500, 30), "mouse=" + Event.current.mousePosition);

            // Restore previous scrollbar style:
            GUI.skin.verticalScrollbar = verticalScrollbar;
            GUI.skin.horizontalScrollbar = horizontalScrollbar;
        }

        private void DrawCanvasBackground()
        {
            if (Event.current.type != EventType.Repaint) return;
            DrawCanvasColor();
            DrawGridLines(MinorGridLineWidth, MinorGridLineColor);
            DrawGridLines(MajorGridLineWidth, MajorGridLineColor);
        }

        private void DrawCanvasColor()
        {
            if (canvasBackgroundColorTexture == null)
            {
                canvasBackgroundColorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                canvasBackgroundColorTexture.SetPixel(0, 0, new Color(0.16f, 0.16f, 0.16f, 0.75f));
                canvasBackgroundColorTexture.Apply();
                canvasBackgroundColorTexture.hideFlags = HideFlags.HideAndDontSave;
            }
            GUI.DrawTexture(new Rect(0, 0, CanvasSize, CanvasSize), canvasBackgroundColorTexture, ScaleMode.StretchToFill);
        }

        private void DrawGridLines(float gridSize, Color gridColor)
        {
            Handles.color = gridColor;
            float maxX = scaledPosition.width + canvasScrollPosition.x;
            float maxY = scaledPosition.height + canvasScrollPosition.y;

            if (Mathf.Approximately(0, canvasScrollPosition.y))
            {
                Handles.DrawLine(new Vector2(0, 1), new Vector2(maxX, 1));
            }
            for (float x = 0; x < maxX; x += gridSize)
            {
                Handles.DrawLine(new Vector2(x, canvasScrollPosition.y), new Vector2(x, maxY));
            }
            for (float y = 0; y < maxY; y += gridSize)
            {
                if (y < canvasScrollPosition.y) continue;
                Handles.DrawLine(new Vector2(0, y), new Vector2(maxX, y));
            }
        }

        public void DrawLink(Vector3 start, Vector3 end, Color color, bool wide)
        {
            if (Event.current.type != EventType.Repaint) return;
            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
            Texture2D connectionTexture = (Texture2D)UnityEditor.Graphs.Styles.connectionTexture.image;
            Handles.color = color;
            Vector3 diff = (end - start);
            Vector3 direction = diff.normalized;
            Vector3 mid = ((0.5f * diff) + start) - (0.5f * cross);
            Vector3 center = mid + direction;
            if ((center.y - canvasScrollPosition.y - 3f) > 0)
            {
                DrawArrow(cross, direction, center, color);
            }
#if UNITY_2022_1_OR_NEWER
            if (start.y < canvasScrollPosition.y && end.y < canvasScrollPosition.y) return;
            ClipLine(ref start, ref end);
#endif
            Handles.DrawAAPolyLine(connectionTexture, wide ? 16f : 4f, new Vector3[] { start, end });
        }

        // Draws a special link style for cross-conversation links.
        public void DrawSpecialLink(Vector3 start, Vector3 end, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
#if UNITY_2022_1_OR_NEWER
            if (start.y < canvasScrollPosition.y && end.y < canvasScrollPosition.y) return;
            ClipLine(ref start, ref end);
#endif
            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
            Texture2D connectionTexture = (Texture2D)UnityEditor.Graphs.Styles.connectionTexture.image;
            Handles.color = color;
            Handles.DrawAAPolyLine(connectionTexture, 4f, new Vector3[] { start, end });
            Vector3 diff = (start - end);
            Vector3 direction = diff.normalized;
            if ((end.y - canvasScrollPosition.y - 3f) > 0)
            {
                DrawArrow(cross, direction, end, color);
            }
        }

        private void ClipLine(ref Vector3 start, ref Vector3 end)
        {
            // If we're not scrolled down, no need to clip:
            if (Mathf.Approximately(0, canvasScrollPosition.y)) return;

            // If both endpoints are below scroll position, no need to clip:
            if (start.y > canvasScrollPosition.y && end.y > canvasScrollPosition.y) return;

            if (start.y < end.y)
            {
                start = LineIntersection(start, end, new Vector3(-CanvasSize, canvasScrollPosition.y), new Vector3(CanvasSize, canvasScrollPosition.y));
            }
            else
            {
                end = LineIntersection(start, end, new Vector3(-CanvasSize, canvasScrollPosition.y), new Vector3(CanvasSize, canvasScrollPosition.y));
            }
        }

        public Vector3 LineIntersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            Vector2 s1;
            Vector2 s2;
            s1.x = B.x - A.x;
            s1.y = B.y - A.y;
            s2.x = D.x - C.x;
            s2.y = D.y - C.y;
            float s = ((-1 * s1.y) * (A.x - C.x) + s1.x * (A.y - C.y)) / ((-1 * s2.x) * s1.y + s1.x * s2.y);
            float t = (s2.x * (A.y - C.y) - s2.y * (A.x - C.x)) / ((-1 * s2.x) * s1.y + s1.x * s2.y);
            var result = new Vector3(
              A.x + (t * s1.x),
              A.y + (t * s1.y));
            return result;
        }

        private void DrawArrow(Vector3 cross, Vector3 direction, Vector3 center, Color color)
        {
            const float sideLength = 6f;
            Vector3[] vertices = new Vector3[] {
                center + (direction * sideLength),
                (center - (direction * sideLength)) + (cross * sideLength),
                (center - (direction * sideLength)) - (cross * sideLength)
            };
            UseLinkArrowMaterial();
            GL.Begin(vertices.Length + 1);
            GL.Color(color);
            for (int i = 0; i < vertices.Length; i++)
            {
                GL.Vertex(vertices[i]);
            }
            GL.End();
        }

        private void UseLinkArrowMaterial()
        {
            if (linkArrowMaterial == null)
            {
                var shader = Shader.Find("Lines/Colored Blended") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse") ?? Shader.Find("Transparent/Diffuse");
                if (shader == null) return;
                linkArrowMaterial = new Material(shader);
            }
            linkArrowMaterial.SetPass(0);
        }

    }

}