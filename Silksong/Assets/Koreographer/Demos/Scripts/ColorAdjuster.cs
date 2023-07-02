//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2020 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.Demos
{
	[AddComponentMenu("Koreographer/Demos/Color Adjuster")]
	public class ColorAdjuster : MonoBehaviour
	{
		[EventID]
		public string eventID;
		public Renderer[] objectsToColor;
		
		void Start()
		{
			// Register for Koreography Events.  This sets up the callback.
			Koreographer.Instance.RegisterForEventsWithTime(eventID, AdjustColor);
		}
		
		void OnDestroy()
		{
			// Sometimes the Koreographer Instance gets cleaned up before hand.
			//  No need to worry in that case.
			if (Koreographer.Instance != null)
			{
				Koreographer.Instance.UnregisterForAllEvents(this);
			}
		}
		
		void AdjustColor(KoreographyEvent evt, int sampleTime, int sampleDelta, DeltaSlice deltaSlice)
		{
			// We have prepared two kinds of events that work with this system:
			//  1) OneOffs that store a Color.
			//  2) Spans that store a Gradient.
			// Ensure that we have the correct types before proceeding!
			if (evt.IsOneOff() && evt.HasColorPayload())
			{
				// This is a simple Color Payload.
				Color targetColor = evt.GetColorValue();
				ApplyColorToObjects(ref targetColor);
			}
			else if (!evt.IsOneOff() && evt.HasGradientPayload())
			{
				// Access the color specified at the current music-time.  This is what
				//  drives musical color animations from gradients!
				Color targetColor = evt.GetColorOfGradientAtTime(sampleTime);
				ApplyColorToObjects(ref targetColor);
			}
		}

		void ApplyColorToObjects(ref Color color)
		{
			for (int i = 0; i < objectsToColor.Length; ++i)
			{
				objectsToColor[i].material.color = color;
			}
		}
	}
}
