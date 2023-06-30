//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/Demo Controls UI")]
	public class DemoControlsUI : MonoBehaviour
	{
		[SerializeField]
		AudioSource audioCom = null;
		float pitch = 1f;

		GUIStyle rightAlignStyle;

		void Start()
		{
			pitch = audioCom.pitch;
		}
		
		void OnGUI()
		{
			if (rightAlignStyle == null)
			{
				rightAlignStyle = GUI.skin.textField;
				rightAlignStyle.alignment = TextAnchor.MiddleRight;
			}

			GUILayout.BeginHorizontal(GUILayout.Width((float)Screen.width));
			{
				GUILayout.FlexibleSpace();

				// The style draws the background color.
				GUILayout.Label("Audio Pitch", GUI.skin.textField);

				GUILayout.BeginVertical();
				{
					GUILayout.Space(9f);
					pitch = GUILayout.HorizontalSlider(pitch, 0.2f, 3f, GUILayout.Width(200f));
				}
				GUILayout.EndVertical();

				GUILayout.Label(string.Format("{0:0.##}x", pitch), rightAlignStyle, GUILayout.Width(40f));

				if (GUILayout.Button("Reset"))
				{
					pitch = 1f;
				}

				audioCom.pitch = pitch;

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}
	}
}
